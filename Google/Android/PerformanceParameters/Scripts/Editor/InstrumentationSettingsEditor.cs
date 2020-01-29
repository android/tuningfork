//-----------------------------------------------------------------------
// <copyright file="InstrumentationSettingsEditor.cs" company="Google">
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
using UnityEditorInternal;
using UnityEngine;

namespace Google.Android.PerformanceParameters.Editor
{
    internal class InstrumentationSettingsEditor
    {
        private EditorStatePrefs<EditorState> m_EditorStatePrefs;

        private readonly ProjectData m_ProjectData;
        private readonly SetupConfig m_SetupConfig;

        private readonly string m_DefaultInfo =
            "       Use advanced settings only if you've found the default ones are not working for you.";

        private readonly EditorState m_EditorState;

        private Settings m_AdvancedSettings;

        private ReorderableList m_HistogramList;

        internal InstrumentationSettingsEditor(ProjectData projectData, SetupConfig setupConfig)
        {
            m_EditorStatePrefs =
                new EditorStatePrefs<EditorState>("instrumentation-settings", new EditorState()
                {
                    UseAdvanced = false,
                    JsonSettings = SettingsUtil.DefaultSettings.ToString()
                });
            m_ProjectData = projectData;
            m_EditorState = m_EditorStatePrefs.Get();
            m_SetupConfig = setupConfig;
            LoadFromCachedEditorStateOrData();
        }

        internal void OnGUI()
        {
            GUILayout.Space(15);
            using (var group = new EditorGUI.ChangeCheckScope())
            {
                m_EditorState.UseAdvanced = !GUILayout.Toggle(
                    !m_EditorState.UseAdvanced, "  Use default settings (recommended)", EditorStyles.radioButton);

                EditorGUILayout.LabelField(m_DefaultInfo, EditorStyles.wordWrappedLabel, GUILayout.ExpandWidth(true));
                GUILayout.Space(10);

                m_EditorState.UseAdvanced = GUILayout.Toggle(
                    m_EditorState.UseAdvanced, "  Use advanced settings", EditorStyles.radioButton);

                if (group.changed)
                {
                    if (m_EditorState.UseAdvanced)
                    {
                        LoadFromCachedEditorStateOrData();
                        m_AdvancedSettings = m_ProjectData.SetSettings(m_AdvancedSettings);
                    }

                    CacheSettings();
                }
            }

            if (!m_EditorState.UseAdvanced) return;

            GUILayout.Space(5);

            using (var group = new EditorGUI.ChangeCheckScope())
            {
                RenderAdvancedSettings();
                if (group.changed)
                {
                    m_AdvancedSettings = m_ProjectData.SetSettings(m_AdvancedSettings);
                    CacheSettings();
                }
            }


            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Reset to Default", Styles.Button, GUILayout.ExpandWidth(false)))
                {
                    m_AdvancedSettings = m_ProjectData.ResetSettingsToDefault();
                    CacheSettings();
                }
            }

            GUILayout.Space(10);
        }


        private void LoadFromCachedEditorStateOrData()
        {
            try
            {
                m_AdvancedSettings = Settings.Parser.ParseJson(m_EditorState.JsonSettings);
            }
            catch (Exception)
            {
                m_AdvancedSettings = m_ProjectData.GetSettings();
            }
        }

        private void CacheSettings()
        {
            m_EditorState.JsonSettings = m_AdvancedSettings.ToString();
            m_EditorStatePrefs.Set(m_EditorState);
        }

        private void RenderAdvancedSettings()
        {
            GUILayout.Label("Aggregation Strategy", EditorStyles.boldLabel);

            var newMethod = EditorGUILayout.Popup(
                "Submission method",
                (int) m_AdvancedSettings.AggregationStrategy.Method - 1,
                new string[2] {"Time based", "Tick based"});

            m_AdvancedSettings.AggregationStrategy.Method =
                (Settings.Types.AggregationStrategy.Types.Submission) (newMethod + 1);

            EditorGUI.indentLevel++;
            switch (m_AdvancedSettings.AggregationStrategy.Method)
            {
                case Settings.Types.AggregationStrategy.Types.Submission.TickBased:
                    m_AdvancedSettings.AggregationStrategy.IntervalmsOrCount =
                        EditorGUILayout.IntField("Count", m_AdvancedSettings.AggregationStrategy.IntervalmsOrCount);
                    break;
                case Settings.Types.AggregationStrategy.Types.Submission.TimeBased:
                    m_AdvancedSettings.AggregationStrategy.IntervalmsOrCount =
                        EditorGUILayout.IntField("Intervals (ms)",
                            m_AdvancedSettings.AggregationStrategy.IntervalmsOrCount);
                    break;
                case Settings.Types.AggregationStrategy.Types.Submission.Undefined:
                    EditorGUILayout.HelpBox("Choose Time based or Tick based ", MessageType.Error);
                    break;
            }

            EditorGUI.indentLevel--;
            GUILayout.Label("Histograms", EditorStyles.boldLabel);
            if (m_HistogramList == null) RebuildList();
            m_HistogramList.DoLayoutList();
        }

        private void RebuildList()
        {
            m_HistogramList = new ReorderableList(
                m_AdvancedSettings.Histograms, typeof(Settings.Types.Histogram), false, true, false, false);
            m_HistogramList.drawHeaderCallback = rect =>
            {
                rect.width /= 5;
                rect.x += rect.width;
                EditorGUI.LabelField(rect, "Instrument key");
                rect.x += rect.width;
                EditorGUI.LabelField(rect, new GUIContent("Bucket min (ms)", "Minimum bucket in milliseconds"));
                rect.x += rect.width;
                EditorGUI.LabelField(rect, new GUIContent("Bucket max (ms)", "Maximum bucket in milliseconds"));
                rect.x += rect.width;
                EditorGUI.LabelField(rect, "Number of buckets");
            };
            m_HistogramList.drawElementCallback = (rect, index, isActive, isFocused) =>
            {
                var histogram = m_AdvancedSettings.Histograms[index];
                var elementWidth = rect.width / 5;
                float offset = 15;
                rect.width = elementWidth - offset;
                EditorGUI.LabelField(rect, FindInstrumentationKeyName(histogram.InstrumentKey));
                rect.x += elementWidth;
                EditorGUI.LabelField(rect, histogram.InstrumentKey.ToString());
                rect.x += elementWidth;
                histogram.BucketMin = EditorGUI.FloatField(rect, histogram.BucketMin);
                rect.x += elementWidth;
                histogram.BucketMax = EditorGUI.FloatField(rect, histogram.BucketMax);
                rect.x += elementWidth;
                histogram.NBuckets = EditorGUI.IntField(rect, histogram.NBuckets);
                m_AdvancedSettings.Histograms[index] = histogram;
            };
        }

        private static string FindInstrumentationKeyName(int key)
        {
            foreach (InstrumentationKeys keyEnum in Enum.GetValues(typeof(InstrumentationKeys)))
                if ((int) keyEnum == key)
                    return keyEnum.ToString();

            return string.Empty;
        }

        [Serializable]
        public class EditorState
        {
            public string JsonSettings;
            public bool UseAdvanced;
        }
    }
}