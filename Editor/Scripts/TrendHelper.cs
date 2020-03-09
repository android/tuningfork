//-----------------------------------------------------------------------
// <copyright file="TrendHelper.cs" company="Google">
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

namespace Google.Android.PerformanceTuner.Editor
{
    using System;
    using UnityEngine;
    using System.Linq;
    using System.Collections.Generic;
    using Google.Protobuf;
    using Google.Protobuf.Reflection;

    public class TrendHelper
    {
        public TrendHelper()
        {
            foreach (var icon in k_IconNames)
                icons.Add(icon.Key, new GUIContent((Texture) Resources.Load(icon.Value)));
        }

        public readonly Dictionary<Trend, GUIContent> icons = new Dictionary<Trend, GUIContent>();

        readonly Dictionary<Trend, string> k_IconNames = new Dictionary<Trend, string>()
        {
            {Trend.Invalid, "ic_error_outline"},
            {Trend.Flat, "ic_trending_flat"},
            {Trend.Down, "ic_trending_down"},
            {Trend.Up, "ic_trending_up"}
        };

        public static Trend FindTrend(IEnumerable<IMessage> messages, FieldDescriptor field)
        {
            object prevValue = null;
            var trend = Trend.Flat;
            foreach (var message in messages)
            {
                field.Accessor.GetValue(message);
                object value = field.Accessor.GetValue(message);
                trend = MergeTrends(trend, FindTrend(prevValue, value));
                prevValue = value;
            }

            return trend;
        }

        static Trend MergeTrends(params Trend[] trends)
        {
            if (trends.Contains(Trend.Invalid))
                return Trend.Invalid;
            if (trends.Contains(Trend.Down) && trends.Contains(Trend.Up))
                return Trend.Invalid;
            if (trends.Contains(Trend.Down)) return Trend.Down;
            if (trends.Contains(Trend.Up)) return Trend.Up;
            return Trend.Flat;
        }

        static Trend FindTrend(object prevValue, object nextValue)
        {
            if (prevValue == null) return Trend.Flat;

            if (prevValue.GetType() != nextValue.GetType())
                throw new ArgumentException("Can not find trend, values have different types");

            if (nextValue is int)
            {
                if ((int) prevValue < (int) nextValue) return Trend.Up;
                if ((int) prevValue > (int) nextValue) return Trend.Down;
                return Trend.Flat;
            }

            if (nextValue is float)
            {
                if ((float) prevValue < (float) nextValue) return Trend.Up;
                if ((float) prevValue > (float) nextValue) return Trend.Down;
                return Trend.Flat;
            }

            if (nextValue is Enum)
            {
                if ((int) prevValue < (int) nextValue) return Trend.Up;
                if ((int) prevValue > (int) nextValue) return Trend.Down;
                return Trend.Flat;
            }

            throw new ArgumentException("Can not find trend, unsupported values type");
        }
    }
}