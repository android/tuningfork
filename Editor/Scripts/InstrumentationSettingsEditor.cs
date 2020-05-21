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
using System.Linq.Expressions;
using System.Text;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.Serialization;

namespace Google.Android.PerformanceTuner.Editor
{
    public class InstrumentationSettingsEditor
    {
        readonly EditorStatePrefs<EditorState> m_EditorStatePrefs;
        readonly ProjectData m_ProjectData;
        readonly EditorState m_EditorState;
        readonly String m_DefaultSettingsUiString;

        const string k_DefaultInfo =
            "       Use advanced settings only if you've found the default ones are not working for you.";

        Settings m_AdvancedSettings;
        ReorderableList m_HistogramList;

        public InstrumentationSettingsEditor(ProjectData projectData)
        {
            m_EditorStatePrefs =
                new EditorStatePrefs<EditorState>("instrumentation-settings", new EditorState()
                {
                    useAdvanced = false,
                    jsonSettings = SettingsUtil.defaultSettings.ToString()
                });
            m_ProjectData = projectData;
            m_EditorState = m_EditorStatePrefs.Get();
            m_DefaultSettingsUiString = ToUiString(SettingsUtil.defaultSettings);
            LoadFromCachedEditorStateOrData();
        }


        public void OnGUI()
        {
            GUILayout.Space(15);
            using (var group = new EditorGUI.ChangeCheckScope())
            {
                m_EditorState.useAdvanced = !GUILayout.Toggle(
                    !m_EditorState.useAdvanced, "  Use default settings (recommended)", EditorStyles.radioButton);

                ShowDefaultSettings();

                GUILayout.Space(10);

                m_EditorState.useAdvanced = GUILayout.Toggle(
                    m_EditorState.useAdvanced, "  Use advanced settings", EditorStyles.radioButton);

                if (group.changed)
                {
                    if (m_EditorState.useAdvanced)
                    {
                        LoadFromCachedEditorStateOrData();
                        m_AdvancedSettings = m_ProjectData.SetSettings(m_AdvancedSettings);
                    }
                    else
                    {
                        m_ProjectData.ResetSettingsToDefault();
                    }


                    CacheSettings();
                }
            }

            if (!m_EditorState.useAdvanced) return;

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
                if (GUILayout.Button("Reset to Default", Styles.button, GUILayout.ExpandWidth(false)))
                {
                    m_AdvancedSettings = m_ProjectData.ResetSettingsToDefault();
                    CacheSettings();
                }
            }

            GUILayout.Space(10);
        }


        bool m_DefaultSettingsFoldGroup = false;

        void ShowDefaultSettings()
        {
            EditorGUILayout.LabelField(k_DefaultInfo, EditorStyles.wordWrappedLabel, GUILayout.ExpandWidth(true));

            EditorGUI.indentLevel++;
            m_DefaultSettingsFoldGroup = EditorGUILayout.Foldout(m_DefaultSettingsFoldGroup, "Default settings");
            if (m_DefaultSettingsFoldGroup)
            {
                EditorGUILayout.LabelField("<i>These are default instrumentation settings</i>",
                    Styles.richWordWrappedLabel,
                    GUILayout.ExpandWidth(true));
                EditorGUILayout.LabelField(m_DefaultSettingsUiString, Styles.richWordWrappedLabel,
                    GUILayout.ExpandWidth(true));
            }

            EditorGUI.indentLevel--;
        }

        void LoadFromCachedEditorStateOrData()
        {
            try
            {
                m_AdvancedSettings = Settings.Parser.ParseJson(m_EditorState.jsonSettings);
            }
            catch (Exception)
            {
                m_AdvancedSettings = m_ProjectData.GetSettings();
            }
        }

        void CacheSettings()
        {
            m_EditorState.jsonSettings = m_AdvancedSettings.ToString();
            m_EditorStatePrefs.Set(m_EditorState);
        }

        void RenderAdvancedSettings()
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
                    float minutes = EditorGUILayout.Slider("Intervals (minutes)",
                        m_AdvancedSettings.AggregationStrategy.IntervalmsOrCount / 60000f /* from ms to minutes */,
                        0.5f /* 30 seconds */,
                        120f /* 2 hours */);
                    float milliseconds = 60000f /* from minutes to ms */ *
                                         Mathf.RoundToInt(minutes * 2f) / 2f /* round to have half-minute steps */;
                    m_AdvancedSettings.AggregationStrategy.IntervalmsOrCount = (int) milliseconds;
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

        void RebuildList()
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

        static string FindInstrumentationKeyName(int key)
        {
            foreach (InstrumentationKeys keyEnum in Enum.GetValues(typeof(InstrumentationKeys)))
                if ((int) keyEnum == key)
                    return keyEnum.ToString();

            return string.Empty;
        }

        /// <summary>
        /// Convert settings to a readable string to display in UI.
        /// </summary>
        static string ToUiString(Settings settings)
        {
            int minutes = settings.AggregationStrategy.IntervalmsOrCount /* ms */ / (60 * 1000);
            StringBuilder builder = new StringBuilder()
                .AppendFormat("       Aggregation Strategy\n")
                .AppendFormat("       Submission method: <b>{0}</b>\n",
                    settings.AggregationStrategy.Method)
                .AppendFormat("       Intervals: <b>{0} minutes</b>\n\n", minutes)
                .Append("       Histograms\n");

            builder.Append("       Instrument Key\tBucket min(ms)\tBucket max(ms)\tNumber of buckets\n");
            foreach (var histogram in settings.Histograms)
            {
                builder.AppendFormat("       {0}\t{1}{2}\t\t{3}\t\t{4}\n",
                    FindInstrumentationKeyName(histogram.InstrumentKey),
                    // Extra tab for 64002/64003 keys to align table.
                    histogram.InstrumentKey == 64002 || histogram.InstrumentKey == 64003 ? "\t" : "",
                    histogram.BucketMin,
                    histogram.BucketMax,
                    histogram.NBuckets);
            }

            return builder.ToString();
        }

        [Serializable]
        public class EditorState
        {
            [FormerlySerializedAs("JsonSettings")] public string jsonSettings;
            [FormerlySerializedAs("UseAdvanced")] public bool useAdvanced;
        }
    }
}