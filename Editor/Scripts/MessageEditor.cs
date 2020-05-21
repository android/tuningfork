//-----------------------------------------------------------------------
// <copyright file="MessageEditor.cs" company="Google">
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

using System;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Reflection;
using Google.Android.PerformanceTuner.Editor.Proto;
using UnityEngine.Serialization;

namespace Google.Android.PerformanceTuner.Editor
{
    public abstract class MessageEditor
    {
        [System.Serializable]
        public class EditorState
        {
            [FormerlySerializedAs("Fields")] public List<FieldInfo> fields = new List<FieldInfo>();
            [FormerlySerializedAs("UseAdvanced")] public bool useAdvanced = false;

            public EditorState()
            {
            }

            public EditorState(EditorState other)
            {
                useAdvanced = other.useAdvanced;
                fields = new List<FieldInfo>(other.fields);
            }

            public EditorState(bool useAdvanced, MessageInfo messageInfo)
            {
                this.useAdvanced = useAdvanced;
                this.fields = messageInfo.fields;
            }

            public bool HasChanges(EditorState other)
            {
                if (useAdvanced != other.useAdvanced) return true;
                if (!useAdvanced) return false;
                if (!fields.SequenceEqual(other.fields)) return true;
                return false;
            }
        }

        public class Selection
        {
            public readonly int id;
            public readonly string name;

            public Selection(int id, string name)
            {
                this.id = id;
                this.name = name;
            }
        }

        protected abstract string basicInfo { get; }
        protected abstract string[] headers { get; }

        ReorderableList m_FieldList;
        string m_ErrorMessage;
        EditorState m_EditorState;
        EditorState m_SavedEditorState;

        readonly SetupConfig m_Config;
        readonly EditorStatePrefs<EditorState> m_EditorStatePrefs;
        readonly ProtoMessageType m_MessageType;
        readonly MessageDescriptor m_Descriptor;
        readonly Action<MessageInfo> m_SaveMessage;
        readonly MessageInfo m_DefaultMessage;
        readonly FileInfo m_ProtoFile;
        readonly EnumInfoHelper m_EnumInfoHelper;

        bool hasChanges
        {
            get { return m_SavedEditorState.HasChanges(m_EditorState); }
        }

        protected MessageEditor(
            SetupConfig config,
            MessageDescriptor descriptor,
            ProtoMessageType messageType,
            MessageInfo defaultMessage,
            FileInfo protoFile,
            EnumInfoHelper enumInfoHelper)
        {
            this.m_Config = config;
            this.m_DefaultMessage = defaultMessage;
            this.m_MessageType = messageType;
            this.m_Descriptor = descriptor;
            this.m_EnumInfoHelper = enumInfoHelper;
            this.m_EditorStatePrefs = new EditorStatePrefs<EditorState>(messageType.ToString(),
                new EditorState(m_Config.GetUseAdvanced(m_MessageType), defaultMessage));
            this.m_EditorState = m_EditorStatePrefs.Get();
            this.m_ProtoFile = protoFile;
            this.m_SavedEditorState =
                new EditorState(m_Config.GetUseAdvanced(m_MessageType), new MessageInfo(m_Descriptor));
            CheckValidationErrors();
        }

        public void OnGUI()
        {
            GUILayout.Space(15);
            using (var check = new EditorGUI.ChangeCheckScope())
            {
                m_EditorState.useAdvanced = !GUILayout.Toggle(
                    !m_EditorState.useAdvanced, "  Use default parameters", EditorStyles.radioButton);
                EditorGUILayout.LabelField(basicInfo, EditorStyles.wordWrappedLabel);

                m_EditorState.useAdvanced = GUILayout.Toggle(
                    m_EditorState.useAdvanced, "  Use custom parameters", EditorStyles.radioButton);

                if (m_EditorState.useAdvanced)
                {
                    if (m_FieldList == null) RebuildList();
                    m_FieldList.displayRemove = m_EditorState.fields.Count > 1;

                    m_FieldList.DoLayoutList();

                    GUILayout.Space(15);
                    if (!string.IsNullOrEmpty(m_ErrorMessage))
                    {
                        EditorGUILayout.HelpBox(m_ErrorMessage, MessageType.Error);
                        GUILayout.Space(5);
                    }
                }

                if (check.changed)
                {
                    CheckValidationErrors();
                    CacheState();
                }
            }

            if (hasChanges)
            {
                EditorGUILayout.HelpBox(
                    "You have unsaved changes.\n" +
                    "Note that saving changes will cause project to recompile.", MessageType.Info);
            }

            GUILayout.Space(5);
            using (new EditorGUI.DisabledGroupScope(!hasChanges))
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Revert", Styles.button, GUILayout.ExpandWidth(false))) RevertState();
                using (new EditorGUI.DisabledGroupScope(!string.IsNullOrEmpty(m_ErrorMessage)))
                {
                    if (GUILayout.Button("Apply", Styles.button, GUILayout.ExpandWidth(false))) SaveState();
                }
            }
        }


