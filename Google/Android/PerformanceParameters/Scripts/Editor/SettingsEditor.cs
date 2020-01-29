//-----------------------------------------------------------------------
// <copyright file="SettingsEditor.cs" company="Google">
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

namespace Google.Android.PerformanceParameters.Editor
{
    internal class SettingsEditor
    {
        private readonly ProjectData m_ProjectData;
        private readonly SetupConfig m_SetupConfig;

        private readonly string m_InfoText =
            "Settings are auto-saved into Google/Android/PerformanceParameters/Editor/AndroidAssets/tuningfork_settings.bin.\n" +
            "To get better frame rate in your game turn on frame pacing optimization in" +
            " Player->Resolution and Presentation->Optimized Frame Pacing.";


        private GUIContent m_PluginEnabled;
        private GUIContent m_PluginDisabled;

        internal SettingsEditor(ProjectData projectData, SetupConfig setupConfig)
        {
            m_ProjectData = projectData;
            m_SetupConfig = setupConfig;
        }

        internal void OnGUI()
        {
            LoadStyles();
            RenderPluginStatus();
            GUILayout.Space(10);
            GUILayout.Label(m_InfoText, Styles.Info);
            GUILayout.Space(10);
            m_ProjectData.ApiKey = EditorGUILayout.TextField("API key", m_ProjectData.ApiKey);
        }

        private void LoadStyles()
        {
            if (m_PluginEnabled == null)
                m_PluginEnabled = new GUIContent("Tuningfork is enabled", (Texture) Resources.Load("ic_done"));
            if (m_PluginDisabled == null)
                m_PluginDisabled =
                    new GUIContent("Tuningfork is disabled", (Texture) Resources.Load("ic_error_outline"));
        }

        private void RenderPluginStatus()
        {
            var label = m_SetupConfig.PluginEnabled ? m_PluginEnabled : m_PluginDisabled;

            GUILayout.Space(10);
            using (var group = new EditorGUI.ChangeCheckScope())
            {
                m_SetupConfig.PluginEnabled = EditorGUILayout.ToggleLeft(label, m_SetupConfig.PluginEnabled);
                if (group.changed) EditorUtility.SetDirty(m_SetupConfig);
            }
        }
    }
}