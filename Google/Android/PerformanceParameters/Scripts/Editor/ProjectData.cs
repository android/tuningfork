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

namespace Google.Android.PerformanceParameters.Editor
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Google.Protobuf;
    using Google.Protobuf.Reflection;

    internal class ProjectData
    {
        internal void LoadFromStreamingAssets()
        {
            Settings = FileUtil.LoadSettings(SettingsUtil.DefaultSettings, SettingsUtil.DefaultAdjustFunction);
            try
            {
                defaultFidelityParametersIndex =
                    FileUtil.GetFidelityMessageIndex(Settings.DefaultFidelityParametersFilename);
            }
            catch (ArgumentException)
            {
                DefaultFidelityParametersIndex = 1;
            }

            Messages = FileUtil.LoadAllFidelityMessages();
            RefreshTrending();
        }

        private Settings Settings;
        internal Dictionary<int, IMessage> Messages;
        internal List<Trending> Trendings;

        internal readonly IList<FieldDescriptor> FPFields =
            MessageUtil.FidelityParamsDescriptor.Fields.InFieldNumberOrder();

        int defaultFidelityParametersIndex = 0;

        internal int DefaultFidelityParametersIndex
        {
            get { return defaultFidelityParametersIndex; }
            set
            {
                defaultFidelityParametersIndex = value;
                Settings.DefaultFidelityParametersFilename = FileUtil.GetFidelityMessageFilename(value);
                FileUtil.SaveSettings(Settings, SettingsUtil.DefaultAdjustFunction);
            }
        }

        internal string ApiKey
        {
            get { return Settings.ApiKey; }
            set
            {
                if (Settings.ApiKey.Equals(value)) return;
                Settings.ApiKey = value;
                FileUtil.SaveSettings(Settings, SettingsUtil.DefaultAdjustFunction);
            }
        }

        internal void RefreshFidelityParameters(IMessage message, int index)
        {
            Messages[index] = message;
            FileUtil.SaveFidelityMessage(message, index);
            RefreshTrending();
        }

        internal void AddNewFidelityParameters()
        {
            IMessage newMessage = MessageUtil.NewFidelityParams();
            int index = Messages.Count > 0 ? Messages.Keys.Max() + 1 : 1;
            Messages.Add(index, newMessage);
            FileUtil.SaveFidelityMessage(newMessage, index);
            RefreshTrending();
        }

        internal void DeleteFidelityParameters(int index)
        {
            Messages.Remove(index);
            FileUtil.SaveAllFidelityMessages(Messages.Values.ToList());
            Messages = FileUtil.LoadAllFidelityMessages();
            if (defaultFidelityParametersIndex == index)
            {
                DefaultFidelityParametersIndex = 1;
            }

            RefreshTrending();
        }

        internal Settings ResetSettingsToDefault()
        {
            SetSettings(SettingsUtil.DefaultSettings);
            // Don't edit settings directly, always return copy
            return Settings.Clone();
        }

        internal Settings GetSettings()
        {
            // Don't edit settings directly, always return copy.
            return Settings.Clone();
        }

        internal Settings SetSettings(Settings settings)
        {
            Settings.Histograms.Clear();
            Settings.Histograms.AddRange(settings.Histograms);
            Settings.AggregationStrategy = settings.AggregationStrategy;
            FileUtil.SaveSettings(Settings, SettingsUtil.DefaultAdjustFunction);
            // Don't edit settings directly, always return copy.
            return Settings.Clone();
        }

        private void RefreshTrending()
        {
            Trendings = new List<Trending>();
            foreach (var field in FPFields)
            {
                Trendings.Add(TrendingHelper.FindTrending(Messages.Values.ToList(), field));
            }
        }
    }
}