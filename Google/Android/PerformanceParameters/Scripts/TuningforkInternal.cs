//-----------------------------------------------------------------------
// <copyright file="TuningforkInternal.cs" company="Google">
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
using System.Collections;
using AOT;
using Google.Protobuf;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Google.Android.PerformanceParameters
{
    /// <summary>
    ///     Internal part
    /// </summary>
    public partial class Tuningfork
    {
        // Use only for marshalling static delegates.
        private static Tuningfork _activeTf;
        private readonly AdditionalLibraryMethods m_AdditionalLibraryMethods;
        private readonly FidelityParamsCallback m_FpCallback = FidelityParamsCallbackImpl;
        private readonly ILibraryMethods m_Library;
        private readonly UploadCallback m_UploadCallback = UploadCallbackImpl;
        private Action m_OnStop;
        private FrameTracer m_SceneObject;

        public Tuningfork()
        {
            _activeTf = this;
            m_Library =
#if UNITY_ANDROID && !UNITY_EDITOR
                new AndroidLibraryMethods();
#else
                new DefaultLibraryMethods();
#endif
            m_AdditionalLibraryMethods = new AdditionalLibraryMethods(m_Library);
        }

        private TFErrorCode StartInternal()
        {
            var setupConfig = Resources.Load("SetupConfig") as SetupConfig;

            if (setupConfig == null)
            {
                Debug.LogWarning(
                    "SetupConfig can not be loaded, open Google->Tuningfork to setup the plugin.");
                return TFErrorCode.NoSettings;
            }

            if (!setupConfig.PluginEnabled)
            {
                Debug.LogWarning(
                    "Tuningfork plugin is not enabled, open Google->Tuningfork to enable the plugin.");
                return TFErrorCode.TuningforkNotInitialized;
            }

            IMessage defaultQualityParameters = null;
            if (!setupConfig.UseAdvancedFidelityParameters)
            {
                defaultQualityParameters = new FidelityParams();
                MessageUtil.SetQualityLevel(defaultQualityParameters, QualitySettings.GetQualityLevel());
            }

            var errorCode = m_AdditionalLibraryMethods.Init(m_FpCallback, defaultQualityParameters, null);

            if (errorCode != TFErrorCode.Ok)
            {
                m_AdditionalLibraryMethods.FreePointers();
                return errorCode;
            }
            m_OnStop += () => m_AdditionalLibraryMethods.FreePointers();

            CreateSceneObject();
            m_SceneObject.StartCoroutine(CallbacksCheck());

            if (!SwappyIsEnabled()) EnableUnityFrameTicks();
            if (!setupConfig.UseAdvancedAnnotations) EnableDefaultAnnotationsMode();
            if (!setupConfig.UseAdvancedFidelityParameters) EnableDefaultFidelityMode();

            AddUploadCallback();

            return errorCode;
        }

        private void EnableDefaultAnnotationsMode()
        {
            SceneManager.activeSceneChanged += OnSceneChanged;
            OnSceneChanged(SceneManager.GetActiveScene(), SceneManager.GetActiveScene());
            m_OnStop += () => { SceneManager.activeSceneChanged -= OnSceneChanged; };
        }

        private void AddUploadCallback()
        {
            var errorCode = m_Library.SetUploadCallback(m_UploadCallback);
            if (errorCode != TFErrorCode.Ok)
                Debug.LogWarningFormat("Tuningfork: Could not set upload callback, status {0}", errorCode);
        }

        private void EnableDefaultFidelityMode()
        {
            OnReceiveFidelityParameters += UpdateQualityLevel;
            m_OnStop += () => { OnReceiveFidelityParameters -= UpdateQualityLevel; };
        }

        private void EnableUnityFrameTicks()
        {
            m_SceneObject.StartCoroutine(UnityFrameTick());
        }

        private void CreateSceneObject()
        {
            if (m_SceneObject != null) return;
            GameObject gameObject = new GameObject("Tuningfork");
            m_SceneObject = gameObject.AddComponent<FrameTracer>();
            GameObject.DontDestroyOnLoad(gameObject);
            m_OnStop += () =>
            {
                if (m_SceneObject != null)
                {
                    m_SceneObject.StopAllCoroutines();
                    GameObject.Destroy(m_SceneObject.gameObject);
                    m_SceneObject = null;
                }
            };
        }

        private void UpdateQualityLevel(FidelityParams message)
        {
            if (message == null) return;
            var qualityLevel = MessageUtil.GetQualityLevel(message);
            QualitySettings.SetQualityLevel(qualityLevel);
        }

        private void OnSceneChanged(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to)
        {
            var annotation = new Annotation();
            MessageUtil.SetScene(annotation, to.buildIndex);
            MessageUtil.SetLoadingState(annotation, MessageUtil.LoadingState.NotLoading);
            SetCurrentAnnotation(annotation);
        }

        /// <summary>
        ///     Used if swappy is not available.
        /// </summary>
        private IEnumerator UnityFrameTick()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                FrameTick(InstrumentationKeys.UNITY_FRAME);
            }
        }

        private FidelityParams m_ReceivedFidelityParameters = null;
        private UploadTelemetryRequest m_UploadTelemetryRequest = null;

        /// <summary>
        ///     Check if new received fidelity parameters or upload telemetry request were stored, 
        ///     and call their callbacks. The C# callbacks are not called directly from the native
        ///     TuningFork callbacks to avoid crashes due to running in a different thread 
        ///     than what Unity is expecting.
        /// </summary>
        private IEnumerator CallbacksCheck()
        {
            while (true)
            {

                yield return new WaitForFixedUpdate();
                if (m_ReceivedFidelityParameters != null)
                {
                    if (OnReceiveFidelityParameters != null) OnReceiveFidelityParameters(m_ReceivedFidelityParameters);
                    m_ReceivedFidelityParameters = null;
                }
                if (m_UploadTelemetryRequest != null)
                {
                    if (OnReceiveUploadLog != null) OnReceiveUploadLog(m_UploadTelemetryRequest);
                    m_UploadTelemetryRequest = null;
                }
            }
        }

        // These callbacks must be static as il2cpp can not marshall non-static delegates.
        [MonoPInvokeCallback(typeof(UploadCallback))]
        private static void UploadCallbackImpl(IntPtr bytes, uint size)
        {
            if (_activeTf == null || _activeTf.OnReceiveUploadLog == null) return;
            // Don't call OnReceiveUploadLog directly from this thread.
            _activeTf.m_UploadTelemetryRequest = UploadTelemetryRequest.Parse(bytes, size);

        }

        [MonoPInvokeCallback(typeof(FidelityParamsCallback))]
        private static void FidelityParamsCallbackImpl(ref CProtobufSerialization ps)
        {
            if (_activeTf == null && _activeTf.OnReceiveUploadLog == null) return;
            // Don't call OnReceiveFidelityParameters directly from this thread.
            _activeTf.m_ReceivedFidelityParameters = ps.ParseMessage(FidelityParams.Parser);
        }
    }
}