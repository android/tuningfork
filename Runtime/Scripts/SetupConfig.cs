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

using UnityEngine;
using UnityEngine.Serialization;

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

        public TunerMode mode = TunerMode.Insights;

        public bool GetUseAdvanced(ProtoMessageType type)
        {
            switch (type)
            {
                case ProtoMessageType.Annotation: return useAdvancedAnnotations;
                case ProtoMessageType.FidelityParams: return useAdvancedFidelityParameters;
                default: throw new System.ArgumentException("Unknown message type");
            }
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