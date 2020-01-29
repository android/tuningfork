//-----------------------------------------------------------------------
// <copyright file="Styles.cs" company="Google">
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

namespace Google.Android.PerformanceParameters.Editor
{
    /// <summary>
    /// Common styles across the plugin.
    /// </summary>
    public class Styles
    {
        private const int ButtonWidthMargin = 10;
        public static GUIStyle Button { get; private set; }
        public static GUIStyle Info { get; private set; }

        private static bool inited = false;

        /// <summary>
        /// Init all styles.
        /// Call it in OnGUI.
        /// </summary>
        public static void InitAllStyles()
        {
            if (inited) return;
            inited = true;
            Button = CreateButtonStyle();
            Info = CreateInfoStyle();
        }

        private static GUIStyle CreateInfoStyle()
        {
            return new GUIStyle(EditorStyles.wordWrappedLabel) {fontSize = 11};
        }

        private static GUIStyle CreateButtonStyle()
        {
            var padding = new RectOffset(ButtonWidthMargin, ButtonWidthMargin, GUI.skin.button.padding.top,
                GUI.skin.button.padding.bottom);
            return new GUIStyle(GUI.skin.button) {padding = padding};
        }
    }
}