        void SaveState()
        {
            m_Config.SetUseAdvanced(m_EditorState.useAdvanced, m_MessageType);
            EditorUtility.SetDirty(m_Config);
            if (m_EditorState.useAdvanced)
            {
                m_ProtoFile.AddMessage(new MessageInfo()
                {
                    name = m_Descriptor.Name,
                    fields = m_EditorState.fields
                });
            }
            else m_ProtoFile.AddMessage(m_DefaultMessage);


            m_SavedEditorState = new EditorState(m_Config.GetUseAdvanced(m_MessageType),
                new MessageInfo(m_Descriptor));

            ApplyState(m_EditorState);
        }

        protected abstract void ApplyState(EditorState state);

        protected abstract FieldInfo RenderInfo(FieldInfo info, Rect rect, int index);

        void CacheState()
        {
            m_EditorStatePrefs.Set(m_EditorState);
        }

        void RevertState()
        {
            m_EditorState = new EditorState(m_SavedEditorState);
            CacheState();
            RebuildList();
        }

        void CheckValidationErrors()
        {
            if (m_EditorState.useAdvanced) m_ErrorMessage = GetValidationErrors();
            else m_ErrorMessage = string.Empty;
        }

        string GetValidationErrors()
        {
            string errors = string.Empty;
            if (m_EditorState.fields.Count == 0)
                errors += "\n► You should have at least one parameter";
            if (m_EditorState.fields.Any(x => string.IsNullOrEmpty(x.name)))
                errors += "\n► Parameter name should not be empty";
            if (m_EditorState.fields.Select(x => x.name).Distinct().Count() != m_EditorState.fields.Count())
                errors += "\n► Parameters names should be different";
            if (!string.IsNullOrEmpty(errors)) errors = "Please fix all errors before saving parameters: " + errors;
            return errors;
        }

        void RebuildList()
        {
            m_FieldList = new ReorderableList(m_EditorState.fields, typeof(string), true, headers.Count() > 0, true,
                true);
            m_FieldList.drawHeaderCallback = (Rect rect) =>
            {
                rect.x += 15;
                rect.width = (rect.width - 15) / headers.Count();
                foreach (string header in headers)
                {
                    EditorGUI.LabelField(rect, header);
                    rect.x += rect.width;
                }
            };
            m_FieldList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                m_EditorState.fields[index] = RenderInfo(m_EditorState.fields[index], rect, index);
            };
            m_FieldList.onAddCallback = (ReorderableList list) =>
            {
                m_EditorState.fields.Add(new FieldInfo(m_Descriptor.File, m_MessageType, list.count));
            };
            m_FieldList.onRemoveCallback = (ReorderableList list) => { m_EditorState.fields.RemoveAt(list.index); };
        }

        protected Selection RenderEnumSelection(Rect rect, int selected)
        {
            var originalEnumNames = m_EnumInfoHelper.GetAllNames();
            selected = Mathf.Clamp(selected, 0, originalEnumNames.Count() - 1);
            originalEnumNames.Add("► Add enum...");
            originalEnumNames.Add("► Update or delete enum...");
            string[] enumNames = originalEnumNames.ToArray();
            int addIndex = enumNames.Count() - 2;
            int deleteOrUpdateIndex = enumNames.Count() - 1;
            int newSelected = EditorGUI.Popup(rect, selected, enumNames);
            if (newSelected == addIndex)
            {
                NewEnumWindow.ShowNewEnumWindow(m_ProtoFile, m_EnumInfoHelper);
                return new Selection(selected, enumNames[selected]);
            }

            if (newSelected == deleteOrUpdateIndex)
            {
                DeleteEnumWindow.ShowWindow(m_ProtoFile, m_Descriptor.File, m_EnumInfoHelper);
                return new Selection(selected, enumNames[selected]);
            }

            return new Selection(newSelected, enumNames[newSelected]);
        }
    }
}