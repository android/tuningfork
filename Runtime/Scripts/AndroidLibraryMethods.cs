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
using System;
using System.Runtime.InteropServices;

namespace Google.Android.PerformanceTuner
{
    public class AndroidLibraryMethods : ILibraryMethods
    {
        const string PerformanceTuner = "unitytuningfork";

        [DllImport(PerformanceTuner)]
        static extern int Unity_TuningFork_init(
            FidelityParamsCallback fidelityParamsCallback,
            IntPtr trainingFidelityParameters,
            IntPtr endpointUrlOverride);

        public ErrorCode Init(FidelityParamsCallback fidelityParamsCallback,
            IntPtr trainingFidelityParameters, IntPtr endpointUrlOverride)
        {
            return (ErrorCode) Unity_TuningFork_init(fidelityParamsCallback, trainingFidelityParameters,
                endpointUrlOverride);
        }

        [DllImport(PerformanceTuner)]
        static extern int TuningFork_getFidelityParameters(
            ref CProtobufSerialization defaultParameters,
            ref CProtobufSerialization parameters,
            UInt32 timeout);

        public ErrorCode GetFidelityParameters(
            ref CProtobufSerialization defaultParameters,
            ref CProtobufSerialization parameters,
            uint timeout)
        {
            return (ErrorCode) TuningFork_getFidelityParameters(ref defaultParameters, ref parameters, timeout);
        }

        [DllImport(PerformanceTuner)]
        static extern int TuningFork_setCurrentAnnotation(ref CProtobufSerialization annotation);

        public ErrorCode SetCurrentAnnotation(ref CProtobufSerialization annotation)
        {
            return (ErrorCode) TuningFork_setCurrentAnnotation(ref annotation);
        }

        [DllImport(PerformanceTuner)]
        static extern int TuningFork_frameTick(UInt16 instrumentKey);

        public ErrorCode FrameTick(InstrumentationKeys key)
        {
            return (ErrorCode) TuningFork_frameTick((UInt16) key);
        }

        [DllImport(PerformanceTuner)]
        static extern int TuningFork_flush();

        public ErrorCode Flush()
        {
            return (ErrorCode) TuningFork_flush();
        }

        [DllImport(PerformanceTuner)]
        static extern int Unity_TuningFork_findFidelityParamsInApk(
            string filename, ref CProtobufSerialization fidelityParameters);

        public ErrorCode FindFidelityParamsInApk(string filename, ref CProtobufSerialization fidelityParameters)
        {
            return (ErrorCode) Unity_TuningFork_findFidelityParamsInApk(filename, ref fidelityParameters);
        }

        [DllImport(PerformanceTuner)]
        static extern bool Unity_TuningFork_swappyIsEnabled();

        public bool SwappyIsEnabled()
        {
            return Unity_TuningFork_swappyIsEnabled();
        }

        [DllImport(PerformanceTuner)]
        static extern int TuningFork_destroy();

        public ErrorCode Destroy()
        {
            return (ErrorCode) TuningFork_destroy();
        }

        [DllImport(PerformanceTuner)]
        static extern int TuningFork_setUploadCallback(UploadCallback uploadCallback);

        public ErrorCode SetUploadCallback(UploadCallback uploadCallback)
        {
            return (ErrorCode) TuningFork_setUploadCallback(uploadCallback);
        }

        [DllImport(PerformanceTuner)]
        static extern int TuningFork_setFidelityParameters(ref CProtobufSerialization fidelityParameters);

        public ErrorCode SetFidelityParameters(ref CProtobufSerialization fidelityParameters)
        {
            return (ErrorCode) TuningFork_setFidelityParameters(ref fidelityParameters);
        }

        [DllImport(PerformanceTuner)]
        static extern int TuningFork_frameDeltaTimeNanos(ushort key, ulong dt);

        public ErrorCode FrameDeltaTimeNanos(InstrumentationKeys key, ulong dt)
        {
            return (ErrorCode) TuningFork_frameDeltaTimeNanos((ushort) key, dt);
        }

        [DllImport(PerformanceTuner)]
        static extern int TuningFork_startTrace(ushort key, ref ulong handle);

        public ErrorCode StartTrace(InstrumentationKeys key, ref ulong handle)
        {
            return (ErrorCode) TuningFork_startTrace((ushort) key, ref handle);
        }

        [DllImport(PerformanceTuner)]
        static extern int TuningFork_endTrace(ulong handle);

        public ErrorCode EndTrace(ulong handle)
        {
            return (ErrorCode) TuningFork_endTrace(handle);
        }

        [DllImport(PerformanceTuner)]
        static extern int TuningFork_enableMemoryRecording(bool handle);

        public ErrorCode EnableMemoryRecording(bool enable)
        {
            return (ErrorCode) TuningFork_enableMemoryRecording(enable);
        }
    }
}
#endif