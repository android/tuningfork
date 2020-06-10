//-----------------------------------------------------------------------
// <copyright file="ILibraryMethods.cs" company="Google">
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

namespace Google.Android.PerformanceTuner
{
    public delegate void FidelityParamsCallback(ref CProtobufSerialization ps);

    public delegate void UploadCallback(IntPtr data, UInt32 size);

    public interface ILibraryMethods
    {
        ErrorCode Init(
            FidelityParamsCallback setFidelityParams,
            IntPtr trainingFidelityParameters,
            IntPtr endpointUrlOverride);

        ErrorCode GetFidelityParameters(
            ref CProtobufSerialization defaultParameters,
            ref CProtobufSerialization parameters,
            UInt32 timeout);

        ErrorCode SetCurrentAnnotation(ref CProtobufSerialization annotation);

        ErrorCode FrameTick(InstrumentationKeys key);

        ErrorCode Flush();

        ErrorCode FindFidelityParamsInApk(string filename, ref CProtobufSerialization fp);

        bool SwappyIsEnabled();

        ErrorCode Destroy();

        ErrorCode SetUploadCallback(UploadCallback uploadCallback);

        ErrorCode SetFidelityParameters(ref CProtobufSerialization fp);

        ErrorCode FrameDeltaTimeNanos(InstrumentationKeys key, UInt64 timeNanos);

        ErrorCode StartTrace(InstrumentationKeys key, ref UInt64 handle);

        ErrorCode EndTrace(UInt64 handle);

        ErrorCode EnableMemoryRecording(bool enable);
    }
}