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

namespace Google.Android.PerformanceTuner.Editor
{
    public class SettingsUtil
    {
        // TODO(b/120588304): Add unity frame buckets.
        static readonly List<Settings.Types.Histogram> k_DefaultHistograms =
            new List<Settings.Types.Histogram>()
            {
                new Settings.Types.Histogram()
                {
                    InstrumentKey = (int) InstrumentationKeys.RawFrameTime,
                    BucketMin = 6.54f,
                    BucketMax = 60f,
                    NBuckets = 200,
                },
                new Settings.Types.Histogram()
                {
                    InstrumentKey = (int) InstrumentationKeys.PacedFrameTime,
                    BucketMin = 10,
                    BucketMax = 40,
                    NBuckets = 30,
                },
                new Settings.Types.Histogram()
                {
                    InstrumentKey = (int) InstrumentationKeys.GpuTime,
                    BucketMin = 0,
                    BucketMax = 20,
                    NBuckets = 30,
                },
                new Settings.Types.Histogram()
                {
                    InstrumentKey = (int) InstrumentationKeys.CpuTime,
                    BucketMin = 0,
                    BucketMax = 20,
                    NBuckets = 30,
                }
            };

        static readonly Settings.Types.AggregationStrategy k_DefaultAggregation =
            new Settings.Types.AggregationStrategy()
            {
                IntervalmsOrCount = 600000, /* 10 minutes */
                Method = Settings.Types.AggregationStrategy.Types.Submission.TimeBased
            };

        /// <summary>
        ///     Settings message with default aggregation and histograms.
        /// </summary>
        public static readonly Settings defaultSettings = new Settings()
        {
            AggregationStrategy = k_DefaultAggregation,
            Histograms = {k_DefaultHistograms}
        };

        /// <summary>
        ///     Always merge these settings into saved settings.
        /// </summary>
        static readonly Settings k_SettingsToMerge = new Settings()
        {
            BaseUri = "https://performanceparameters.googleapis.com/v1/",
            InitialRequestTimeoutMs = 1000,
            UltimateRequestTimeoutMs = 100000,
        };

        /// <summary>
        ///     Adjust Settings to match information from proto descriptor.
        /// </summary>
        /// <returns></returns>
        public static Settings DefaultAdjustFunction(Settings settings, DevDescriptor devDescriptor)
        {
            settings.AggregationStrategy.AnnotationEnumSize.Clear();
            settings.AggregationStrategy.AnnotationEnumSize.AddRange(devDescriptor.annotationEnumSizes);
            settings.AggregationStrategy.MaxInstrumentationKeys = settings.Histograms.Count;
            settings.LevelAnnotationIndex = devDescriptor.sceneAnnotationIndex;
            settings.LoadingAnnotationIndex = devDescriptor.loadingAnnotationIndex;
            settings.MergeFrom(k_SettingsToMerge);
            return settings;
        }
    }
}