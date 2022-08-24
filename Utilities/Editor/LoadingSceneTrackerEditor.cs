//-----------------------------------------------------------------------
// <copyright file="LoadingSceneTrackerEditor.cs" company="Google">
//
// Copyright 2022 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

// Enabled in Initializer after having generated the classes this script depends on.
// Delete this symbol from Unity if the APT Plugin is removed from the project.
#if ANDROID_PERFORMANCE_TUNER_UTILITIES
using System;
using System.Collections.Generic;
using System.Reflection;
using Google.Android.PerformanceTuner.Editor.Proto;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Google.Android.PerformanceTuner.Editor
{
    [CustomEditor(typeof(LoadingSceneTracker), /*editorForChildClasses*/true)]
    public class LoadingSceneTrackerEditor : UnityEditor.Editor
    {
        private SetupConfig setupConfig;
        private List<AnnotationFieldSaver> savedAnnotationFields;
        private bool updated = false;

        private void OnEnable()
        {
            updated = false;
        }

        public override void OnInspectorGUI()
        {
            if (!Initializer.valid)
            {
                updated = false;
                return;
            }
            if (!updated)
            {
                setupConfig = FileUtil.LoadSetupConfig();
                savedAnnotationFields = ((LoadingSceneTracker) target).annotationFields;
                UpdateSavedAnnotationFields();
                updated = true;
            }
            serializedObject.Update();

            DrawDefaultInspector(); // Draw LoadingTimeMetadata fields.

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Annotation parameters", EditorStyles.boldLabel);

            // Show the Enum Popups to insert the Annotation fields values.
            if (setupConfig.GetUseAdvanced(ProtoMessageType.Annotation))
            {
                for (int i = 0; i < savedAnnotationFields.Count; i++)
                {
                    PropertyInfo property = typeof(Annotation).GetProperty(savedAnnotationFields[i].fieldName);
                    if (property == null)
                    {
                        Debug.LogErrorFormat("No {0} property defined in Annotation.", 
                            savedAnnotationFields[i].fieldName);
                        return;
                    }
                    Type enumType = property.PropertyType;
                    dynamic savedValue = Enum.ToObject(enumType, savedAnnotationFields[i].value);
                    var newValue = EditorGUILayout.EnumPopup(savedAnnotationFields[i].fieldName, savedValue);
                    savedAnnotationFields[i].value = (int) newValue;
                    if (savedValue != newValue)
                    {
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }
                }

                EditorGUILayout.Space();

                var buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.alignment = TextAnchor.MiddleLeft;
                if (GUILayout.Button("Reset\nAnnotation", buttonStyle, GUILayout.ExpandWidth(false)))
                {
                    ResetAllValues();
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
            }
            else // Show an info message box when default annotation is enabled.
            {
                EditorGUILayout.HelpBox("No parameters to set when default annotations are enabled.",
                    MessageType.Info);
            }
            serializedObject.ApplyModifiedProperties();
        }

        // Updates the annotation saved in the LoadingSceneTracker to match the current annotation structure.
        private void UpdateSavedAnnotationFields()
        {
            bool sceneDirty = false;

            // Contains the annotation fields last saved in the inspector. Might differ from the ones in the current
            // annotation as it might have been changed from the APT Window.
            savedAnnotationFields = ((LoadingSceneTracker) target).annotationFields;
            if (savedAnnotationFields == null)
            {
                savedAnnotationFields = new List<AnnotationFieldSaver>();
            }

            // When using default annotations, clear the list so the LoadingSceneTracker creates an annotation
            // with only the scene field.
            if (!setupConfig.GetUseAdvanced(ProtoMessageType.Annotation))
            {
                savedAnnotationFields.Clear();
                return;
            }

            // Contains all the fields defined in the annotation.
            var annotationFields = new MessageInfo(Initializer.DevDescriptor.annotationMessage).fields;
            // Maps fieldName to FieldInfo.
            Dictionary<string, Google.Android.PerformanceTuner.Editor.Proto.FieldInfo> fieldsMap =
                CreateFieldDictionary(annotationFields);

            // Counting backwards as some elements might be removed.
            for (int i = savedAnnotationFields.Count - 1; i >= 0; i--)
            {
                // If both the field name and the field type coincide, then this saved field doesn't need updating and
                // can be removed from the map.
                if (fieldsMap.ContainsKey(savedAnnotationFields[i].fieldName) &&
                    fieldsMap[savedAnnotationFields[i].fieldName].enumType.Equals(savedAnnotationFields[i].enumName))
                {
                    fieldsMap.Remove(savedAnnotationFields[i].fieldName);
                }
                // Either the field name or the field type have changed. The saved field might possibly not be in the
                // annotation anymore. Remove it from the saved fields.
                else
                {
                    savedAnnotationFields.RemoveAt(i);
                    sceneDirty = true;
                }
            }
            // The remaining fields need to be added to the saved ones with default value.
            foreach(var key in fieldsMap.Keys)
            {
                savedAnnotationFields.Add(new AnnotationFieldSaver()
                {
                    enumName = fieldsMap[key].enumType,
                    fieldName = key,
                    value = 0
                });
                sceneDirty = true;
            }

            if (sceneDirty)
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }

        // Resets all the fields in the Annotation to 0.
        private void ResetAllValues()
        {
            for (int i = 0; i < savedAnnotationFields.Count; i++)
            {
                savedAnnotationFields[i].value = 0;
            }
        }

        private string SanitiseFieldName(string fieldName)
        {
            var tokens = fieldName.Split('_');
            for (int i = 0; i < tokens.Length; i++)
            {
                tokens[i] = Char.ToUpper(tokens[i][0]) + tokens[i].Substring(1);
            }
            return string.Concat(tokens);
        }

        private Dictionary<string, Google.Android.PerformanceTuner.Editor.Proto.FieldInfo> CreateFieldDictionary(List<Google.Android.PerformanceTuner.Editor.Proto.FieldInfo> fields)
        {
            Dictionary<string, Google.Android.PerformanceTuner.Editor.Proto.FieldInfo> fieldsMap = new Dictionary<string, Google.Android.PerformanceTuner.Editor.Proto.FieldInfo>();
            for (int i = 0; i < fields.Count; i++)
            {
                string sanitisedfieldName = SanitiseFieldName(fields[i].name);
                fieldsMap[sanitisedfieldName] = fields[i];
            }
            return fieldsMap;
        }
    }
}
#endif