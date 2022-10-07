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
using UnityEngine;
using System.IO;
using UnityEditor;
using Google.Android.PerformanceTuner.Editor.Proto;
using FileInfo = Google.Android.PerformanceTuner.Editor.Proto.FileInfo;

#if APT_ADDRESSABLE_PACKAGE_PRESENT
using UnityEditor.AddressableAssets.Settings;
#endif

namespace Google.Android.PerformanceTuner.Editor
{
    /// <summary>
    ///     Initializing plugin data and callbacks.
    /// </summary>
    public class Initializer : AssetPostprocessor
    {
        private static Proto.FileInfo protoFile;
        private static DevDescriptor devDescriptor;
        private static ProjectData projectData;
        private static EnumInfoHelper enumInfoHelper;
        private static SetupConfig setupConfig;
        
        public static FileInfo ProtoFile => protoFile;
        public static DevDescriptor DevDescriptor => devDescriptor;
        public static ProjectData ProjectData => projectData;
        public static EnumInfoHelper EnumInfoHelper => enumInfoHelper;
        public static SetupConfig SetupConfig => setupConfig;

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

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (string str in importedAssets)
            {
                // We are interested in knowing when the SetupConfig has been re-imported so that it can correctly be
                // loaded and used by the Init function.
                if (str.Equals(Paths.configPath))
                {
                    Init();
                    break;
                }
            }
        }

        static Initializer()
        {
            // When reimporting a project, scripts might be imported before other types of assets are. This covers the
            // case in which the SetupConfig is present in the project but has not yet been imported by the
            // AssetDatabase. In this case, return and wait for the OnPostProcessAllAssets to call the Init function
            // after importing SetupConfig.
#if UNITY_2017 || UNITY_2018
            var guid = AssetDatabase.AssetPathToGUID(Paths.configPath);
            if(!guid.Equals("") && AssetDatabase.LoadAssetAtPath<SetupConfig>(Paths.configPath) == null)
#else
            int setupConfigFilesFound = AssetDatabase
                .FindAssets(Paths.configFileName, new string[] {Paths.configFolderPath}).Length;
            if ( setupConfigFilesFound > 0
                 && AssetDatabase.LoadAssetAtPath<SetupConfig>(Paths.configPath) == null)
#endif
            {
                return;
            }
            // If there's no SetupConfig file, create one and let Init be called by OnPostProcessAllAssets.
#if UNITY_2017 || UNITY_2018
            if(guid.Equals(""))
#else
            if (setupConfigFilesFound == 0)
#endif
            {
                setupConfig = FileUtil.CreateSetupConfig();
                return;
            }

            // In all the other cases, proceed to Init.
            Init();
        }

        static void Init()
        {
            setupConfig = FileUtil.LoadSetupConfig();

            if (setupConfig.pluginVerboseLoggingEnabled)
            {
                Debug.Log("Android Performance Tuner Initializer");
            }

            devDescriptor = CreateDescriptor();

            // When a new package is imported in Unity 2018, EditorBuildSettings.scenes are not updated yet when
            // Initializer() is called, thus the .proto file does not reflect possible changes in the scenes.
            // This callback creates the .proto file when the package has finished importing and the
            // EditorBuildSettings are up to date.
#if UNITY_2018
            AssetDatabase.importPackageCompleted += (packageName) =>
            {
                CreateProtoFile(devDescriptor);
            };
#endif

            enumInfoHelper = new EnumInfoHelper(devDescriptor.fileDescriptor);
            protoFile = CreateProtoFile(devDescriptor);

            CreateAsmdefFile();

            projectData = new ProjectData();
            projectData.LoadFromStreamingAssets(devDescriptor);

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
            if (setupConfig && setupConfig.pluginVerboseLoggingEnabled)
            {
                Debug.Log("Android Performance Tuner RefreshAssetsCompleted");
            }
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
                File.WriteAllText(Paths.devProtoPath, protoFile.ToProtoString(setupConfig));
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
                !FidelityBuilder.builder.valid ||
                HasSceneEnumChanged())
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
            if (HasSceneEnumChanged())
            {
                Debug.Log("Android Performance Tuner: Scenes enum is updated");
                var newSceneEnum = EnumInfoHelper.GetSceneEnum(EditorBuildSettings.scenes);
                protoFile.AddEnum(newSceneEnum);
            }
        }

        static bool HasSceneEnumChanged()
        {
            var newSceneEnum = EnumInfoHelper.GetSceneEnum(EditorBuildSettings.scenes);
            var allEnums = enumInfoHelper.CreateInfoList();
            var prevSceneEnum = allEnums.Find(x => x.name == Names.sceneEnumName);
            return newSceneEnum.values != prevSceneEnum.values;
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