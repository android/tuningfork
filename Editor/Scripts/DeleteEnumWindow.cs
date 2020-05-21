//-----------------------------------------------------------------------
// <copyright file="DeleteEnumWindow.cs" company="Google">
//
// Copyright 2020 Google Inc. All Rights Reserved.
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

using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using Google.Protobuf.Reflection;
using Google.Android.PerformanceTuner.Editor.Proto;

namespace Google.Android.PerformanceTuner.Editor
{
    public class DeleteEnumWindow : EditorWindow
    {
        public static void ShowWindow(FileInfo protoFile, FileDescriptor fileDescriptor, EnumInfoHelper enumInfoHelper)
        {
            var window = ScriptableObject.CreateInstance<DeleteEnumWindow>();
            window.titleContent = new GUIContent("Update / Delete Enum");
            window.m_ProtoFile = protoFile;
            window.m_FileDescriptor = fileDescriptor;
            window.m_EnumInfoHelper = enumInfoHelper;
            window.position = new Rect(Screen.width / 2f, Screen.height / 2f, 400, 200);
            window.ShowUtility();
        }

        string m_ErrorMessage;
        int m_EnumToDelete = -1;
        FileInfo m_ProtoFile;
        FileDescriptor m_FileDescriptor;
        EnumInfoHelper m_EnumInfoHelper;

        void OnGUI()
        {
            var enumNames = m_EnumInfoHelper.GetAllEditableNames().ToArray();
            if (enumNames.Length == 0)
            {
                EditorGUILayout.LabelField(
                    "Fidelity and Annotation messages don't contain any enums you can update or delete.",
                    EditorStyles.wordWrappedLabel);
                return;
            }

            if (m_EnumToDelete < 0)
            {
                m_EnumToDelete = 0;
                m_ErrorMessage = CheckForDeleteErrors(enumNames[m_EnumToDelete]);
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Select enum to update or delete");
            m_EnumToDelete = EditorGUILayout.Popup(m_EnumToDelete, enumNames);
            if (EditorGUI.EndChangeCheck())
            {
                m_ErrorMessage = CheckForDeleteErrors(enumNames[m_EnumToDelete]);
            }


            GUILayout.Space(10);
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Update Enum", Styles.button, GUILayout.ExpandWidth(false)))
                {
                    EnumInfo? enumInfoToUpdate = m_EnumInfoHelper.GetInfo(enumNames[m_EnumToDelete]);
                    if (enumInfoToUpdate.HasValue)
                    {
                        NewEnumWindow.ShowWindow(m_ProtoFile, m_EnumInfoHelper, enumInfoToUpdate.Value);
                    }
                    else
                    {
                        Debug.LogErrorFormat("Information for [{0}] enum was not found", enumNames[m_EnumToDelete]);
                        NewEnumWindow.ShowNewEnumWindow(m_ProtoFile, m_EnumInfoHelper);
                    }

                    this.Close();
                }
            }

            GUILayout.Space(10);

            if (!string.IsNullOrEmpty(m_ErrorMessage))
            {
                EditorGUILayout.HelpBox(m_ErrorMessage, MessageType.Error);
            }

            EditorGUI.BeginDisabledGroup(!string.IsNullOrEmpty(m_ErrorMessage));
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Delete Enum", Styles.button, GUILayout.ExpandWidth(false)))
                {
                    m_ProtoFile.DeleteEnum(enumNames[m_EnumToDelete]);
                    this.Close();
                }
            }

            EditorGUI.EndDisabledGroup();
        }

        string CheckForDeleteErrors(string enumToDelete)
        {
            string errors = string.Empty;
            foreach (var message in m_FileDescriptor.MessageTypes)
            {
                var fields = ContainsEnumToDelete(message, enumToDelete);
                if (fields.Count() > 0)
                {
                    errors += string.Format("\n► {0} message contains parameters with {1} type",
                        message.Name, enumToDelete);
                    foreach (var field in fields) errors += string.Format("\n      * {0}", field);
                }
            }

            if (!string.IsNullOrEmpty(errors))
                errors += string.Format(
                    "\n Delete all parameters with type {0} from annotation and fidelity messages before deleting enum",
                    enumToDelete);

            return errors;
        }

        IEnumerable<string> ContainsEnumToDelete(MessageDescriptor descriptor, string enumToDelete)
        {
            return descriptor.Fields
                .InDeclarationOrder()
                .Where(x => x.FieldType == FieldType.Enum && x.EnumType.Name == enumToDelete)
                .Select(x => x.Name);
        }
    }
}