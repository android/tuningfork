//-----------------------------------------------------------------------
// <copyright file="MetricLimits.cs" company="Google">
//
// Copyright 2023 Google Inc. All Rights Reserved.
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
using System.Runtime.InteropServices;
using Google.Protobuf;
using UnityEngine;

namespace Google.Android.PerformanceTuner
{
    /// <summary>
    /// A structure to match the struct returned from the predictability API.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct QualityLevelPredictionsCStruct
    {
        public IntPtr fidelityParams;
        public IntPtr predictedTimeUs;
        public uint size;
    }

    /// <summary>
    /// A structure holding a fidelity and the corresponding predicted frame time
    /// in microseconds. In some cases when the parsing of fidelity has gone wrong,
    /// fidelity member will be null.
    /// </summary>
    public class QualityLevelPredictions<TFidelity>
            where TFidelity : class, IMessage<TFidelity>, new()
    {
        public TFidelity fidelity;
        public int predictedTimeUs;
    }
}
