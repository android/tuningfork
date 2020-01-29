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
namespace Google.Android.PerformanceParameters
{
    using System;

    internal class DefaultLibraryMethods : ILibraryMethods
    {
        public TFErrorCode Init(FidelityParamsCallback fidelityParamsCallback,
            IntPtr trainingFidelityParams, IntPtr endpointUrlOverride)
        {
            return TFErrorCode.PlatformNotSupported;
        }

        public TFErrorCode GetFidelityParameters(
            ref CProtobufSerialization defaultParameters,
            ref CProtobufSerialization parameters,
            uint timeout)
        {
            return TFErrorCode.PlatformNotSupported;
        }

        public TFErrorCode SetCurrentAnnotation(ref CProtobufSerialization annotation)
        {
            return TFErrorCode.PlatformNotSupported;
        }

        public TFErrorCode FrameTick(InstrumentationKeys key)
        {
            return TFErrorCode.PlatformNotSupported;
        }

        public TFErrorCode Flush()
        {
            return TFErrorCode.PlatformNotSupported;
        }

        public TFErrorCode FindFidelityParamsInApk(string filename, ref CProtobufSerialization fidelityParameters)
        {
            return TFErrorCode.PlatformNotSupported;
        }

        public bool SwappyIsEnabled()
        {
            return false;
        }

        public TFErrorCode Destroy()
        {
            return TFErrorCode.PlatformNotSupported;
        }

        public TFErrorCode SetUploadCallback(UploadCallback uploadCallback)
        {
            return TFErrorCode.PlatformNotSupported;
        }

        public TFErrorCode SetFidelityParameters(ref CProtobufSerialization fidelityParameters)
        {
            return TFErrorCode.PlatformNotSupported;
        }
    }
}
#endif