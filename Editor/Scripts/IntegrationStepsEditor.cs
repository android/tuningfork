//-----------------------------------------------------------------------
// <copyright file="IntegrationStepsEditor.cs" company="Google">
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

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Google.Android.PerformanceTuner.Editor
{
    public class IntegrationStepsEditor
    {
        readonly ProjectData projectData;
        readonly SetupConfig setupConfig;

        public IntegrationStepsEditor(ProjectData projectData, SetupConfig setupConfig)
        {
            this.projectData = projectData;
            this.setupConfig = setupConfig;
        }

        public void OnGUI()
        {
            RenderPluginStatus();
        }


        readonly GUIContent contentPrivacy =
            new GUIContent("1) Review and, if necessary, update your app's Privacy Policy");

        readonly GUIContent contentFramePacing2019 = new GUIContent("2) Enable Optimized Frame Pacing in your project");

        readonly GUIContent contentFramePacing = new GUIContent(string.Format(
            "2) Unity version is {0}, to get advantage of Optimized Frame Pacing upgrade your project to 2019 LTS or newer",
            Application.unityVersion));

        readonly GUIContent contentInterval = new GUIContent("3) Set up a proper interval between telemetry uploads");

        readonly Dictionary<int, string> contentAggregation =
            new Dictionary<int, string>()
            {
                {
                    0,
                    "\tYour current aggregation strategy is unknown / {0}. " +
                    "Please set aggregation strategy in Instrumentation Settings"
                },
                {1, "\tYour current aggregation strategy is time based and interval is {0} min."},
                {2, "\tYour current aggregation strategy is tick based and count is {0} ticks"}
            };


        readonly GUIContent contentFidelity = new GUIContent("4) Define fidelity parameters and quality levels");

        readonly GUIContent contentFidelityDefault =
            new GUIContent("\tUnity quality levels are used as Android Performance Tuner quality levels.");

        readonly GUIContent contentAnnotations = new GUIContent("5) Define custom annotations");

        readonly GUIContent contentLoading = new GUIContent("6) Record when your game is performing loading events");

        readonly GUIContent contentDebug =
            new GUIContent(
                "7) Run your game in debug mode and validate the output using logcat or the Tuning Fork Monitor app");

        readonly GUIContent contentVitals =
            new GUIContent(
                "8) Confirm that you and other relevant game engineers have access to Android Vitals in the Google Play Console");


        void RenderPluginStatus()
        {
            // Step 1
            GUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(contentPrivacy, Styles.richWordWrappedLabel);
            if (GUILayout.Button("Privacy Policy", Styles.button, GUILayout.ExpandWidth(false)))
                Application.OpenURL(
                    "https://developer.android.com/games/sdk/performance-tuner/unity/enable-api#privacy");
            EditorGUILayout.EndHorizontal();

            // Step 2
            GUILayout.Space(5);
#if UNITY_2019_3_OR_NEWER
            EditorGUILayout.LabelField(contentFramePacing2019, Styles.richWordWrappedLabel);
#else
            EditorGUILayout.LabelField(contentFramePacing, Styles.richWordWrappedLabel);
#endif

            // Step 3
            GUILayout.Space(5);
            EditorGUILayout.LabelField(contentInterval, Styles.richWordWrappedLabel);
            var method = projectData.GetSettings().AggregationStrategy.Method;
            var intervalsOrCounts = projectData.GetSettings().AggregationStrategy.IntervalmsOrCount;
            EditorGUILayout.LabelField(string.Format(
                    contentAggregation[(int) method],
                    method == Settings.Types.AggregationStrategy.Types.Submission.TimeBased
                        ? (intervalsOrCounts / 60000f /* from ms to minutes */)
                        : intervalsOrCounts),
                Styles.richWordWrappedLabel);

            // Step 4
            GUILayout.Space(5);
            EditorGUILayout.LabelField(contentFidelity, Styles.richWordWrappedLabel);
            EditorGUILayout.LabelField(string.Format("\tYou are currently using {0} fidelity parameters",
                setupConfig.useAdvancedFidelityParameters ? "custom" : "default"), Styles.richWordWrappedLabel);
            if (setupConfig.useAdvancedFidelityParameters)
            {
                EditorGUILayout.LabelField(string.Format("\tNumber of quality levels is {0}",
                        projectData.messages.Count),
                    projectData.messages.Count == 0 ? Styles.warningLabel : Styles.richWordWrappedLabel);
            }
            else
            {
                EditorGUILayout.LabelField(contentFidelityDefault, Styles.richWordWrappedLabel);
            }


            // Step 5
            GUILayout.Space(5);
            EditorGUILayout.LabelField(contentAnnotations, Styles.richWordWrappedLabel);
            EditorGUILayout.LabelField(string.Format("\tYou are currently using {0} annotations",
                setupConfig.useAdvancedAnnotations ? "custom" : "default"), Styles.richWordWrappedLabel);


            // Step 6
            GUILayout.Space(5);
            EditorGUILayout.LabelField(contentLoading, Styles.richWordWrappedLabel);

            // Step 7
            GUILayout.Space(5);
            EditorGUILayout.LabelField(contentDebug, Styles.richWordWrappedLabel);

            // Step 8
            GUILayout.Space(5);
            EditorGUILayout.LabelField(contentVitals, Styles.richWordWrappedLabel);

            // Documentation and codelab
            GUILayout.Space(30);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Review full documentation", Styles.richWordWrappedLabel);
            if (GUILayout.Button("Documentation", Styles.button, GUILayout.ExpandWidth(false)))
                Application.OpenURL(
                    "https://developer.android.com/games/sdk/performance-tuner/unity/review-and-publish");
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Review codelab", Styles.richWordWrappedLabel);
            if (GUILayout.Button("Codelab", Styles.button, GUILayout.ExpandWidth(false)))
                Application.OpenURL(
                    "https://developer.android.com/codelabs/android-performance-tuner-unity");
            EditorGUILayout.EndHorizontal();
        }
    }
}