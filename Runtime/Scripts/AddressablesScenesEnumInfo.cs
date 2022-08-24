//-----------------------------------------------------------------------
// <copyright file="AddressablesScenesEnumInfo.cs" company="Google">
//
// Copyright 2022 Google Inc. All Rights Reserved.
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

using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Google.Android.PerformanceTuner
{
    /// <summary>
    ///     Associates addressables scene with their corresponding value in the Scene enum
    ///     used by Android Performance Tuner's protobuf as listed in DevDescriptor.cs.
    /// </summary>
    [System.Serializable]
    public class AddressablesScenesEnumInfo
    {
        public string scenePath;
        public string protoEnumSceneName;
        public int value;

        static readonly Regex k_SceneFieldRegex = new Regex("[^a-zA-Z0-9_]");

        public AddressablesScenesEnumInfo(string scenePath, int value)
        {
            this.scenePath = scenePath;
            protoEnumSceneName = ConvertScenePathToProtoEnumEntry(scenePath, true);
            this.value = value;
        }

        // Use this function to get the protobuf Scene Enum entry corresponding to a scene.
        public static string ConvertScenePathToProtoEnumEntry(string scenePath, bool isAddressableScene)
        {
            string sceneName = scenePath
                .Replace(Path.DirectorySeparatorChar, '_')
                .Replace(Path.AltDirectorySeparatorChar, '_')
                .Replace(Path.GetExtension(scenePath), "")
                .Replace(" ", "_")
                .ToUpper();
            sceneName = k_SceneFieldRegex.Replace(sceneName, "");
            string prefix = isAddressableScene ? "ADDR_" : "";
            return  prefix + sceneName;
        }

        public static int Compare(AddressablesScenesEnumInfo first, AddressablesScenesEnumInfo second)
        {
            return first.value >= second.value ? 1 : -1;
        }
    }
}