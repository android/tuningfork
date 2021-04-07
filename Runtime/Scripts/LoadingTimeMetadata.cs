//-----------------------------------------------------------------------
// <copyright file="LoadingTimeMetadata.cs" company="Google">
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

using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming.
namespace Google.Android.PerformanceTuner
{
    /// <summary>
    ///    Metadata recorded with a loading time event.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class LoadingTimeMetadata
    {
        public enum LoadingState
        {
            Unknown = 0,

            /// <summary>
            ///     The first time the game is run.
            /// </summary>
            FirstRun = 1,

            /// <summary>
            ///     App is not backgrounded.
            /// </summary>
            ColdStart = 2,

            /// <summary>
            ///     App is backgrounded.
            /// </summary>
            WarmStart = 3,

            /// <summary>
            ///     App is backgrounded, least work needed.
            /// </summary>
            HotStart = 4,

            /// <summary>
            ///     Asset loading between levels.
            /// </summary>
            InterLevel = 5
        }

        public LoadingState state;

        public enum LoadingSource
        {
            UnknownSource = 0,

            /// <summary>
            ///     Uncompressing data.
            /// </summary>
            Memory = 1,

            /// <summary>
            ///     Reading assets from APK bundle.
            /// </summary>
            Apk = 2,

            /// <summary>
            ///     Reading assets from device storage.
            /// </summary>
            DeviceStorage = 3,

            /// <summary>
            ///     Reading assets from external storage, for example SD card.
            /// </summary>
            ExternalStorage = 4,

            /// <summary>
            ///     Loading assets from the network.
            /// </summary>
            Network = 5,

            /// <summary>
            ///     Shader compilation.
            /// </summary>
            ShaderCompilation = 6,

            /// <summary>
            ///     Time spent between process starting and onCreate.
            /// </summary>
            PreActivity = 7,

            /// <summary>
            ///     Total time spent between process starting and first render frame.
            /// </summary>
            FirstTouchToFirstFrame = 8,

            /// <summary>
            ///     Time from start to end of a group of events.
            /// </summary>
            TotalUserWaitForGroup = 9
        }

        public LoadingSource source;

        /// <summary>
        ///     0 = no compression, 100 = max compression
        /// </summary>
        public int compression_level;

        public enum NetworkConnectivity
        {
            Unknown = 0,
            Wifi = 1,
            CellularNetwork = 2
        }

        public NetworkConnectivity network_connectivity;

        /// <summary>
        ///     Bandwidth in bits per second.
        /// </summary>
        public ulong network_transfer_speed_bps;

        /// <summary>
        ///     Latency in nanoseconds.
        /// </summary>
        public ulong network_latency_ns;
    }
}
