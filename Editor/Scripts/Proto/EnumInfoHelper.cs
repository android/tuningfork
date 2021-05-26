//-----------------------------------------------------------------------
// <copyright file="EnumInfoHelper.cs" company="Google">
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
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Google.Protobuf.Reflection;
using UnityEditor;

namespace Google.Android.PerformanceTuner.Editor.Proto
{
    /// <summary>
    ///     Helper to manipulate enums being declared in the Tuning Fork generated proto.
    /// </summary>
    public class EnumInfoHelper
    {
        public static readonly string[] reservedEnumNames = {Names.sceneEnumName};

        readonly IList<EnumDescriptor> m_EnumTypes;

        public EnumInfoHelper(FileDescriptor fileDescriptor)
        {
            m_EnumTypes = fileDescriptor.EnumTypes;
        }

        public List<string> GetAllNames()
        {
            return m_EnumTypes.Select(x => x.Name).ToList();
        }

        public IEnumerable<string> GetAllEditableNames()
        {
            return m_EnumTypes.Select(x => x.Name).Where(x => !reservedEnumNames.Contains(x));
        }

        public List<EnumInfo> CreateInfoList()
        {
            var enums = new List<EnumInfo>();
            foreach (var enumType in m_EnumTypes)
            {
                var prefix = enumType.Name + "_";
                var prefixSize = prefix.Length;
                var info = new EnumInfo
                {
                    name = enumType.Name,
                    values = enumType.Values.Select(enumValue => enumValue.Name.Substring(prefixSize)).ToList()
                };
                enums.Add(info);
            }

            return enums;
        }

        public EnumInfo? GetInfo(string name)
        {
            EnumDescriptor enumType = m_EnumTypes.FirstOrDefault(x => x.Name == name);
            if (enumType == null) return null;
            var prefix = enumType.Name + "_";
            var prefixSize = prefix.Length;
            return new EnumInfo
            {
                name = enumType.Name,
                values = enumType.Values.Select(enumValue => enumValue.Name.Substring(prefixSize)).ToList()
            };
        }


        static readonly Regex k_SceneFieldRegex = new Regex("[^a-zA-Z0-9_]");

        public static EnumInfo GetSceneEnum(EditorBuildSettingsScene[] scenes)
        {
            var info = new EnumInfo
            {
                name = Names.sceneEnumName,
                values = new List<string>()
            };
            // TODO (b/120588304): Make sure enum values are not duplicated.
            foreach (var scene in scenes)
            {
                // Scene could be deleted, skip them
                if (scene == null || string.IsNullOrEmpty(scene.path)) continue;
                var sceneName = scene.path
                    .Replace(Path.DirectorySeparatorChar, '_')
                    .Replace(Path.AltDirectorySeparatorChar, '_')
                    .Replace(Path.GetExtension(scene.path), "")
                    .Replace(" ", "_")
                    .ToUpper();
                sceneName = k_SceneFieldRegex.Replace(sceneName, "");
                info.values.Add(sceneName);
            }

            return info;
        }
    }
}