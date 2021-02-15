//-----------------------------------------------------------------------
// <copyright file="InitializationSettings.cs" company="Google">
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

namespace Google.Android.PerformanceTuner
{
    /// <summary>
    ///     Initialization settings.
    /// </summary>
    public struct InitializationSettings
    {
        /// <summary>
        ///     Callback
        ///     If set, this is called with the fidelity parameters that are downloaded.
        ///     If null, you need to call TuningFork_getFidelityParameters yourself.
        /// </summary>ß
        public FidelityParamsCallback fidelityParamsCallback;

        /// <summary>
        ///     A serialized protobuf containing the fidelity parameters to be uploaded for training.
        ///     Set this to nullptr if you are not using training mode.
        ///     In training mode, these parameters are taken to be the parameters used within the game
        ///     and they are used to help suggest parameter changes for different devices. Note that
        ///     these override the default parameters loaded from the APK at startup.
        /// </summary>
        public IMessage trainingFidelityParams;

        /// <summary>
        ///     A null-terminated UTF-8 string containing the endpoint that Tuning Fork will connect
        ///     to for parameter, upload and debug requests. This overrides the value in base_uri in
        ///     the settings proto and is intended for debugging purposes only.
        /// </summary>
        public string endpointUriOverride;

        /// <summary>
        ///     The number of each metric that is allowed to be allocated at any given time. If any
        ///     element is zero, the default for that metric type will be used. Memory for all metrics
        ///     is allocated up-front at initialization. When all metrics of a given type are allocated,
        ///     further requested metrics will not be added and data will be lost.
        /// </summary>
        public MetricLimits maxNumMetrics;
    }
}