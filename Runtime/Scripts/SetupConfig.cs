//-----------------------------------------------------------------------
// <copyright file="SetupConfig.cs" company="Google">
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
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR && APT_ADDRESSABLE_PACKAGE_PRESENT
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
#endif

namespace Google.Android.PerformanceTuner
{
    /// <summary>
    /// Editor / play settings.
    /// </summary>
    public class SetupConfig : ScriptableObject
    {
        /// <summary>
        /// If plugin is enabled or not.
        /// </summary>
        [FormerlySerializedAs("PluginEnabled")]
        public bool pluginEnabled = true;

        /// <summary>
        /// Use base (quality settings) or advanced fidelity parameters.
        /// </summary>
        [FormerlySerializedAs("UseAdvancedFidelityParameters")]
        public bool useAdvancedFidelityParameters;

        /// <summary>
        /// Use base (level) or advanced annotation.
        /// </summary>
        [FormerlySerializedAs("UseAdvancedAnnotations")]
        public bool useAdvancedAnnotations;

        /// <summary>
        /// Use default or custom instrumentation settings.
        /// </summary>
        public bool useAdvancedInstrumentationSettings;

        /// <summary>
        ///      If false, sensitive information is removed from native logging.
        /// </summary>
        public bool verboseLoggingEnabled;

        /// <summary>
        ///      If false, less APT Plugin information logs are shown
        /// </summary>
        public bool pluginVerboseLoggingEnabled;

        public TunerMode mode = TunerMode.Insights;

#if APT_ADDRESSABLE_PACKAGE_PRESENT
        [SerializeField] private string addressablesSettingsObjectPath = "";

        [HideInInspector] [SerializeField] private List<AddressablesScenesEnumInfo> addressableScenes;
        public List<AddressablesScenesEnumInfo> AddressablesScenes => addressableScenes;

        // Addressable scenes will have an enum value starting from 1000 to allow enough space
        // between them and the scenes in the build settings which start from 1.
        private int initialLastUsedValue = 1000;
        private int lastUsedValue = 1000;

        public bool AreAddressablesScenesPresent()
        {
            return addressableScenes != null && addressableScenes.Count != 0;
        }

#if UNITY_EDITOR
        public void UpdateAddressablesScenes()
        {
            AddressableAssetSettings settings;
            if (!addressablesSettingsObjectPath.Equals(""))
            {
                settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>(addressablesSettingsObjectPath);
            }
            else
            {
                settings = AddressableAssetSettingsDefaultObject.Settings;
            }
            if (settings == null)
            {
                Debug.LogError("Couldn't find Addressable Settings object.");
                return;
            }

            if (addressableScenes == null)
            {
                addressableScenes = new List<AddressablesScenesEnumInfo>();
            }

            List<AddressableAssetEntry> currentAddressablesScenes = GetListOfAddressableScenes(settings);
            Dictionary<string, AddressablesScenesEnumInfo> savedAddrScenesDict = CreateScenesDictionary(addressableScenes);
            List<AddressablesScenesEnumInfo> newSavedAddressablesScenes = new List<AddressablesScenesEnumInfo>();

            for (int i = 0; i < currentAddressablesScenes.Count; i++)
            {
                AddressablesScenesEnumInfo scenesEnumInfo;
                if (!savedAddrScenesDict.TryGetValue(currentAddressablesScenes[i].address, out scenesEnumInfo))
                {
                    scenesEnumInfo = new AddressablesScenesEnumInfo(currentAddressablesScenes[i].address, lastUsedValue);
                    ++lastUsedValue;
                }
                newSavedAddressablesScenes.Add(scenesEnumInfo);
            }

            addressableScenes = newSavedAddressablesScenes;
            addressableScenes.Sort(AddressablesScenesEnumInfo.Compare);

            EditorUtility.SetDirty(this);
        }

        public void ResetAddressablesScenes()
        {
            addressableScenes = new List<AddressablesScenesEnumInfo>();
            lastUsedValue = initialLastUsedValue;

            EditorUtility.SetDirty(this);
        }

        // Retrieves the list of all addressable scenes in the settings object.
        private static List<AddressableAssetEntry> GetListOfAddressableScenes(AddressableAssetSettings settings)
        {
            List<AddressableAssetEntry> allAssets = new List<AddressableAssetEntry>();
            settings.GetAllAssets(allAssets, true);

            List<AddressableAssetEntry> scenes = new List<AddressableAssetEntry>();
            for (int asset = 0; asset < allAssets.Count; asset++)
            {
                LookForScenes(allAssets[asset], scenes);
            }

            return scenes;
        }

        // Recursively look into the sub-assets of an addressable asset looking for scenes.
        private static void LookForScenes(AddressableAssetEntry asset, List<AddressableAssetEntry> scenes)
        {
            if (asset.IsScene)
            {
                if (!asset.IsInSceneList)
                {
                    scenes.Add(asset);
                    Debug.LogWarning(asset.ToString());
                }
                if (asset.SubAssets != null)
                {
                    for (int subAsset = 0; subAsset < asset.SubAssets.Count; subAsset++)
                    {
                        LookForScenes(asset.SubAssets[subAsset], scenes);
                    }
                }
            }
        }
#endif // UNITY_EDITOR

        // Creates a dictionary mapping scene paths to their proto enum value.
        private static Dictionary<string, AddressablesScenesEnumInfo> CreateScenesDictionary(List<AddressablesScenesEnumInfo> scenesList)
        {
            Dictionary<string, AddressablesScenesEnumInfo> sceneDictionary = new Dictionary<string, AddressablesScenesEnumInfo>();
            for (int i = 0; i < scenesList.Count; i++)
            {
                sceneDictionary.Add(scenesList[i].scenePath, scenesList[i]);
            }

            return sceneDictionary;
        }

#endif // APT_ADDRESSABLE_PACKAGE_PRESENT

        public bool GetUseAdvanced(ProtoMessageType type)
        {
            switch (type)
            {
                case ProtoMessageType.Annotation: return useAdvancedAnnotations;
                case ProtoMessageType.FidelityParams: return useAdvancedFidelityParameters;
                default: throw new System.ArgumentException("Unknown message type");
            }
        }

        public void SetUseAdvanceInstrumentationSettings(bool useAdvanced)
        {
            useAdvancedInstrumentationSettings = useAdvanced;
        }

        public void SetUseAdvanced(bool useAdvanced, ProtoMessageType type)
        {
            switch (type)
            {
                case ProtoMessageType.Annotation:
                    useAdvancedAnnotations = useAdvanced;
                    break;
                case ProtoMessageType.FidelityParams:
                    useAdvancedFidelityParameters = useAdvanced;
                    break;
                default: throw new System.ArgumentException("Unknown message type");
            }
        }
    }
}