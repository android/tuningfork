//-----------------------------------------------------------------------
// <copyright file="PerformanceParametersWindow.cs" company="Google">
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
    using UnityEditor;
    using UnityEngine;
    using UnityEditor.Callbacks;

    public class PerformanceParametersWindow : EditorWindow
    {
        [MenuItem("Android/Tuningfork")]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow(typeof(PerformanceParametersWindow),
                false, "Tuningfork") as PerformanceParametersWindow;
            window.OnEnable();
        }

        SettingsEditor m_SettingsEditor;
        FidelityParametersEditor m_FidelityParametersEditor;
        AnnotationMessageEditor m_AnnotationMessageEditor;
        FidelityMessageEditor m_FidelityMessageEditor;
        InstrumentationSettingsEditor m_InstrumentationSettingsEditor;
        SetupConfig m_SetupConfig;

        protected void OnEnable()
        {
            ProjectData projectData = new ProjectData();
            projectData.LoadFromStreamingAssets();

            m_SetupConfig = FileUtil.LoadSetupConfig();

            m_SettingsEditor = new SettingsEditor(projectData, m_SetupConfig);
            m_FidelityParametersEditor = new FidelityParametersEditor(projectData);
            m_AnnotationMessageEditor = new AnnotationMessageEditor(m_SetupConfig, EditorUtil.ProtoFile);
            m_FidelityMessageEditor = new FidelityMessageEditor(m_SetupConfig, EditorUtil.ProtoFile);
            m_InstrumentationSettingsEditor = new InstrumentationSettingsEditor(projectData, m_SetupConfig);
        }

        readonly string[] m_ToolbarOptions = new string[5]
        {
            "Settings",
            "Annotation parameters",
            "Fidelity parameters",
            "Fidelity levels",
            "Instrumentation Settings"
        };

        readonly string[] m_Tooltips = new string[5]
        {
            null,
            null,
            null,
            "To create custom fidelity levels, use custom fidelity parameters.",
            null
        };

        bool[] m_ToolbarEnabled = new bool[5] {true, true, true, true, true};
        int m_ToolbarSelected = 0;

        //TODO(b/120588304) Check if that color is ok.
        readonly Color m_HighlightColor = new Color(69f / 255f, 95f / 255f, 146f / 255f);
        readonly float m_MenuWidth = 160;
        readonly float m_Offset = 5;
        readonly float m_MenuButtonHeight = 20;

        void OnGUI()
        {
            Styles.InitAllStyles();
            m_ToolbarEnabled[3] = m_SetupConfig.UseAdvancedFidelityParameters;
            DrawMenu();
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(m_MenuWidth + m_Offset);
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Space(m_Offset);
                    GUILayout.Label(m_ToolbarOptions[m_ToolbarSelected],
                        new GUIStyle(EditorStyles.boldLabel) {fontSize = 16});

                    switch (m_ToolbarSelected)
                    {
                        case 0:
                            m_SettingsEditor.OnGUI();
                            break;
                        case 1:
                            m_AnnotationMessageEditor.OnGUI();
                            break;
                        case 2:
                            m_FidelityMessageEditor.OnGUI();
                            break;
                        case 3:
                            m_FidelityParametersEditor.OnGUI();
                            break;
                        case 4:
                            m_InstrumentationSettingsEditor.OnGUI();
                            break;
                    }
                }
            }
        }

        void DrawMenu()
        {
            Rect rect = new Rect(m_Offset, 2 * m_Offset, m_MenuWidth, m_MenuButtonHeight);
            Rect highlightRect = new Rect(rect) {x = 0};
            for (int i = 0; i < m_ToolbarOptions.Length; ++i)
            {
                EditorGUI.BeginDisabledGroup(!m_ToolbarEnabled[i]);
                if (m_ToolbarSelected == i) EditorGUI.DrawRect(highlightRect, m_HighlightColor);
                if (GUI.Button(rect, new GUIContent(m_ToolbarOptions[i], m_Tooltips[i]), EditorStyles.label))
                {
                    m_ToolbarSelected = i;
                }

                EditorGUI.EndDisabledGroup();
                rect.y += m_MenuButtonHeight;
                highlightRect.y += m_MenuButtonHeight;
            }

            EditorGUI.DrawRect(new Rect(m_MenuWidth, 0, 1, position.height), Color.black);
        }

        [DidReloadScripts]
        public static void RefreshAssetsCompleted()
        {
            Debug.Log("RefreshAssetsCompleted");
        }
    }
}