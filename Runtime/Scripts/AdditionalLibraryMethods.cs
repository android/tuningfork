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

namespace Google.Android.PerformanceTuner
{
    public class AdditionalLibraryMethods<TFidelity, TAnnotation>
        where TFidelity : class, IMessage<TFidelity>, new()
        where TAnnotation : class, IMessage<TAnnotation>, new()

    {
        readonly ILibraryMethods m_LibraryMethods;
        readonly List<IntPtr> m_Ptrs = new List<IntPtr>();

        public AdditionalLibraryMethods(ILibraryMethods libraryMethods)
        {
            m_LibraryMethods = libraryMethods;
        }

        public void FreePointers()
        {
            foreach (var ptr in m_Ptrs) Marshal.FreeHGlobal(ptr);
            m_Ptrs.Clear();
        }

        public ErrorCode Init(FidelityParamsCallback fidelityParamsCallback,
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

        public ErrorCode InitWithSettings(InitializationSettings settings)
        {
            CInitializationSettings cSettings = new CInitializationSettings()
            {
                persistent_cache = IntPtr.Zero, // Using default one.
                swappy_tracer_fn = null, // It will be set by native library.
                fidelity_params_callback = settings.fidelityParamsCallback,
                training_fidelity_params = IntPtr.Zero,
                endpoint_uri_override = IntPtr.Zero,
                swappy_version = 0, // It will be set by native library.
                max_num_metrics = settings.maxNumMetrics
            };

            if (settings.trainingFidelityParams != null)
            {
                CProtobufSerialization cs = CProtobufSerialization.Create(settings.trainingFidelityParams);
                cSettings.training_fidelity_params = Marshal.AllocHGlobal(Marshal.SizeOf(cs));
                Marshal.StructureToPtr(cs, cSettings.training_fidelity_params, true);
                m_Ptrs.Add(cSettings.training_fidelity_params);
            }

            if (!string.IsNullOrEmpty(settings.endpointUriOverride))
            {
                cSettings.endpoint_uri_override = Marshal.StringToHGlobalAnsi(settings.endpointUriOverride);
                m_Ptrs.Add(cSettings.endpoint_uri_override);
            }

            return m_LibraryMethods.InitWithSettings(ref cSettings);
        }

        public Result<TFidelity> FindFidelityParametersInApk(string filename)
        {
            var ps = new CProtobufSerialization();
            var errorCode = m_LibraryMethods.FindFidelityParamsInApk(filename, ref ps);

            TFidelity fp = null;
            if (errorCode == ErrorCode.Ok)
            {
                fp = ps.ParseMessage<TFidelity>();
                if (fp == null) errorCode = ErrorCode.InvalidFidelity;
            }

            CProtobufSerialization.CallDealloc(ref ps);
            return new Result<TFidelity>(errorCode, fp);
        }

        public Result<TFidelity> GetFidelityParameters(TFidelity defaultFidelity, UInt32 initialTimeoutMs)
        {
            var defaultPs = CProtobufSerialization.Create(defaultFidelity);
            var newPs = new CProtobufSerialization();
            var errorCode = m_LibraryMethods.GetFidelityParameters(
                ref defaultPs, ref newPs, initialTimeoutMs);
            TFidelity fp = null;
            if (errorCode == ErrorCode.Ok)
            {
                fp = newPs.ParseMessage<TFidelity>();
                if (fp == null) errorCode = ErrorCode.InvalidFidelity;
            }

            CProtobufSerialization.CallDealloc(ref newPs);
            CProtobufSerialization.CallDealloc(ref defaultPs);
            return new Result<TFidelity>(errorCode, fp);
        }

        public ErrorCode SetCurrentAnnotation(TAnnotation annotation)
        {
            if (MessageUtil.HasInvalidEnumField(annotation)) return ErrorCode.InvalidAnnotation;
            var ps = CProtobufSerialization.Create(annotation);
            var errorCode = m_LibraryMethods.SetCurrentAnnotation(ref ps);
            CProtobufSerialization.CallDealloc(ref ps);
            return errorCode;
        }

        public ErrorCode SetFidelityParameters(TFidelity fidelityParams)
        {
            if (MessageUtil.HasInvalidEnumField(fidelityParams)) return ErrorCode.InvalidFidelity;
            var ps = CProtobufSerialization.Create(fidelityParams);
            var errorCode = m_LibraryMethods.SetFidelityParameters(ref ps);
            CProtobufSerialization.CallDealloc(ref ps);
            return errorCode;
        }

        public Result<ulong> StartRecordingLoadingTime(LoadingTimeMetadata metadata, TAnnotation annotation)
        {
            IntPtr metadataPtr = IntPtr.Zero;
            uint metadataSize = 0;
            if (metadata != null)
            {
                metadataSize = (uint) Marshal.SizeOf(metadata);
                metadataPtr = Marshal.AllocHGlobal(Marshal.SizeOf(metadata));
                Marshal.StructureToPtr(metadata, metadataPtr, false);
            }

            var ps = CProtobufSerialization.Create(annotation);
            ulong handle = 0;
            var errorCode = m_LibraryMethods.StartRecordingLoadingTime(
                metadataPtr, metadataSize, ref ps, ref handle);
            CProtobufSerialization.CallDealloc(ref ps);

            if (metadataPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(metadataPtr);
            }

            return new Result<ulong>(errorCode, handle);
        }

        public Result<ulong> StartLoadingGroup(LoadingTimeMetadata metadata, TAnnotation annotation)
        {
            IntPtr metadataPtr = IntPtr.Zero;
            uint metadataSize = 0;
            if (metadata != null)
            {
                metadataPtr = Marshal.AllocHGlobal(Marshal.SizeOf(metadata));
                metadataSize = (uint) Marshal.SizeOf(metadata);
                Marshal.StructureToPtr(metadata, metadataPtr, false);
            }

            IntPtr annotationPtr = IntPtr.Zero;
            CProtobufSerialization ps = new CProtobufSerialization();
            if (annotation != null)
            {
                ps = CProtobufSerialization.Create(annotation);
                annotationPtr = Marshal.AllocHGlobal(Marshal.SizeOf(ps));
                Marshal.StructureToPtr(ps, annotationPtr, false);
            }

            ulong handle = 0;
            var errorCode = m_LibraryMethods.StartLoadingGroup(
                metadataPtr, metadataSize, annotationPtr, ref handle);

            if (metadataPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(metadataPtr);
            }

            if (annotationPtr != IntPtr.Zero)
            {
                CProtobufSerialization.CallDealloc(ref ps);
                Marshal.FreeHGlobal(annotationPtr);
            }

            return new Result<ulong>(errorCode, handle);
        }
    }
}