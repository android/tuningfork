//-----------------------------------------------------------------------
// <copyright file="Notification.cs" company="Google">
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
    public class Notification : EditorWindow
    {
        public static void Show(string title, string body)
        {
            var window = CreateInstance<Notification>();
            window.titleContent = new GUIContent(title);
            window.position = new Rect(Screen.width / 2f, Screen.height / 2f, 400, 200);
            window.m_Body = body;
            window.ShowUtility();
        }

        string m_Body;

        void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label(m_Body, EditorStyles.wordWrappedLabel);
            if (GUILayout.Button("Ok")) Close();
        }
    }
}