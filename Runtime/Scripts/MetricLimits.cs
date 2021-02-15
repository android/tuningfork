//-----------------------------------------------------------------------
// <copyright file="MetricLimits.cs" company="Google">
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

namespace Google.Android.PerformanceTuner
{
    /// <summary>
    ///     Limits on the number of metrics of each type.
    ///     Zero any values to get the default for that type:
    ///         Frame time: min(64, the maximum number of annotation combinations) * num_instrument_keys.
    ///         Loading time: 32.
    ///         Memory: 15 possible memory metrics.
    ///         Battery: 32.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MetricLimits
    {
        public uint frame_time;
        public uint loading_time;
        public uint memory;
        public uint battery;
    }
}