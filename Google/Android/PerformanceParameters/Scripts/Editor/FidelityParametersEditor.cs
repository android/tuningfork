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

namespace Google.Android.PerformanceParameters.Editor
{
    using System;
    using UnityEditor;
    using UnityEngine;
    using System.Reflection;
    using Google.Protobuf;
    using Google.Protobuf.Reflection;

    internal class FidelityParametersEditor
    {
        Vector2 m_ScrollPosition = Vector2.zero;
        GUIStyle m_TextStyle = null;
        GUIStyle m_NumberStyle = null;
        GUIStyle m_HeaderStyle = null;
        GUIStyle m_EnumStyle = null;
        GUIStyle m_AddButtonStyle = null;

        readonly GUILayoutOption m_ColumnWidth = GUILayout.Width(130);
        readonly GUILayoutOption m_TrendingColumnWidth = GUILayout.Width(100);
        readonly Color m_GuiColour = GUI.color;

        readonly string m_HelpInfo = "All fidelity levels are saved into " +
                                     "Google/Android/PerformanceParameters/Editor/AndroidAssets/dev_tuningfork_fidelityparams_*.bin files.\n" +
                                     "You should have at least one fidelity level.";

        readonly TrendingHelper m_Helper = new TrendingHelper();
        readonly GUIContent m_PlusIcon = EditorGUIUtility.IconContent("Toolbar Plus");

        readonly ProjectData m_ProjectData;

        internal FidelityParametersEditor(ProjectData projectData)
        {
            this.m_ProjectData = projectData;
        }

        internal void OnGUI()
        {
            CreateStyles();
            RenderAddButtonAndInfo();

            if (m_ProjectData.Messages.Count == 0)
            {
                EditorGUILayout.HelpBox("You should have at least one fidelity level", MessageType.Error);
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
            foreach (var pair in m_ProjectData.Messages)
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
                        m_ProjectData.DefaultFidelityParametersIndex = index;
                    }

                    using (new EditorGUI.DisabledGroupScope(m_ProjectData.Messages.Count == 1))
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
                EditorGUILayout.LabelField(m_HelpInfo, Styles.Info);
                if (GUILayout.Button("+ Add Level", Styles.Button, GUILayout.ExpandWidth(false)))
                {
                    m_ProjectData.AddNewFidelityParameters();
                }
            }
        }

        IMessage RenderMessage(IMessage message)
        {
            foreach (var field in m_ProjectData.FPFields)
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
                foreach (var index in m_ProjectData.Messages.Keys)
                {
                    string name = index.ToString();
                    if (index == m_ProjectData.DefaultFidelityParametersIndex) name += " (Default)";
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
                foreach (var trending in m_ProjectData.Trendings)
                {
                    EditorGUILayout.LabelField(m_Helper.Icons[trending], m_TextStyle, m_TrendingColumnWidth);
                }
            }
        }

        void RenderFieldNames()
        {
            using (new GUILayout.VerticalScope(m_ColumnWidth))
            {
                GUILayout.Space(5);
                foreach (var field in m_ProjectData.FPFields)
                {
                    EditorGUILayout.LabelField(field.Name, m_HeaderStyle, m_ColumnWidth);
                }
            }
        }
    }
}