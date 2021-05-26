//-----------------------------------------------------------------------
// <copyright file="Initializer.cs" company="Google">
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
using Google.Protobuf;
using UnityEngine;
using System.IO;
using UnityEditor;
using Google.Android.PerformanceTuner.Editor.Proto;

namespace Google.Android.PerformanceTuner.Editor
{
    /// <summary>
    ///     Initializing plugin data and callbacks.
    /// </summary>
    public class Initializer
    {
        public static readonly Proto.FileInfo protoFile;
        public static readonly DevDescriptor devDescriptor;
        public static readonly ProjectData projectData;
        public static readonly EnumInfoHelper enumInfoHelper;
        public static readonly SetupConfig setupConfig;

        static bool s_Valid;

        public static bool valid
        {
            get { return s_Valid; }
            private set { s_Valid = value; }
        }

        static string s_InitMessage = string.Empty;

        public static string initMessage
        {
            get { return s_InitMessage; }
            private set { s_InitMessage = value; }
        }

        static Initializer()
        {
            Debug.Log("Android Performance Tuner Initializer");

            setupConfig = FileUtil.LoadSetupConfig();

            devDescriptor = CreateDescriptor();

            protoFile = CreateProtoFile(devDescriptor);

            CreateAsmdefFile();

            projectData = new ProjectData();
            projectData.LoadFromStreamingAssets(devDescriptor);

            enumInfoHelper = new EnumInfoHelper(devDescriptor.fileDescriptor);

            EditorBuildSettings.sceneListChanged += () =>
            {
                Debug.Log("Android Performance Tuner SceneListChanged");
                UpdateSceneEnum();
            };

            if (!FidelityBuilder.builder.valid)
            {
                initMessage = "Fidelity message is not generated yet or your project is still compiling. " +
                              "If this message persists, try to re-import the plugin and generated assets." +
                              "\n[macOS] Check if protoc compiler can be opened. " +
                              "In the Finder locate protoc binary in AndroidPerformanceTuner/Editor/Protoc, " +
                              "control-click the binary, choose Open and then click Open. ";
                Debug.Log(initMessage);
                valid = false;
                return;
            }

            UpdateFidelityMessages();
            CheckForLoadingStateInAnnotation();

            // TODO(kseniia): Check for possible inconsistencies in the data, set to false if any found
            // TODO(kseniia): or remove "valid" if all problems could be fixed in-place
            valid = true;
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        public static void RefreshAssetsCompleted()
        {
            Debug.Log("Android Performance Tuner RefreshAssetsCompleted");
        }

        static void CheckForLoadingStateInAnnotation()
        {
            if (projectData.hasLoadingState)
            {
                Debug.LogError(Names.fixDefaultAnnotationConsoleMessage);
            }
        }

        static DevDescriptor CreateDescriptor()
        {
            if (File.Exists(Paths.devDescriptorPath))
            {
                var bytes = File.ReadAllBytes(Paths.devDescriptorPath);
                var savedDescriptor = new DevDescriptor(bytes);
                if (savedDescriptor.Build())
                {
                    // Descriptor is successfully loaded from saved .descriptor file
                    return savedDescriptor;
                }

                Debug.LogErrorFormat(
                    "Android Performance Tuner: Proto definition from {0} could not be loaded, it will be replaced by default proto",
                    Paths.devDescriptorPath);
            }

            // TODO(kseniia): Save .descriptor, .cs, etc.
            var defaultDescriptor = new DevDescriptor(DefaultMessages.serializedDefaultProto);
            defaultDescriptor.Build();
            return defaultDescriptor;
        }

        static Proto.FileInfo CreateProtoFile(DevDescriptor devDescriptor)
        {
            var protoFile = new Proto.FileInfo(devDescriptor.fileDescriptor);

            // Always add scene.
            protoFile.AddEnum(EnumInfoHelper.GetSceneEnum(EditorBuildSettings.scenes));

            protoFile.onUpdate += () =>
            {
                Action onFail = null;
                if (File.Exists(Paths.devProtoPath))
                {
                    var previousText = File.ReadAllText(Paths.devProtoPath);
                    onFail = () => { File.WriteAllText(Paths.devProtoPath, previousText); };
                }

                Directory.CreateDirectory(Path.GetDirectoryName(Paths.devProtoPath));
                File.WriteAllText(Paths.devProtoPath, protoFile.ToProtoString());
                if (!ProtocCompiler.GenerateProtoAndDesc() && onFail != null) onFail();
                else
                {
                    AssetDatabase.Refresh();
                    areFidelityMessagesDirty = true;
                }
            };

            // If files do not exist yet, call OnUpdate to generate them (.proto, .descriptor and .cs).
            if (!File.Exists(Paths.devProtoPath) ||
                !File.Exists(Paths.devDescriptorPath) ||
                !FidelityBuilder.builder.valid)
            {
                protoFile.onUpdate();
            }

            return protoFile;
        }

        const string k_AsmdefContent =
            "{\"name\": \"Google.Android.PerformanceTuner_gen\"," +
            "\"references\": [\"Google.Android.PerformanceTuner\"]," +
            "\"optionalUnityReferences\": [],\"includePlatforms\": []," +
            "\"excludePlatforms\": [],\"allowUnsafeCode\": false," +
            "\"overrideReferences\": false,\"precompiledReferences\": [\"Google.Protobuf.dll\"], " +
            "\"autoReferenced\": true,\"defineConstraints\": []}";

        static void CreateAsmdefFile()
        {
            if (File.Exists(Paths.asmdefPath)) return;
            Debug.LogFormat("Creating Google.Android.PerformanceTuner_gen.asmdef file...\n {0}", Paths.asmdefPath);
            Directory.CreateDirectory(Path.GetDirectoryName(Paths.asmdefPath));
            File.WriteAllText(Paths.asmdefPath, k_AsmdefContent);
        }

        /// <summary>
        ///     Update scenes enum if scenes have changed in build settings
        /// </summary>
        static void UpdateSceneEnum()
        {
            var allEnums = enumInfoHelper.CreateInfoList();
            var newSceneEnum = EnumInfoHelper.GetSceneEnum(EditorBuildSettings.scenes);
            var prevSceneEnum = allEnums.Find(x => x.name == Names.sceneEnumName);

            if (newSceneEnum.values == prevSceneEnum.values) return;
            Debug.Log("Android Performance Tuner: Scenes enum is updated");
            protoFile.AddEnum(newSceneEnum);
        }

        static void UpdateFidelityMessages()
        {
            // If the proto message was updated (fields were added/deleted/etc),
            // saved fidelity messages might become invalid.
            if (areFidelityMessagesDirty)
            {
                if (FileUtil.ReSaveAllFidelityMessages())
                {
                    Debug.Log("Quality levels are re-saved.");
                    Notification.Show("Quality levels",
                        "Your quality levels were automatically re-saved after you changed the " +
                        "fidelity message. Please check your quality levels to ensure all values are correct.");
                }

                areFidelityMessagesDirty = false;
            }

            if (!setupConfig.useAdvancedFidelityParameters)
            {
                // Fidelity messages will be saved in PostGenerateGradle for version > 2018.2
#if !UNITY_2018_2_OR_NEWER
                FileUtil.SaveFidelityMessages(Paths.androidAssetsPath, MessageUtil.FidelityMessagesWithQualityLevels());
#endif
            }
        }

        static bool areFidelityMessagesDirty
        {
            set { PlayerPrefs.SetInt("android-performance-tuner-invalidFidelityMessages", value ? 1 : 0); }
            get { return PlayerPrefs.GetInt("android-performance-tuner-invalidFidelityMessages", 0) == 1; }
        }
    }
}