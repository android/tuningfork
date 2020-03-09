//-----------------------------------------------------------------------
// <copyright file="Cache.cs" company="Google">
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

namespace Google.Android.PerformanceTuner.Editor
{
    public class EditorStatePrefs<T> where T : class
    {
        const string k_KeyPrefix = "android-performance-tuner-cache-";
        readonly string m_Key;
        readonly T m_DefaultValue;

        public EditorStatePrefs(string key, T defaultValue)
        {
            m_Key = k_KeyPrefix + key;
            m_DefaultValue = defaultValue;
        }

        public T Get()
        {
            if (!PlayerPrefs.HasKey(m_Key)) Set(m_DefaultValue);
            var json = PlayerPrefs.GetString(m_Key);
            return JsonUtility.FromJson<T>(json);
        }

        public void Set(T value)
        {
            var json = JsonUtility.ToJson(value);
            PlayerPrefs.SetString(m_Key, json);
        }
    }
}