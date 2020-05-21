//-----------------------------------------------------------------------
// <copyright file="InstrumentationKeys.cs" company="Google">
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

namespace Google.Android.PerformanceTuner
{
    /// <summary>
    ///     Instrument keys indicating time periods within a frame.
    ///     Keys 64000-65535 are reserved
    /// </summary>
    public enum InstrumentationKeys
    {
        /// <summary>
        ///     If GPU time is available, this is MAX(CpuTime, GpuTime).
        ///     If not, this is the same as PacedFrameTime
        /// </summary>
        RawFrameTime = 64000,

        /// <summary>
        ///     Frame time between ends of eglSwapBuffers calls or Vulkan queue present.
        /// </summary>
        PacedFrameTime = 64001,

        /// <summary>
        ///     The time between frame start and the call to Swappy_swap.
        /// </summary>
        CpuTime = 64002,

        /// <summary>
        ///     The time between buffer swap and GPU fence triggering.
        /// </summary>
        GpuTime = 64003,
    }
}