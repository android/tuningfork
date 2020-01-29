//-----------------------------------------------------------------------
// <copyright file="FileUtil.cs" company="Google">
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Google.Protobuf;
using UnityEditor;
using UnityEngine;

namespace Google.Android.PerformanceParameters.Editor
{
    public delegate Settings AdjustSettings(Settings settings);

    /// <summary>
    ///     Util to save and load tuningfork files from/to the project.
    /// </summary>
    public class FileUtil
    {
        /// <summary>
        ///     Load the setup config asset from the project.
        ///     If config is not found - create a new one.
        /// </summary>
        /// <returns>SetupConfig.asset loaded from the project.</returns>
        public static SetupConfig LoadSetupConfig()
        {
            SetupConfig config = AssetDatabase.LoadAssetAtPath<SetupConfig>(Paths.ConfigPath);
            if (config != null) return config;
            Directory.CreateDirectory(Path.GetDirectoryName(Paths.ConfigPath));
            config = ScriptableObject.CreateInstance<SetupConfig>();
            AssetDatabase.CreateAsset(config, Paths.ConfigPath);
            Debug.LogFormat("New asset created at path {0}", Paths.ConfigPath);
            return config;
        }

        /// <summary>
        ///     First index for fidelity messages saved in StreamingAssets.
        /// </summary>
        private const int MessageStartIndex = 1;

        private static readonly Regex FpFilename = new Regex(Paths.DevFidelityFilePrefix + "(\\d+)" + ".bin");

        public static string GetFidelityMessageFilename(int index)
        {
            return Paths.DevFidelityFilePrefix + index + ".bin";
        }

        public static int GetFidelityMessageIndex(string fileName)
        {
            var match = FpFilename.Match(fileName);
            if (match.Success)
            {
                int index;
                if (!int.TryParse(match.Groups[1].Value, out index))
                    throw new ArgumentException(string.Format("File name has incorrect format [{0}]", fileName));
                return index;
            }

            throw new ArgumentException(string.Format("File name has incorrect format [{0}]", fileName));
        }

