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

namespace Google.Android.PerformanceParameters
{
    using UnityEngine;

    /// <summary>
    /// Editor / play settings.
    /// </summary>
    public class SetupConfig : ScriptableObject
    {
        /// <summary>
        /// If plugin is enabled or not.
        /// </summary>
        public bool PluginEnabled = true;

        /// <summary>
        /// Use base (quality settings) or advanced fidelity parameters.
        /// </summary>
        public bool UseAdvancedFidelityParameters;

        /// <summary>
        /// Use base (level) or advanced annotation.
        /// </summary>
        public bool UseAdvancedAnnotations;

        public bool GetUseAdvanced(ProtoMessageType type)
        {
            switch (type)
            {
                case ProtoMessageType.Annotation: return UseAdvancedAnnotations;
                case ProtoMessageType.FidelityParams: return UseAdvancedFidelityParameters;
                default: throw new System.ArgumentException("Unknown message type");
            }
        }

        public void SetUseAdvanced(bool useAdvanced, ProtoMessageType type)
        {
            switch (type)
            {
                case ProtoMessageType.Annotation:
                    UseAdvancedAnnotations = useAdvanced;
                    break;
                case ProtoMessageType.FidelityParams:
                    UseAdvancedFidelityParameters = useAdvanced;
                    break;
                default: throw new System.ArgumentException("Unknown message type");
            }
        }
    }
}