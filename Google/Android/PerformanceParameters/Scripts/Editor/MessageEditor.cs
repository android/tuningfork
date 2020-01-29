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

namespace Google.Android.PerformanceParameters.Editor
{
    using System;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;
    using System.Collections.Generic;
    using System.Linq;
    using Google.Protobuf.Reflection;
    using Google.Android.PerformanceParameters.Editor.Proto;

    public abstract class MessageEditor
    {
        [System.Serializable]
        public class EditorState
        {
            public List<FieldInfo> Fields = new List<FieldInfo>();
            public bool UseAdvanced = false;

            public EditorState()
            {
            }

            public EditorState(EditorState other)
            {
                UseAdvanced = other.UseAdvanced;
                Fields = new List<FieldInfo>(other.Fields);
            }

            public EditorState(bool useAdvanced, MessageInfo messageInfo)
            {
                this.UseAdvanced = useAdvanced;
                this.Fields = messageInfo.fields;
            }

            public bool HasChanges(EditorState other)
            {
                if (UseAdvanced != other.UseAdvanced) return true;
                if (!UseAdvanced) return false;
                if (!Fields.SequenceEqual(other.Fields)) return true;
                return false;
            }
        }

        public class Selection
        {
            internal int id;
            internal string name;

            internal Selection(int id, string name)
            {
                this.id = id;
                this.name = name;
            }
        }

        protected abstract string BasicInfo { get; }
        protected abstract string[] Headers { get; }

        ReorderableList m_FieldList;
        string m_ErrorMessage;
        readonly SetupConfig m_Config;
        private EditorStatePrefs<EditorState> m_EditorStatePrefs;
        EditorState m_EditorState;
        EditorState m_SavedEditorState;

        readonly ProtoMessageType m_MessageType;
        readonly MessageDescriptor m_Descriptor;
        readonly Action<MessageInfo> m_SaveMessage;
        private readonly MessageInfo m_DefaultMessage;
        private readonly FileInfo m_ProtoFile;

        bool HasChanges
        {
            get { return m_SavedEditorState.HasChanges(m_EditorState); }
        }

        internal MessageEditor(
            SetupConfig config,
            MessageDescriptor descriptor,
            ProtoMessageType messageType,
            MessageInfo defaultMessage,
            FileInfo protoFile)
        {
            this.m_Config = config;
            this.m_DefaultMessage = defaultMessage;
            this.m_MessageType = messageType;
            this.m_Descriptor = descriptor;
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
                m_EditorState.UseAdvanced = !GUILayout.Toggle(
                    !m_EditorState.UseAdvanced, "  Use default parameters", EditorStyles.radioButton);
                EditorGUILayout.LabelField(BasicInfo, EditorStyles.wordWrappedLabel);

                m_EditorState.UseAdvanced = GUILayout.Toggle(
                    m_EditorState.UseAdvanced, "  Use custom parameters", EditorStyles.radioButton);

                if (m_EditorState.UseAdvanced)
                {
                    if (m_FieldList == null) RebuildList();
                    m_FieldList.displayRemove = m_EditorState.Fields.Count > 1;

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

            if (HasChanges)
            {
                EditorGUILayout.HelpBox(
                    "You have unsaved changes.\n" +
                    "Note that saving changes will cause project to recompile.", MessageType.Info);
            }

            GUILayout.Space(5);
            using (new EditorGUI.DisabledGroupScope(!HasChanges))
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Revert", Styles.Button, GUILayout.ExpandWidth(false))) RevertState();
                using (new EditorGUI.DisabledGroupScope(!string.IsNullOrEmpty(m_ErrorMessage)))
                {
                    if (GUILayout.Button("Apply", Styles.Button, GUILayout.ExpandWidth(false))) SaveState();
                }
            }
        }


        void SaveState()
        {
            m_Config.SetUseAdvanced(m_EditorState.UseAdvanced, m_MessageType);
            EditorUtility.SetDirty(m_Config);
            if (m_EditorState.UseAdvanced)
            {
                m_ProtoFile.AddMessage(new MessageInfo()
                {
                    name = m_Descriptor.Name,
                    fields = m_EditorState.Fields
                });
            }
            else m_ProtoFile.AddMessage(m_DefaultMessage);


            m_SavedEditorState = new EditorState(m_Config.GetUseAdvanced(m_MessageType),
                new MessageInfo(m_Descriptor));
        }

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
            if (m_EditorState.UseAdvanced) m_ErrorMessage = GetValidationErrors();
            else m_ErrorMessage = string.Empty;
        }

        string GetValidationErrors()
        {
            string errors = string.Empty;
            if (m_EditorState.Fields.Count == 0)
                errors += "\n► You should have at least one parameter";
            if (m_EditorState.Fields.Any(x => string.IsNullOrEmpty(x.name)))
                errors += "\n► Parameter name should no be empty";
            if (m_EditorState.Fields.Select(x => x.name).Distinct().Count() != m_EditorState.Fields.Count())
                errors += "\n► Parameters names should be different";
            if (!string.IsNullOrEmpty(errors)) errors = "Please fix all errors before saving parameters: " + errors;
            return errors;
        }

        void RebuildList()
        {
            m_FieldList = new ReorderableList(m_EditorState.Fields, typeof(string), true, Headers.Count() > 0, true,
                true);
            m_FieldList.drawHeaderCallback = (Rect rect) =>
            {
                rect.x += 15;
                rect.width = (rect.width - 15) / Headers.Count();
                foreach (string header in Headers)
                {
                    EditorGUI.LabelField(rect, header);
                    rect.x += rect.width;
                }
            };
            m_FieldList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                m_EditorState.Fields[index] = RenderInfo(m_EditorState.Fields[index], rect, index);
            };
            m_FieldList.onAddCallback = (ReorderableList list) =>
            {
                m_EditorState.Fields.Add(new FieldInfo(m_Descriptor.File, m_MessageType, list.count));
            };
            m_FieldList.onRemoveCallback = (ReorderableList list) => { m_EditorState.Fields.RemoveAt(list.index); };
        }

        protected Selection RenderEnumSelection(Rect rect, int selected)
        {
            var originalEnumNames = MessageUtil.EnumHelper.GetAllNames();
            selected = Mathf.Clamp(selected, 0, originalEnumNames.Count() - 1);
            originalEnumNames.Add("► Add or update enum...");
            originalEnumNames.Add("► Delete enum...");
            string[] enumNames = originalEnumNames.ToArray();
            int addIndex = enumNames.Count() - 2;
            int deleteIndex = enumNames.Count() - 1;
            int newSelected = EditorGUI.Popup(rect, selected, enumNames);
            if (newSelected == addIndex)
            {
                NewEnumWindow.ShowWindow(m_ProtoFile);
                return new Selection(selected, enumNames[selected]);
            }

            if (newSelected == deleteIndex)
            {
                DeleteEnumWindow.ShowWindow(m_ProtoFile);
                return new Selection(selected, enumNames[selected]);
            }

            return new Selection(newSelected, enumNames[newSelected]);
        }
    }
}