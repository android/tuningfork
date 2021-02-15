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

namespace Google.Android.PerformanceTuner.Editor
{
    public static class Paths
    {
        /// <summary>
        ///     Package name as defined in package.json
        /// </summary>
        const string k_PackageName = "com.google.android.performancetuner";

        /// <summary>
        ///     Path of the plugin relative to Assets folder.
        /// </summary>
        static readonly string k_PluginRelativePath =
            Path.Combine("AndroidPerformanceTuner");

        /// <summary>
        ///     Absolute path of the plugin folder.
        /// </summary>
        public static string pluginAbsolutePath
        {
            get
            {
                if (string.IsNullOrEmpty(s_PluginAbsolutePath))
                {
                    var packagePath = Path.Combine("Packages", k_PackageName);
                    // If plugin is a package it will be inside Packages directory.
                    if (Directory.Exists(packagePath))
                        s_PluginAbsolutePath = Path.GetFullPath(packagePath);
                    // If plugin is not a package it will be inside Assets directory.
                    else
                        s_PluginAbsolutePath = Path.Combine(Application.dataPath, k_PluginRelativePath);
                }

                return s_PluginAbsolutePath;
            }
        }

        static string s_PluginAbsolutePath;


        /// <summary>
        ///     Absolute path to the directory with generated files
        /// </summary>
        static readonly string k_GeneratedFilesDirectory =
            Path.Combine(Application.dataPath, "AndroidPerformanceTuner_gen");


        /// <summary>
        ///     Path to AndroidAssets folder.
        /// </summary>
        static readonly string k_AndroidAssetsPath2018 = Path.Combine(
            k_GeneratedFilesDirectory,
            "Editor",
            "AndroidAssets");

        /// <summary>
        ///     Path to StreamingAssets/tuningfork folder.
        /// </summary>
        static readonly string k_AndroidAssetsPath2017 = Path.Combine(
            Application.streamingAssetsPath, "tuningfork");

        /// <summary>
        ///     Relative path to assets folder.
        ///     For unity >= 2018.2 it points to AndroidAssets.
        ///     For unity < 2018.2 it points to StreamingAssets.
        /// </summary>
        public static readonly string androidAssetsPathName =
#if UNITY_2018_2_OR_NEWER
            Path.Combine("AndroidPerformanceTuner_gen", "Editor", "AndroidAssets");
#else
            Path.Combine("StreamingAssets", "tuningfork");
#endif

        /// <summary>
        ///     Path to assets folder.
        ///     For unity >= 2018.2 it points to AndroidAssets.
        ///     For unity < 2018.2 it points to StreamingAssets.
        /// </summary>
        public static readonly string androidAssetsPath =
#if UNITY_2018_2_OR_NEWER
            k_AndroidAssetsPath2018;
#else
            k_AndroidAssetsPath2017;
#endif


        /// <summary>
        ///     Path to dev_tuningfork.proto file
        /// </summary>
        public static readonly string devProtoPath = Path.Combine(
            k_GeneratedFilesDirectory,
            "Editor",
            "Proto",
            "dev_tuningfork.proto");


        /// <summary>
        ///     Path to a folder where DevTuningfork.cs file should be generated.
        /// </summary>
        public static readonly string devCsOutDirectoryPath =
            Path.Combine(k_GeneratedFilesDirectory, "Runtime", "Scripts");


        /// <summary>
        ///     Path to Google.Android.PerformanceTuner_gen.asmdef file.
        /// </summary>
        public static readonly string asmdefPath =
            Path.Combine(k_GeneratedFilesDirectory, "Runtime", "Google.Android.PerformanceTuner_gen.asmdef");

        /// <summary>
        ///     Path to dev_tuningfork.descriptor file.
        /// </summary>
        public static readonly string devDescriptorPath = Path.Combine(
            androidAssetsPath,
            "dev_tuningfork.descriptor");

        /// <summary>
        ///     Path to tuningfork_settings.bin file.
        /// </summary>
        public static readonly string devSettingsPath = Path.Combine(
            androidAssetsPath,
            "tuningfork_settings.bin");

        public static readonly string devFidelityFilePrefix = "dev_tuningfork_fidelityparams_";

        /// <summary>
        ///     Path to SetupConfig.asset file
        /// </summary>
        public static readonly string configPath = Path.Combine(
            "Assets",
            "AndroidPerformanceTuner_gen",
            "Runtime",
            "Resources",
            "SetupConfig.asset");

        /// <summary>
        ///     Path to root folder with protoc binaries.
        /// </summary>
        public static readonly string protocPath = Path.Combine(pluginAbsolutePath, "Editor", "Protoc");
    }
}