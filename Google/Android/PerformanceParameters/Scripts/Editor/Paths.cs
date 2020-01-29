//-----------------------------------------------------------------------
// <copyright file="Paths.cs" company="Google">
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

using System.IO;
using UnityEngine;

namespace Google.Android.PerformanceParameters.Editor
{
    public static class Paths
    {
        /// <summary>
        ///     Path of the plugin relative to Assets folder.
        /// </summary>
        private static readonly string PluginRelativePath =
            Path.Combine("Google", "Android", "PerformanceParameters");

        /// <summary>
        ///     Absolute path of the plugin folder.
        /// </summary>
        public static readonly string PluginAbsolutePath = Path.Combine(Application.dataPath, PluginRelativePath);

        public static readonly string AndroidAssetsPath = Path.Combine(
            PluginAbsolutePath,
            "Editor",
            "AndroidAssets");

        public static readonly string DevProtoPath = Path.Combine(
            PluginAbsolutePath,
            "Editor",
            "Proto",
            "dev_tuningfork.proto");


        public static readonly string DevCsOutDirectoryPath = Path.Combine(PluginAbsolutePath, "Scripts", "Proto");

        public static readonly string DevDescriptorPath = Path.Combine(
            AndroidAssetsPath,
            "dev_tuningfork.descriptor");

        public static readonly string DevSettingsPath = Path.Combine(
            AndroidAssetsPath,
            "tuningfork_settings.bin");

        public static readonly string DevFidelityFilePrefix = "dev_tuningfork_fidelityparams_";

        public static readonly string ConfigPath = Path.Combine(
            "Assets",
            PluginRelativePath,
            "Resources",
            "SetupConfig.asset");

        public static readonly string CachePath = Path.Combine(
            "Assets",
            PluginRelativePath,
            "Editor",
            "Resources",
            "Cache.asset");

        public static readonly string ProtocPath = Path.Combine(PluginAbsolutePath, "Editor", "Protoc");
    }
}