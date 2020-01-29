//-----------------------------------------------------------------------
// <copyright file="SettingsUtil.cs" company="Google">
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

namespace Google.Android.PerformanceParameters.Editor
{
    public class SettingsUtil
    {
        // TODO(b/120588304): Add unity frame buckets.
        private static readonly List<Settings.Types.Histogram> DefaultHistograms = new List<Settings.Types.Histogram>()
        {
            new Settings.Types.Histogram()
            {
                InstrumentKey = (int) InstrumentationKeys.SYSCPU,
                BucketMin = 6.54f,
                BucketMax = 60f,
                NBuckets = 200,
            },
            new Settings.Types.Histogram()
            {
                InstrumentKey = (int) InstrumentationKeys.SYSGPU,
                BucketMin = 10,
                BucketMax = 40,
                NBuckets = 30,
            },
            new Settings.Types.Histogram()
            {
                InstrumentKey = (int) InstrumentationKeys.SWAPPY_SWAP_TIME,
                BucketMin = 0,
                BucketMax = 20,
                NBuckets = 30,
            },
            new Settings.Types.Histogram()
            {
                InstrumentKey = (int) InstrumentationKeys.SWAPPY_WAIT_TIME,
                BucketMin = 0,
                BucketMax = 20,
                NBuckets = 30,
            }
        };

        private static readonly Settings.Types.AggregationStrategy DefaultAggregation =
            new Settings.Types.AggregationStrategy()
            {
                IntervalmsOrCount = 7200000,
                Method = Settings.Types.AggregationStrategy.Types.Submission.TimeBased
            };

        /// <summary>
        ///     Settings message with default aggregation and histograms.
        /// </summary>
        public static readonly Settings DefaultSettings = new Settings()
        {
            AggregationStrategy = DefaultAggregation,
            Histograms = {DefaultHistograms}
        };

        /// <summary>
        ///     Always merge these settings into saved settings.
        /// </summary>
        private static readonly Settings SettingsToMerge = new Settings()
        {
            BaseUri = "https://performanceparameters.googleapis.com/v1/",
            InitialRequestTimeoutMs = 1000,
            UltimateRequestTimeoutMs = 100000,
        };

        public static Settings DefaultAdjustFunction(Settings settings)
        {
            settings.AggregationStrategy.AnnotationEnumSize.Clear();
            settings.AggregationStrategy.AnnotationEnumSize.AddRange(MessageUtil.AnnotationEnumSizes);
            settings.AggregationStrategy.MaxInstrumentationKeys = settings.Histograms.Count;
            settings.LevelAnnotationIndex = MessageUtil.SceneAnnotationIndex;
            settings.LoadingAnnotationIndex = MessageUtil.LoadingAnnotationIndex;
            settings.MergeFrom(SettingsToMerge);
            return settings;
        }
    }
}