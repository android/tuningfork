//-----------------------------------------------------------------------
// <copyright file="DefaultLibraryMethods.cs" company="Google">
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

#if UNITY_EDITOR || !UNITY_ANDROID
using System;

namespace Google.Android.PerformanceTuner
{
    public class DefaultLibraryMethods : ILibraryMethods
    {
        public ErrorCode Init(FidelityParamsCallback fidelityParamsCallback,
            IntPtr trainingFidelityParams, IntPtr endpointUrlOverride)
        {
            return ErrorCode.PlatformNotSupported;
        }

        public ErrorCode InitWithSettings(ref CInitializationSettings settings)
        {
            return ErrorCode.PlatformNotSupported;
        }

        public ErrorCode GetFidelityParameters(
            ref CProtobufSerialization defaultParameters,
            ref CProtobufSerialization parameters,
            uint timeout)
        {
            return ErrorCode.PlatformNotSupported;
        }

        public ErrorCode SetCurrentAnnotation(ref CProtobufSerialization annotation)
        {
            return ErrorCode.PlatformNotSupported;
        }

        public ErrorCode FrameTick(InstrumentationKeys key)
        {
            return ErrorCode.PlatformNotSupported;
        }

        public ErrorCode Flush()
        {
            return ErrorCode.PlatformNotSupported;
        }

        public ErrorCode FindFidelityParamsInApk(string filename, ref CProtobufSerialization fidelityParameters)
        {
            return ErrorCode.PlatformNotSupported;
        }

        public bool SwappyIsEnabled()
        {
            return false;
        }

        public ErrorCode Destroy()
        {
            return ErrorCode.PlatformNotSupported;
        }

        public ErrorCode SetUploadCallback(UploadCallback uploadCallback)
        {
            return ErrorCode.PlatformNotSupported;
        }

        public ErrorCode SetFidelityParameters(ref CProtobufSerialization fidelityParameters)
        {
            return ErrorCode.PlatformNotSupported;
        }

        public ErrorCode FrameDeltaTimeNanos(InstrumentationKeys key, ulong dt)
        {
            return ErrorCode.PlatformNotSupported;
        }

        public ErrorCode StartTrace(InstrumentationKeys key, ref ulong handle)
        {
            return ErrorCode.PlatformNotSupported;
        }

        public ErrorCode EndTrace(ulong handle)
        {
            return ErrorCode.PlatformNotSupported;
        }

        public ErrorCode EnableMemoryRecording(bool enable)
        {
            return ErrorCode.PlatformNotSupported;
        }

        public ErrorCode StartRecordingLoadingTime(
            IntPtr eventMetadata,
            uint eventMetadataSize,
            ref CProtobufSerialization annotation,
            ref ulong handle)
        {
            return ErrorCode.PlatformNotSupported;
        }

        public ErrorCode StopRecordingLoadingTime(ulong handle)
        {
            return ErrorCode.PlatformNotSupported;
        }

        public ErrorCode ReportLifecycleEvent(LifecycleState state)
        {
            return ErrorCode.PlatformNotSupported;
        }

        public ErrorCode StartLoadingGroup(IntPtr eventMetadata, uint eventMetadataSize,
            IntPtr annotation, ref ulong handle)
        {
            return ErrorCode.PlatformNotSupported;
        }

        public ErrorCode StopLoadingGroup(ulong handle)
        {
            return ErrorCode.PlatformNotSupported;
        }
    }
}
#endif