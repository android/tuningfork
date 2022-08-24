//-----------------------------------------------------------------------
// <copyright file="AddressablesSettingsEditor.cs" company="Google">
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

using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

// Disable warning for unused updateButtonString if addressables package is not present.
#pragma warning disable CS0414


namespace Google.Android.PerformanceTuner.Editor
{
    public class AddressablesSettingsEditor
    {
        private readonly SetupConfig m_SetupConfig;
        private readonly string updateButtonString = "Update Addressables Scenes";
        private readonly string errorMessage =
            "Only available in Unity 2019.3 or higher if the addressable package is present in the project.";
        private readonly string noAddressablesMessage =
            "There are currently no addressables scenes recorded by APT.";

        private readonly string resetAddressables = "Reset Addressables Scenes";

        private Vector2 scrollPos;
        public AddressablesSettingsEditor(SetupConfig setupConfig)
        {
            m_SetupConfig = setupConfig;
        }

        public void OnGUI()
        {
#if APT_ADDRESSABLE_PACKAGE_PRESENT && UNITY_2019_3_OR_NEWER
            if(!m_SetupConfig) return;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(updateButtonString, GUILayout.ExpandWidth(false)))
            {
                m_SetupConfig.UpdateAddressablesScenes();
                CompilationPipeline.RequestScriptCompilation(); // Requires 2019.3
            };
            EditorGUILayout.EndHorizontal();

            if(m_SetupConfig.AreAddressablesScenesPresent()){
                scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                for (int i = 0; i < m_SetupConfig.AddressablesScenes.Count; i++)
                {
                    EditorGUILayout.LabelField(
                        String.Format("{0}: {1}",
                            m_SetupConfig.AddressablesScenes[i].scenePath,
                            m_SetupConfig.AddressablesScenes[i].value));
                }
                EditorGUILayout.EndScrollView();
            }
            else
            {
                EditorGUILayout.HelpBox(noAddressablesMessage, MessageType.Info);
            }
            DisplayResetButton();
#else
            EditorGUILayout.HelpBox(errorMessage, MessageType.Warning);
#endif
        }

        private void DisplayResetButton()
        {
            // Move to bottom of window.
            EditorGUILayout.BeginVertical();
            GUILayout.FlexibleSpace();

            // Move towards the right.
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(resetAddressables))
            {
                ResetAddressablesWindow.ShowWindow(m_SetupConfig);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
    }
}
