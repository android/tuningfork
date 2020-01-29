//-----------------------------------------------------------------------
// <copyright file="AndroidLibraryMethods.cs" company="Google">
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

#if UNITY_ANDROID || UNITY_EDITOR
namespace Google.Android.PerformanceParameters
{
    using System;
    using System.Runtime.InteropServices;

    internal class AndroidLibraryMethods : ILibraryMethods
    {
        private const string Tuningfork = "unitytuningfork";

        [DllImport(Tuningfork)]
        private static extern int Unity_TuningFork_init(
            FidelityParamsCallback fidelityParamsCallback,
            IntPtr trainingFidelityParameters,
            IntPtr endpointUrlOverride);

        public TFErrorCode Init(FidelityParamsCallback fidelityParamsCallback,
            IntPtr trainingFidelityParameters, IntPtr endpointUrlOverride)
        {
            return (TFErrorCode)Unity_TuningFork_init(fidelityParamsCallback, trainingFidelityParameters, endpointUrlOverride);
        }

        [DllImport(Tuningfork)]
        private static extern int TuningFork_getFidelityParameters(
            ref CProtobufSerialization defaultParameters,
            ref CProtobufSerialization parameters,
            UInt32 timeout);

        public TFErrorCode GetFidelityParameters(
            ref CProtobufSerialization defaultParameters,
            ref CProtobufSerialization parameters,
            uint timeout)
        {
            return (TFErrorCode) TuningFork_getFidelityParameters(ref defaultParameters, ref parameters, timeout);
        }

        [DllImport(Tuningfork)]
        private static extern int TuningFork_setCurrentAnnotation(ref CProtobufSerialization annotation);

        public TFErrorCode SetCurrentAnnotation(ref CProtobufSerialization annotation)
        {
            return (TFErrorCode) TuningFork_setCurrentAnnotation(ref annotation);
        }

        [DllImport(Tuningfork)]
        private static extern int TuningFork_frameTick(UInt16 instrumentKey);

        public TFErrorCode FrameTick(InstrumentationKeys key)
        {
            return (TFErrorCode) TuningFork_frameTick((UInt16)key);
        }

        [DllImport(Tuningfork)]
        private static extern int TuningFork_flush();

        public TFErrorCode Flush()
        {
            return (TFErrorCode) TuningFork_flush();
        }

        [DllImport(Tuningfork)]
        private static extern int Unity_TuningFork_findFidelityParamsInApk(
            string filename, ref CProtobufSerialization fidelityParameters);

        public TFErrorCode FindFidelityParamsInApk(string filename, ref CProtobufSerialization fidelityParameters)
        {
            return (TFErrorCode) Unity_TuningFork_findFidelityParamsInApk(filename, ref fidelityParameters);
        }

        [DllImport(Tuningfork)]
        private static extern bool Unity_TuningFork_swappyIsEnabled();

        public bool SwappyIsEnabled()
        {
            return Unity_TuningFork_swappyIsEnabled();
        }

        [DllImport(Tuningfork)]
        private static extern int TuningFork_destroy();

        public TFErrorCode Destroy()
        {
            return (TFErrorCode) TuningFork_destroy();
        }

        [DllImport(Tuningfork)]
        private static extern int TuningFork_setUploadCallback(UploadCallback uploadCallback);

        public TFErrorCode SetUploadCallback(UploadCallback uploadCallback)
        {
            return (TFErrorCode) TuningFork_setUploadCallback(uploadCallback);
        }

        [DllImport(Tuningfork)]
        private static extern int TuningFork_setFidelityParameters(ref CProtobufSerialization fidelityParameters);

        public TFErrorCode SetFidelityParameters(ref CProtobufSerialization fidelityParameters)
        {
            return (TFErrorCode) TuningFork_setFidelityParameters(ref fidelityParameters);
        }
    }
}
#endif