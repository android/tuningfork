//-----------------------------------------------------------------------
// <copyright file="ResetAddressablesWindow.cs" company="Google">
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

using UnityEditor;
using UnityEngine;

namespace Google.Android.PerformanceTuner.Editor
{
    public class ResetAddressablesWindow : EditorWindow
    {
#if APT_ADDRESSABLE_PACKAGE_PRESENT
        private readonly string warningMessage = "Are you sure you want to reset the Addressables Scenes?\n" +
                                                 "This operation can NOT be undone and will destroy all the current " +
                                                 "bindings between scenes and protobuf values.";

        private readonly string cancelButton = "Cancel";
        private readonly string resetButton = "Reset";
#endif
        SetupConfig setupConfig;

        public static void ShowWindow(SetupConfig config)
        {
            var window = ScriptableObject.CreateInstance<ResetAddressablesWindow>();
            window.titleContent = new GUIContent("Reset Addressables Scenes");
            window.position = new Rect(Screen.width / 2f, Screen.height / 2f, 400, 200);
            window.setupConfig = config;
            window.ShowUtility();
        }

        void OnGUI()
        {
#if APT_ADDRESSABLE_PACKAGE_PRESENT
            EditorGUILayout.HelpBox(warningMessage, MessageType.Warning);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(cancelButton))
            {
                Close();
                return;
            }
            GUILayout.Space(20);
            if (GUILayout.Button(resetButton) && setupConfig)
            {
                setupConfig.ResetAddressablesScenes();
                Close();
            }
#endif
        }
    }
}