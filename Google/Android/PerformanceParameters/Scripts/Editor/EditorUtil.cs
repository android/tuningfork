//-----------------------------------------------------------------------
// <copyright file="EditorUtil.cs" company="Google">
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
    using Google.Protobuf;
    using UnityEngine;
    using System.Collections.Generic;
    using System.IO;
    using UnityEditor;
    using Google.Android.PerformanceParameters.Editor.Proto;

    internal class EditorUtil
    {
        private static bool invalidFidelityMessages
        {
            set { EditorPrefs.SetBool("tuningfork-invalidFidelityMessages", value); }
            get { return EditorPrefs.GetBool("tuningfork-invalidFidelityMessages", false); }
        }

        public static readonly Proto.FileInfo ProtoFile = CreateProtoFile();

        private static Proto.FileInfo CreateProtoFile()
        {
            var protoFile = new Proto.FileInfo(DevTuningforkReflection.Descriptor);

            // Always add scene and loading state enum.
            protoFile.AddEnum(EnumInfoHelper.GetSceneEnum(EditorBuildSettings.scenes));
            protoFile.AddEnum(EnumInfoHelper.GetLoadingStateEnum());

            protoFile.OnUpdate += () =>
            {
                Action OnFail = null;
                if (File.Exists(Paths.DevProtoPath))
                {
                    string previousText = File.ReadAllText(Paths.DevProtoPath);
                    OnFail = () => { File.WriteAllText(Paths.DevProtoPath, previousText); };
                }

                Directory.CreateDirectory(Path.GetDirectoryName(Paths.DevProtoPath));
                File.WriteAllText(Paths.DevProtoPath, protoFile.ToProtoString());
                if (!ProtocCompiler.GenerateProtoAndDesc() && OnFail != null) OnFail();
                else
                {
                    AssetDatabase.Refresh();
                    invalidFidelityMessages = true;
                }
            };

            // If files do not exist yet, call OnUpdate to generate them (.proto and .descriptor).
            if (!File.Exists(Paths.DevProtoPath))
            {
                protoFile.OnUpdate();
            }

            return protoFile;
        }

        // Update scenes enum if scenes have changed in build settings
        internal static void UpdateSceneEnum()
        {
            var allEnums = MessageUtil.EnumHelper.CreateInfoList();
            EnumInfo newSceneEnum = EnumInfoHelper.GetSceneEnum(EditorBuildSettings.scenes);
            EnumInfo prevSceneEnum = allEnums.Find(x => x.name == Names.SceneEnumName);

            if (newSceneEnum.values == prevSceneEnum.values) return;
            Debug.Log("Tuningfork: Scenes enum is updated");
            ProtoFile.AddEnum(newSceneEnum);
        }

        // Get set of parameters with quality levels
        internal static List<IMessage> FidelityMessagesWithQualityLevels()
        {
            List<IMessage> messages = new List<IMessage>();
            for (int i = 0; i < QualitySettings.names.Length; ++i)
            {
                IMessage fp = MessageUtil.NewFidelityParams();
                PerformanceParameters.MessageUtil.SetQualityLevel(fp, i);
                messages.Add(fp);
            }

            return messages;
        }

        static EditorUtil()
        {
            EditorBuildSettings.sceneListChanged += () => { UpdateSceneEnum(); };

            // In case the proto message was updated (fields were added/deleted/etc)
            // saved fidelity messages might become invalid.
            if (invalidFidelityMessages)
            {
                if (FileUtil.ReSaveAllFidelityMessages())
                {
                    Debug.Log("Fidelity levels are re-saved.");
                    Notification.Show("Fidelity levels",
                        "Your fidelity levels were automatically re-saved after you've changed the " +
                        "fidelity message. Please check your fidelity levels to ensure all values are correct.");
                }

                invalidFidelityMessages = false;
            }
        }


        [UnityEditor.Callbacks.DidReloadScripts]
        public static void RefreshAssetsCompleted()
        {
            Debug.Log("Editor Util reload ");
            RefreshSettings();
        }

        // Annotation size may change, refresh settings if it is happened
        static void RefreshSettings()
        {
            Settings saved = FileUtil.LoadSettings(SettingsUtil.DefaultSettings, SettingsUtil.DefaultAdjustFunction);
            var sizes = MessageUtil.AnnotationEnumSizes;
            if (saved.AggregationStrategy.AnnotationEnumSize.Count != sizes.Count)
            {
                FileUtil.SaveSettings(saved, SettingsUtil.DefaultAdjustFunction);
                return;
            }

            for (int i = 0; i < sizes.Count; ++i)
            {
                if (sizes[i] != saved.AggregationStrategy.AnnotationEnumSize[i])
                {
                    FileUtil.SaveSettings(saved, SettingsUtil.DefaultAdjustFunction);
                    return;
                }
            }
        }
    }
}