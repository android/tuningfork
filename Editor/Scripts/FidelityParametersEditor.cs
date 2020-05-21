//-----------------------------------------------------------------------
// <copyright file="FidelityParametersEditor.cs" company="Google">
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
using UnityEngine;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Google.Android.PerformanceTuner.Editor
{
    public class FidelityParametersEditor
    {
        Vector2 m_ScrollPosition = Vector2.zero;
        GUIStyle m_TextStyle;
        GUIStyle m_NumberStyle;
        GUIStyle m_HeaderStyle;
        GUIStyle m_EnumStyle;
        GUIStyle m_AddButtonStyle;

        readonly GUILayoutOption m_ColumnWidth = GUILayout.Width(130);
        readonly GUILayoutOption m_TrendingColumnWidth = GUILayout.Width(100);
        readonly Color m_GuiColour = GUI.color;

        static readonly string k_HelpInfo =
            "All quality levels are saved into " + Paths.androidAssetsPathName +
            "/dev_tuningfork_fidelityparams_*.bin files.\n" +
            "You should have at least one quality level.";

        readonly TrendHelper m_Helper = new TrendHelper();

        readonly ProjectData m_ProjectData;
        readonly DevDescriptor m_DevDescriptor;

        public FidelityParametersEditor(ProjectData projectData, DevDescriptor devDescriptor)
        {
            m_ProjectData = projectData;
            m_DevDescriptor = devDescriptor;
        }

        public void OnGUI()
        {
            CreateStyles();
            RenderAddButtonAndInfo();

            if (m_ProjectData.messages.Count == 0)
            {
                EditorGUILayout.HelpBox("You should have at least one quality level", MessageType.Error);
                return;
            }

            m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition, GUILayout.ExpandHeight(false));
            RenderHeaders();
            using (new GUILayout.HorizontalScope())
            {
                RenderFieldNames();
                RenderTrending();
                RenderMessages();
            }

            EditorGUILayout.EndScrollView();
        }

        void RenderMessages()
        {
            foreach (var pair in m_ProjectData.messages)
            {
                var index = pair.Key;
                var message = pair.Value;
                using (new GUILayout.VerticalScope(m_ColumnWidth))
                {
                    GUILayout.Space(5);
                    using (var check = new EditorGUI.ChangeCheckScope())
                    {
                        IMessage newMessage = RenderMessage(message);
                        if (check.changed)
                        {
                            m_ProjectData.RefreshFidelityParameters(newMessage, index);
                            break;
                        }
                    }

                    GUILayout.Space(15);
                    if (GUILayout.Button("Select as Default", m_ColumnWidth))
                    {
                        m_ProjectData.defaultFidelityParametersIndex = index;
                    }

                    using (new EditorGUI.DisabledGroupScope(m_ProjectData.messages.Count == 1))
                    {
                        if (GUILayout.Button("Delete Level", m_ColumnWidth))
                        {
                            m_ProjectData.DeleteFidelityParameters(index);
                            break;
                        }
                    }
                }
            }
        }

        void CreateStyles()
        {
            if (m_TextStyle == null)
                m_TextStyle = new GUIStyle(EditorStyles.boldLabel) {alignment = TextAnchor.MiddleCenter};
            if (m_NumberStyle == null)
                m_NumberStyle = new GUIStyle(EditorStyles.numberField) {alignment = TextAnchor.MiddleCenter};
            if (m_HeaderStyle == null)
                m_HeaderStyle = new GUIStyle(EditorStyles.boldLabel) {alignment = TextAnchor.MiddleRight};
            if (m_EnumStyle == null)
                m_EnumStyle = new GUIStyle(EditorStyles.popup) {alignment = TextAnchor.MiddleCenter};
            if (m_AddButtonStyle == null)
                m_AddButtonStyle = new GUIStyle(GUI.skin.button) {alignment = TextAnchor.MiddleRight};
        }

        void RenderAddButtonAndInfo()
        {
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(k_HelpInfo, Styles.info);
                if (GUILayout.Button("+ Add Level", Styles.button, GUILayout.ExpandWidth(false)))
                {
                    m_ProjectData.AddNewFidelityParameters();
                }
            }
        }

        IMessage RenderMessage(IMessage message)
        {
            foreach (var field in message.Descriptor.Fields.InDeclarationOrder())
            {
                object value = field.Accessor.GetValue(message);
                switch (field.FieldType)
                {
                    case FieldType.Int32:
                        value = EditorGUILayout.IntField((int) value, m_NumberStyle, m_ColumnWidth);
                        break;
                    case FieldType.Float:
                        value = (float) EditorGUILayout.FloatField((float) value, m_NumberStyle, m_ColumnWidth);
                        break;
                    case FieldType.Enum:
                        if ((int) value == 0) GUI.color = Color.yellow;
                        value = EditorGUILayout.EnumPopup((Enum) value, m_EnumStyle, m_ColumnWidth);
                        GUI.color = m_GuiColour;
                        break;
                }

                field.Accessor.SetValue(message, value);
            }

            return message;
        }

        void RenderHeaders()
        {
            using (new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Parameter name", m_HeaderStyle, m_ColumnWidth);
                EditorGUILayout.LabelField("Trend", m_TextStyle, m_TrendingColumnWidth);
                foreach (var index in m_ProjectData.messages.Keys)
                {
                    string name = index.ToString();
                    if (index == m_ProjectData.defaultFidelityParametersIndex) name += " (Default)";
                    EditorGUILayout.LabelField(name, m_TextStyle, m_ColumnWidth);
                }
            }

            EditorHelper.LineSeparator();
        }

        void RenderTrending()
        {
            using (new GUILayout.VerticalScope(m_TrendingColumnWidth))
            {
                GUILayout.Space(5);
                foreach (var trending in m_ProjectData.trends)
                {
                    EditorGUILayout.LabelField(m_Helper.icons[trending], m_TextStyle, m_TrendingColumnWidth);
                }
            }
        }

        void RenderFieldNames()
        {
            using (new GUILayout.VerticalScope(m_ColumnWidth))
            {
                GUILayout.Space(5);
                foreach (var fieldName in m_DevDescriptor.fidelityFieldNames)
                {
                    EditorGUILayout.LabelField(fieldName, m_HeaderStyle, m_ColumnWidth);
                }
            }
        }
    }
}