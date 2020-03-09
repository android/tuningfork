//-----------------------------------------------------------------------
// <copyright file="AndroidPerformanceTunerWindow.cs" company="Google">
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

namespace Google.Android.PerformanceTuner.Editor
{
    public class AndroidPerformanceTunerWindow : EditorWindow
    {
        [MenuItem("Google/Android Performance Tuner")]
        public static void ShowWindow()
        {
            var window = EditorWindow.GetWindow(typeof(AndroidPerformanceTunerWindow),
                false, k_WindowName) as AndroidPerformanceTunerWindow;
            window.OnEnable();
        }

        const string k_WindowName =
#if UNITY_2018_3_OR_NEWER
            "Android Performance Tuner";
#else
            "Android Tuner";
#endif

        SetupConfig m_SetupConfig;

        SettingsEditor m_SettingsEditor;
        FidelityParametersEditor m_FidelityParametersEditor;
        AnnotationMessageEditor m_AnnotationMessageEditor;
        FidelityMessageEditor m_FidelityMessageEditor;
        InstrumentationSettingsEditor m_InstrumentationSettingsEditor;

        protected void OnEnable()
        {
            if (!Initializer.valid)
            {
                // Something went wrong - that should never happen.
                // In case it did happen - don't create UI  components as it will fail.
                return;
            }

            m_SetupConfig = FileUtil.LoadSetupConfig();

            m_SettingsEditor = new SettingsEditor(Initializer.projectData, m_SetupConfig);
            m_FidelityParametersEditor =
                new FidelityParametersEditor(Initializer.projectData, Initializer.devDescriptor);
            m_AnnotationMessageEditor = new AnnotationMessageEditor(m_SetupConfig, Initializer.protoFile,
                Initializer.devDescriptor.annotationMessage, Initializer.enumInfoHelper);
            m_FidelityMessageEditor = new FidelityMessageEditor(Initializer.projectData, m_SetupConfig,
                Initializer.protoFile,
                Initializer.devDescriptor.fidelityMessage, Initializer.enumInfoHelper);
            m_InstrumentationSettingsEditor =
                new InstrumentationSettingsEditor(Initializer.projectData);
        }

        readonly string[] m_ToolbarOptions = new string[5]
        {
            "Settings",
            "Annotation parameters",
            "Fidelity parameters",
            "Quality levels",
            "Instrumentation Settings"
        };

        readonly string[] m_Tooltips = new string[5]
        {
            null,
            null,
            null,
            "To create custom quality levels, use custom fidelity parameters.",
            null
        };

        readonly bool[] m_ToolbarEnabled = new bool[5] {true, true, true, true, true};
        int m_ToolbarSelected = 0;

        //TODO(b/120588304) Check if that color is ok.
        readonly Color m_HighlightColor = new Color(69f / 255f, 95f / 255f, 146f / 255f);
        const float k_MenuWidth = 160;
        const float k_Offset = 5;
        const float k_MenuButtonHeight = 20;

        void OnGUI()
        {
            Styles.InitAllStyles();

            if (!Initializer.valid)
            {
                // Initialization of plugins data failed. If this happens, don't display UI.
                // Display list of possible problems and how to solve it.
                // TODO(kseniia): add list of problems and how to solve it
                GUILayout.Space(k_Offset);
                EditorGUILayout.HelpBox(Initializer.initMessage, MessageType.Warning);
                return;
            }

            m_ToolbarEnabled[3] = m_SetupConfig.useAdvancedFidelityParameters;
            DrawMenu();
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Space(k_MenuWidth + k_Offset);
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Space(k_Offset);
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
            var rect = new Rect(k_Offset, 2 * k_Offset, k_MenuWidth, k_MenuButtonHeight);
            var highlightRect = new Rect(rect) {x = 0};
            for (int i = 0; i < m_ToolbarOptions.Length; ++i)
            {
                EditorGUI.BeginDisabledGroup(!m_ToolbarEnabled[i]);
                if (m_ToolbarSelected == i) EditorGUI.DrawRect(highlightRect, m_HighlightColor);
                if (GUI.Button(rect, new GUIContent(m_ToolbarOptions[i], m_Tooltips[i]), EditorStyles.label))
                {
                    m_ToolbarSelected = i;
                }

                EditorGUI.EndDisabledGroup();
                rect.y += k_MenuButtonHeight;
                highlightRect.y += k_MenuButtonHeight;
            }

            EditorGUI.DrawRect(new Rect(k_MenuWidth, 0, 1, position.height), Color.black);
        }
    }
}