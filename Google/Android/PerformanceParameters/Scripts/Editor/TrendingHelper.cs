//-----------------------------------------------------------------------
// <copyright file="TrendingHelper.cs" company="Google">
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
    using UnityEngine;
    using System.Linq;
    using System.Collections.Generic;
    using System.Reflection;
    using Google.Protobuf;
    using Google.Protobuf.Reflection;

    internal class TrendingHelper
    {
        internal TrendingHelper()
        {
            foreach (var icon in m_IconNames)
                Icons.Add(icon.Key, new GUIContent((Texture) Resources.Load(icon.Value)));
        }

        internal readonly Dictionary<Trending, GUIContent> Icons = new Dictionary<Trending, GUIContent>();

        readonly Dictionary<Trending, String> m_IconNames = new Dictionary<Trending, string>()
        {
            {Trending.Invalid, "ic_error_outline"},
            {Trending.Flat, "ic_trending_flat"},
            {Trending.Down, "ic_trending_down"},
            {Trending.Up, "ic_trending_up"}
        };

        internal static Trending FindTrending(IList<IMessage> messages, FieldDescriptor field)
        {
            object prevValue = null;
            Trending trending = Trending.Flat;
            foreach (var message in messages)
            {
                field.Accessor.GetValue(message);
                object value = field.Accessor.GetValue(message);
                trending = MergeTrending(trending, FindTrending(prevValue, value));
                prevValue = value;
            }

            return trending;
        }

        static Trending MergeTrending(params Trending[] trendings)
        {
            if (trendings.Contains(Trending.Invalid))
                return Trending.Invalid;
            if (trendings.Contains(Trending.Down) && trendings.Contains(Trending.Up))
                return Trending.Invalid;
            if (trendings.Contains(Trending.Down)) return Trending.Down;
            if (trendings.Contains(Trending.Up)) return Trending.Up;
            return Trending.Flat;
        }

        static Trending FindTrending(object prevValue, object nextValue)
        {
            if (prevValue == null) return Trending.Flat;

            if (prevValue.GetType() != nextValue.GetType())
                throw new ArgumentException("Can not find trending, values have different types");

            if (nextValue is int)
            {
                if ((int) prevValue < (int) nextValue) return Trending.Up;
                if ((int) prevValue > (int) nextValue) return Trending.Down;
                return Trending.Flat;
            }

            if (nextValue is float)
            {
                if ((float) prevValue < (float) nextValue) return Trending.Up;
                if ((float) prevValue > (float) nextValue) return Trending.Down;
                return Trending.Flat;
            }

            if (nextValue is Enum)
            {
                if ((int) prevValue < (int) nextValue) return Trending.Up;
                if ((int) prevValue > (int) nextValue) return Trending.Down;
                return Trending.Flat;
            }

            throw new ArgumentException("Can not find trending, unsupported values type");
        }
    }
}