        /// <summary>
        ///     Save list of messages to StreamingAssets.
        ///     All previously saved messages will be deleted.
        /// </summary>
        /// <param name="messages">messages to save</param>
        internal static void SaveAllFidelityMessages(List<IMessage> messages)
        {
            DeleteAllFidelityMessages();

            var index = MessageStartIndex;
            foreach (var message in messages)
            {
                SaveFidelityMessage(message, index);
                index++;
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        ///     Save fidelity messages in the apk.
        ///     Call this in OnPostGenerateGradleAndroidProject.
        /// </summary>
        /// <param name="path">path to the Android project</param>
        /// <param name="messages">messages to save in the apk</param>
        public static void SaveFidelityMessagesInApk(string path, List<IMessage> messages)
        {
            var index = MessageStartIndex;
            foreach (var message in messages)
            {
                var fileName = GetFidelityMessageFilename(index);
                using (var output = File.Create(Path.Combine(path, "src", "main", "assets", "tuningfork", fileName)))
                {
                    message.WriteTo(output);
                }

                index++;
            }
        }

        /// <summary>
        ///     Copy all files (excluding .meta) from the Tuningfork AndroidAssets folder
        ///     to the Android project in /assets/tuningfork/ folder.
        /// </summary>
        /// <param name="path">path to android project</param>
        public static void CopyTuningforkFilesToAndroidProject(string path)
        {
            var tuningforkPath = Path.Combine(path, "src", "main", "assets", "tuningfork");
            Directory.CreateDirectory(tuningforkPath);
            foreach (var assetPath in Directory.EnumerateFiles(Paths.AndroidAssetsPath))
            {
                var fileName = Path.GetFileName(assetPath);
                if (Path.GetExtension(assetPath) != ".meta")
                    File.Copy(
                        Path.Combine(Paths.AndroidAssetsPath, fileName),
                        Path.Combine(tuningforkPath, fileName), true);
            }
        }

        /// <summary>
        ///     Delete all fidelity messages and its metadata from the Android assets.
        /// </summary>
        public static void DeleteAllFidelityMessages()
        {
            foreach (var filePath in Directory.GetFiles(Paths.AndroidAssetsPath,
                Paths.DevFidelityFilePrefix + "*.bin.meta"))
                File.Delete(filePath);
            foreach (var filePath in Directory.GetFiles(Paths.AndroidAssetsPath,
                Paths.DevFidelityFilePrefix + "*.bin"))
                File.Delete(filePath);
            AssetDatabase.Refresh();
        }

        /// <summary>
        ///     Save fidelity message to a file.
        /// </summary>
        /// <param name="message">message to save</param>
        /// <param name="index">index of the file to save file to</param>
        public static void SaveFidelityMessage(IMessage message, int index)
        {
            var fileName = GetFidelityMessageFilename(index);
            using (var output = File.Create(Path.Combine(Paths.AndroidAssetsPath, fileName)))
            {
                message.WriteTo(output);
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        ///     Load all fidelity messages saved in Android assets.
        /// </summary>
        /// <returns>Fidelity messages with its file index as key.</returns>
        public static Dictionary<int, IMessage> LoadAllFidelityMessages()
        {
            var list = new Dictionary<int, IMessage>();
            foreach (var filePath in Directory.GetFiles(Paths.AndroidAssetsPath,
                Paths.DevFidelityFilePrefix + "*.bin"))
            {
                var index = GetFidelityMessageIndex(Path.GetFileName(filePath));
                using (var input = File.OpenRead(filePath))
                {
                    var parameters = MessageUtil.FidelityParamsParser.ParseFrom(input);
                    list.Add(index, parameters);
                }
            }

            return list;
        }

        /// <summary>
        ///     Re-save all fidelity messages saved in Android assets.
        /// </summary>
        /// <returns>true if any changes was detected in fidelity messages.</returns>
        public static bool ReSaveAllFidelityMessages()
        {
            if (!Directory.Exists(Paths.AndroidAssetsPath)) return false;
            var hadChanges = false;
            foreach (var filePath in Directory.GetFiles(Paths.AndroidAssetsPath,
                Paths.DevFidelityFilePrefix + "*.bin"))
            {
                var content = File.ReadAllBytes(filePath);
                var message = MessageUtil.FidelityParamsParser.ParseFrom(content);
                var updatedContent = message.ToByteArray();
                if (content.Length != updatedContent.Length || !content.SequenceEqual(updatedContent))
                {
                    hadChanges = true;
                    File.WriteAllBytes(filePath, updatedContent);
                }
            }

            return hadChanges;
        }

        /// <summary>
        ///     Save settings to a file.
        /// </summary>
        /// <param name="settings">settings to save</param>
        /// <param name="presaveAdjust">Function to automatically change settings before saving them</param>
        public static void SaveSettings(Settings settings, AdjustSettings presaveAdjust)
        {
            settings = presaveAdjust(settings);
            Directory.CreateDirectory(Path.GetDirectoryName(Paths.DevSettingsPath));
            using (var output = File.Create(Paths.DevSettingsPath))
            {
                settings.WriteTo(output);
            }

            AssetDatabase.Refresh();
        }


        /// <summary>
        ///     Load settings from from a file.
        /// </summary>
        /// <param name="defaultSettings">Save and return defaultSettings if there are no saved settings.</param>
        /// <returns>Return loaded or default settings.</returns>
        public static Settings LoadSettings(Settings defaultSettings, AdjustSettings presaveAdjust)
        {
            if (!File.Exists(Paths.DevSettingsPath))
            {
                SaveSettings(defaultSettings, presaveAdjust);
                return defaultSettings;
            }

            using (var input = File.OpenRead(Paths.DevSettingsPath))
            {
                var settings = Settings.Parser.ParseFrom(input);
                return settings;
            }
        }
    }
}