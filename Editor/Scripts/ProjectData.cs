//-----------------------------------------------------------------------
// <copyright file="ProjectData.cs" company="Google">
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
using System.Linq;
using System.Collections.Generic;
using Google.Protobuf;

namespace Google.Android.PerformanceTuner.Editor
{
    public class ProjectData
    {
        public void LoadFromStreamingAssets(DevDescriptor descriptor)
        {
            m_DevDescriptor = descriptor;
            m_Settings = FileUtil.LoadSettings(SettingsUtil.defaultSettings, m_DevDescriptor,
                SettingsUtil.DefaultAdjustFunction);

            if (AreSettingsDirty())
                FileUtil.SaveSettings(m_Settings, descriptor, SettingsUtil.DefaultAdjustFunction);

            try
            {
                m_DefaultFidelityParametersIndex =
                    FileUtil.GetFidelityMessageIndex(m_Settings.DefaultFidelityParametersFilename);
            }
            catch (ArgumentException)
            {
                defaultFidelityParametersIndex = 1;
            }

            messages = FileUtil.LoadAllFidelityMessages();
            RefreshTrends();
        }

        public Dictionary<int, IMessage> messages;
        public List<Trend> trends;
        Settings m_Settings;
        DevDescriptor m_DevDescriptor;
        int m_DefaultFidelityParametersIndex;

        public int defaultFidelityParametersIndex
        {
            get { return m_DefaultFidelityParametersIndex; }
            set
            {
                m_DefaultFidelityParametersIndex = value;
                m_Settings.DefaultFidelityParametersFilename = FileUtil.GetFidelityMessageFilename(value);
                FileUtil.SaveSettings(m_Settings, m_DevDescriptor, SettingsUtil.DefaultAdjustFunction);
            }
        }

        public string apiKey
        {
            get { return m_Settings.ApiKey; }
            set
            {
                if (m_Settings.ApiKey.Equals(value)) return;
                m_Settings.ApiKey = value;
                FileUtil.SaveSettings(m_Settings, m_DevDescriptor, SettingsUtil.DefaultAdjustFunction);
            }
        }

        public bool hasLoadingState
        {
            get { return m_Settings.LoadingAnnotationIndex > 0; }
        }

        /// <summary>
        ///     Check if settings has outdated annotation enum sizes.
        /// </summary>
        bool AreSettingsDirty()
        {
            var sizes = m_DevDescriptor.annotationEnumSizes;
            if (m_Settings.AggregationStrategy.AnnotationEnumSize.Count != sizes.Count)
                return true;

            for (int i = 0; i < sizes.Count; ++i)
                if (sizes[i] != m_Settings.AggregationStrategy.AnnotationEnumSize[i])
                    return true;
            return false;
        }

        public void RefreshFidelityParameters(IMessage message, int index)
        {
            messages[index] = message;
            FileUtil.SaveFidelityMessage(message, index);
            RefreshTrends();
        }

        public void AddNewFidelityParameters()
        {
            var newMessage = FidelityBuilder.builder.CreateNew();
            MessageUtil.AdjustEnumValues(newMessage);
            int index = messages.Count > 0 ? messages.Keys.Max() + 1 : 1;
            messages.Add(index, newMessage);
            FileUtil.SaveFidelityMessage(newMessage, index);
            RefreshTrends();
        }

        public void DeleteFidelityParameters(int index)
        {
            messages.Remove(index);
            FileUtil.SaveAllFidelityMessages(messages.Values.ToList());
            messages = FileUtil.LoadAllFidelityMessages();
            if (m_DefaultFidelityParametersIndex == index)
            {
                defaultFidelityParametersIndex = 1;
            }

            RefreshTrends();
        }

        public void DeleteAllFidelityMessages()
        {
            messages = new Dictionary<int, IMessage>();
            FileUtil.SaveAllFidelityMessages(messages.Values.ToList());
            RefreshTrends();
        }

        public Settings ResetSettingsToDefault()
        {
            SetSettings(SettingsUtil.defaultSettings);
            // Don't edit settings directly, always return copy
            return m_Settings.Clone();
        }

        public Settings GetSettings()
        {
            // Don't edit settings directly, always return copy.
            return m_Settings.Clone();
        }

        public Settings SetSettings(Settings settings)
        {
            m_Settings.Histograms.Clear();
            m_Settings.Histograms.AddRange(settings.Histograms);
            m_Settings.AggregationStrategy = settings.AggregationStrategy;
            FileUtil.SaveSettings(m_Settings, m_DevDescriptor, SettingsUtil.DefaultAdjustFunction);
            // Don't edit settings directly, always return copy.
            return m_Settings.Clone();
        }

        void RefreshTrends()
        {
            trends = new List<Trend>();
            if (messages.Count == 0) return;
            var msg = messages.Values.First(); // Any message from Messages will work
            foreach (var field in msg.Descriptor.Fields.InDeclarationOrder())
            {
                trends.Add(TrendHelper.FindTrend(messages.Values.ToList(), field));
            }
        }
    }
}