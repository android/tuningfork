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

using UnityEditor;
using UnityEngine;

namespace Google.Android.PerformanceTuner.Editor
{
    public class SettingsEditor
    {
        readonly ProjectData m_ProjectData;
        readonly SetupConfig m_SetupConfig;

        const string k_InfoText =
            "Settings are auto-saved into " +
            "AndroidPerformanceTuner/Editor/AndroidAssets/tuningfork_settings.bin.\n" +
            "To get better frame rate in your game turn on frame pacing optimization in" +
            " Player->Resolution and Presentation->Optimized Frame Pacing.";


        GUIContent m_PluginEnabled;
        GUIContent m_PluginDisabled;

        public SettingsEditor(ProjectData projectData, SetupConfig setupConfig)
        {
            m_ProjectData = projectData;
            m_SetupConfig = setupConfig;
        }

        public void OnGUI()
        {
            LoadStyles();
            RenderPluginStatus();
            GUILayout.Space(10);
            GUILayout.Label(k_InfoText, Styles.info);
            GUILayout.Space(10);
            m_ProjectData.apiKey = EditorGUILayout.TextField("API key", m_ProjectData.apiKey);
        }

        void LoadStyles()
        {
            if (m_PluginEnabled == null)
                m_PluginEnabled = new GUIContent("Android Performance Tuner is enabled", (Texture) Resources.Load("ic_done"));
            if (m_PluginDisabled == null)
                m_PluginDisabled =
                    new GUIContent("Android Performance Tuner is disabled", (Texture) Resources.Load("ic_error_outline"));
        }

        void RenderPluginStatus()
        {
            var label = m_SetupConfig.pluginEnabled ? m_PluginEnabled : m_PluginDisabled;

            GUILayout.Space(10);
            using (var group = new EditorGUI.ChangeCheckScope())
            {
                m_SetupConfig.pluginEnabled = EditorGUILayout.ToggleLeft(label, m_SetupConfig.pluginEnabled);
                if (group.changed) EditorUtility.SetDirty(m_SetupConfig);
            }
        }
    }
}