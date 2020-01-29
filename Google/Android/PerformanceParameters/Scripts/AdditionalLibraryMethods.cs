//-----------------------------------------------------------------------
// <copyright file="AdditionalLibraryMethods.cs" company="Google">
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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Google.Android.PerformanceParameters
{
    public class AdditionalLibraryMethods
    {
        private readonly ILibraryMethods m_LibraryMethods;
        private List<IntPtr> m_Ptrs = new List<IntPtr>();

        public AdditionalLibraryMethods(ILibraryMethods libraryMethods)
        {
            m_LibraryMethods = libraryMethods;
        }

        public void FreePointers()
        {
            foreach(var ptr in m_Ptrs) Marshal.FreeHGlobal(ptr);
        }

        public TFErrorCode Init(FidelityParamsCallback fidelityParamsCallback,
            IMessage trainingFidelityParameters, String endpointUrlOverride)
        {
            IntPtr trainingFidelityParametersPtr = IntPtr.Zero;
            if (trainingFidelityParameters != null)
            {
                CProtobufSerialization cs = CProtobufSerialization.Create(trainingFidelityParameters);
                trainingFidelityParametersPtr = Marshal.AllocHGlobal(Marshal.SizeOf(cs));
                Marshal.StructureToPtr(cs, trainingFidelityParametersPtr, true);
                m_Ptrs.Add(trainingFidelityParametersPtr);
            }
            IntPtr endpointUrlOverridePtr = IntPtr.Zero;
            if (!string.IsNullOrEmpty(endpointUrlOverride))
            {
                endpointUrlOverridePtr = Marshal.StringToHGlobalAnsi(endpointUrlOverride);
                m_Ptrs.Add(endpointUrlOverridePtr);
            }

            return m_LibraryMethods.Init(fidelityParamsCallback, trainingFidelityParametersPtr, endpointUrlOverridePtr);
        }

        public Result<FidelityParams> FindFidelityParametersInApk(string filename)
        {
            var ps = new CProtobufSerialization();
            var errorCode = m_LibraryMethods.FindFidelityParamsInApk(filename, ref ps);

            FidelityParams fp = null;
            if (errorCode == TFErrorCode.Ok)
            {
                fp = ps.ParseMessage(FidelityParams.Parser);
            }

            CProtobufSerialization.CallDealloc(ref ps);
            if (fp == null) return new Result<FidelityParams>(TFErrorCode.BadParameter, null);
            return new Result<FidelityParams>(errorCode, fp);
        }

        public Result<FidelityParams> GetFidelityParameters(FidelityParams defaultFidelity, UInt32 initialTimeoutMs)
        {
            var defaultPs = CProtobufSerialization.Create(defaultFidelity);
            var newPs = new CProtobufSerialization();
            var errorCode = m_LibraryMethods.GetFidelityParameters(
                ref defaultPs, ref newPs, initialTimeoutMs);
            FidelityParams fp = null;
            if (errorCode == TFErrorCode.Ok)
            {
                fp = newPs.ParseMessage(FidelityParams.Parser);
            }

            CProtobufSerialization.CallDealloc(ref newPs);
            CProtobufSerialization.CallDealloc(ref defaultPs);
            if (fp == null) return new Result<FidelityParams>(TFErrorCode.BadParameter, null);
            return new Result<FidelityParams>(errorCode, fp);
        }

        public TFErrorCode SetCurrentAnnotation(Annotation annotation)
        {
            var ps = CProtobufSerialization.Create(annotation);
            var errorCode = m_LibraryMethods.SetCurrentAnnotation(ref ps);
            CProtobufSerialization.CallDealloc(ref ps);
            return errorCode;
        }

        public TFErrorCode SetFidelityParameters(FidelityParams fidelityParams)
        {
            var ps = CProtobufSerialization.Create(fidelityParams);
            var errorCode = m_LibraryMethods.SetFidelityParameters(ref ps);
            CProtobufSerialization.CallDealloc(ref ps);
            return errorCode;
        }
    }
}