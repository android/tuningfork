//-----------------------------------------------------------------------
// <copyright file="AndroidPerformanceTunerInternal.cs" company="Google">
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
using UnityEngine.Scripting;

namespace Google.Android.PerformanceTuner
{
    /// <summary>
    ///     Internal part
    /// </summary>
    public partial class AndroidPerformanceTuner<TFidelity, TAnnotation>
        where TFidelity : class, IMessage<TFidelity>, new()
        where TAnnotation : class, IMessage<TAnnotation>, new()
    {
        readonly AdditionalLibraryMethods<TFidelity, TAnnotation> m_AdditionalLibraryMethods;
        readonly ILibraryMethods m_Library;

        Action m_OnStop;
        FrameTracer m_SceneObject;
        SetupConfig m_SetupConfig;
        string m_endPoint = null;
        MetricLimits m_MetricLimits;
        const string k_LocalEndPoint = "http://localhost:9000";

        // For callbacks only.
        static AndroidPerformanceTuner<TFidelity, TAnnotation> s_Tuner;

        public AndroidPerformanceTuner()
        {
            s_Tuner = this;
            m_Library =
#if UNITY_ANDROID && !UNITY_EDITOR
                new AndroidLibraryMethods();
#else
                new DefaultLibraryMethods();
#endif
            m_AdditionalLibraryMethods = new AdditionalLibraryMethods<TFidelity, TAnnotation>(m_Library);
            Callbacks.onFidelityParamsReceived = FidelityParamsCallbackImpl;
            Callbacks.onUploadReceived = UploadCallbackImpl;
        }

        ErrorCode StartInternal()
        {
            m_SetupConfig = Resources.Load("SetupConfig") as SetupConfig;

            if (m_SetupConfig == null)
            {
                Debug.LogWarning(
                    "SetupConfig can not be loaded, open Google->Android Performance Tuner to setup the plugin.");
                return ErrorCode.NoSettings;
            }

            if (!m_SetupConfig.pluginEnabled)
            {
                Debug.LogWarning(
                    "Android Performance Tuner plugin is not enabled, open Google->Android Performance Tuner to enable the plugin.");
                return ErrorCode.TuningforkNotInitialized;
            }

            var annotationStatus = CheckAnnotationMessage(m_SetupConfig);
            var fidelityStatus = CheckFidelityMessage(m_SetupConfig);

            if (annotationStatus != ErrorCode.Ok) return annotationStatus;
            if (fidelityStatus != ErrorCode.Ok) return fidelityStatus;


            InitializationSettings settings = new InitializationSettings();

            if (!m_SetupConfig.useAdvancedFidelityParameters)
            {
                settings.trainingFidelityParams = new TFidelity();
                MessageUtil.SetQualityLevel(settings.trainingFidelityParams, QualitySettings.GetQualityLevel());
            }

            settings.endpointUriOverride = m_endPoint;
            if (m_SetupConfig.mode == TunerMode.Experiments)
                settings.fidelityParamsCallback = Callbacks.FidelityParamsCallbackImpl;
            settings.maxNumMetrics = m_MetricLimits;


            var errorCode = m_AdditionalLibraryMethods.InitWithSettings(settings);

            if (errorCode != ErrorCode.Ok)
            {
                m_AdditionalLibraryMethods.FreePointers();
                return errorCode;
            }

            m_OnStop += () => m_AdditionalLibraryMethods.FreePointers();

            CreateSceneObject();
            m_SceneObject.StartCoroutine(CallbacksCheck());

            if (!SwappyIsEnabled()) EnableUnityFrameTicks();
            if (!m_SetupConfig.useAdvancedAnnotations) EnableDefaultAnnotationsMode();
            if (!m_SetupConfig.useAdvancedFidelityParameters) EnableDefaultFidelityMode();

            AddAutoFlush();
            AddAutoLifecycleUpdate();
            CheckNetworkReachability();

            return errorCode;
        }

        /// <summary>
        ///     Required to add android.permission.ACCESS_NETWORK_STATE permission to AndroidManifest.
        /// </summary>
        [Preserve]
        void CheckNetworkReachability()
        {
            var internetReachability = Application.internetReachability;
            Debug.LogFormat("internet reachability is {0}", internetReachability);
        }

        ErrorCode CheckAnnotationMessage(SetupConfig config)
        {
            if (config.useAdvancedAnnotations) return ErrorCode.Ok;
            if (!MessageUtil.HasLoadingState<TAnnotation>())
            {
                Debug.LogError("Android Performance Tuner is using default annotation, " +
                               "but Annotation message doesn't contain loading state parameter.");
                return ErrorCode.InvalidAnnotation;
            }

            if (!MessageUtil.HasScene<TAnnotation>())
            {
                Debug.LogError("Android Performance Tuner is using default annotation, " +
                               "but Annotation message doesn't contain scene parameter.");
                return ErrorCode.InvalidAnnotation;
            }

            return ErrorCode.Ok;
        }

        ErrorCode CheckFidelityMessage(SetupConfig config)
        {
            if (config.useAdvancedFidelityParameters) return ErrorCode.Ok;

            if (!MessageUtil.HasQualityLevel<TFidelity>())
            {
                Debug.LogError("Android Performance Tuner is using default fidelity, " +
                               "but Fidelity message doesn't contain level parameter.");
                return ErrorCode.InvalidFidelity;
            }

            return ErrorCode.Ok;
        }

        void EnableDefaultAnnotationsMode()
        {
            SceneManager.activeSceneChanged += OnSceneChanged;
            OnSceneChanged(SceneManager.GetActiveScene(), SceneManager.GetActiveScene());
            m_OnStop += () => { SceneManager.activeSceneChanged -= OnSceneChanged; };
        }

        ErrorCode AddUploadCallback()
        {
            var errorCode = m_Library.SetUploadCallback(Callbacks.UploadCallbackImpl);
            if (errorCode != ErrorCode.Ok)
                Debug.LogWarningFormat("Android Performance Tuner: Could not set upload callback, status {0}",
                    errorCode);
            return errorCode;
        }

        void AddAutoFlush()
        {
            m_SceneObject.onAppInBackground += () =>
            {
                ErrorCode code = m_Library.Flush();
                Debug.LogFormat("Flush in background {0}", code);
            };
        }

        void AddAutoLifecycleUpdate()
        {
            m_Library.ReportLifecycleEvent(LifecycleState.OnCreate);
            m_Library.ReportLifecycleEvent(LifecycleState.OnStart);
            m_SceneObject.onLifecycleChanged += (state) =>
            {
                ErrorCode code = m_Library.ReportLifecycleEvent(state);
                if (code != ErrorCode.Ok)
                    Debug.LogFormat("ReportLifecycleEvent({0}) errorCode is {1}", state, code);
            };
            m_OnStop += () => { m_Library.ReportLifecycleEvent(LifecycleState.OnDestroy); };
        }

        void EnableDefaultFidelityMode()
        {
            onReceiveFidelityParameters += UpdateQualityLevel;
            m_OnStop += () => { onReceiveFidelityParameters -= UpdateQualityLevel; };

            m_SceneObject.StartCoroutine(QualitySettingsCheck());
        }

        void EnableUnityFrameTicks()
        {
            m_SceneObject.StartCoroutine(UnityFrameTick());
        }

        void CreateSceneObject()
        {
            if (m_SceneObject != null) return;
            GameObject gameObject = new GameObject("Android Performance Tuner");
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

        void UpdateQualityLevel(TFidelity message)
        {
            if (message == null) return;
            var qualityLevel = MessageUtil.GetQualityLevel(message);
            QualitySettings.SetQualityLevel(qualityLevel);
        }

        MessageUtil.LoadingState m_DefaultAnnotationLoadingState = MessageUtil.LoadingState.NotLoading;

        void OnSceneChanged(UnityEngine.SceneManagement.Scene from, UnityEngine.SceneManagement.Scene to)
        {
            var annotation = new TAnnotation();
            MessageUtil.SetScene(annotation, to.buildIndex);
            MessageUtil.SetLoadingState(annotation, m_DefaultAnnotationLoadingState);
            var errorCode = m_AdditionalLibraryMethods.SetCurrentAnnotation(annotation);
            if (errorCode != ErrorCode.Ok)
                Debug.LogErrorFormat("SetCurrentAnnotation({0}) result is {1}", annotation, errorCode);
        }

        ErrorCode SetDefaultAnnotation(MessageUtil.LoadingState state)
        {
            var annotation = new TAnnotation();
            MessageUtil.SetScene(annotation, SceneManager.GetActiveScene().buildIndex);
            MessageUtil.SetLoadingState(annotation, state);
            m_DefaultAnnotationLoadingState = state;
            var errorCode = m_AdditionalLibraryMethods.SetCurrentAnnotation(annotation);
            if (errorCode != ErrorCode.Ok)
                Debug.LogErrorFormat("SetCurrentAnnotation({0}) result is {1}", annotation, errorCode);
            return errorCode;
        }


        /// <summary>
        ///     Used if swappy is not available.
        /// </summary>
        IEnumerator UnityFrameTick()
        {
            while (true)
            {
                yield return new WaitForEndOfFrame();
                FrameTick(InstrumentationKeys.RawFrameTime);
            }
        }

        IEnumerator QualitySettingsCheck()
        {
            int currentLevel = QualitySettings.GetQualityLevel();
            while (true)
            {
                yield return new WaitForEndOfFrame();
                if (currentLevel != QualitySettings.GetQualityLevel())
                {
                    currentLevel = QualitySettings.GetQualityLevel();
                    TFidelity message = new TFidelity();
                    MessageUtil.SetQualityLevel(message, QualitySettings.GetQualityLevel());
                    var errorCode = m_AdditionalLibraryMethods.SetFidelityParameters(message);
                    if (errorCode != ErrorCode.Ok)
                        Debug.LogErrorFormat("SetFidelityParameters({0}) result is {1}", message, errorCode);
                }
            }
        }

        TFidelity m_ReceivedFidelityParameters = null;
        UploadTelemetryRequest m_UploadTelemetryRequest = null;

        /// <summary>
        ///     Check if new received fidelity parameters or upload telemetry request were stored, 
        ///     and call their callbacks. The C# callbacks are not called directly from the native
        ///     TuningFork callbacks to avoid crashes due to running in a different thread 
        ///     than what Unity is expecting.
        /// </summary>
        IEnumerator CallbacksCheck()
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();
                if (m_ReceivedFidelityParameters != null)
                {
                    if (onReceiveFidelityParameters != null) onReceiveFidelityParameters(m_ReceivedFidelityParameters);
                    m_ReceivedFidelityParameters = null;
                }

                if (m_UploadTelemetryRequest != null)
                {
                    if (onReceiveUploadLog != null) onReceiveUploadLog(m_UploadTelemetryRequest);
                    m_UploadTelemetryRequest = null;
                }
            }
        }


        static void UploadCallbackImpl(IntPtr bytes, uint size)
        {
            if (s_Tuner == null || s_Tuner.onReceiveUploadLog == null) return;
            s_Tuner.m_UploadTelemetryRequest = UploadTelemetryRequest.Parse(bytes, size);
        }

        static void FidelityParamsCallbackImpl(CProtobufSerialization ps)
        {
            // Don't call OnReceiveFidelityParameters directly from this thread.
            if (s_Tuner == null || s_Tuner.onReceiveFidelityParameters == null) return;
            s_Tuner.m_ReceivedFidelityParameters = ps.ParseMessage<TFidelity>();
        }
    }

    static class Callbacks
    {
        internal static Action<CProtobufSerialization> onFidelityParamsReceived;
        internal static Action<IntPtr, uint> onUploadReceived;

        // These callbacks must be
        // * static as il2cpp can not marshall non-static delegates.
        // * non-generic
        [MonoPInvokeCallback(typeof(FidelityParamsCallback))]
        internal static void FidelityParamsCallbackImpl(ref CProtobufSerialization ps)
        {
            if (onFidelityParamsReceived != null) onFidelityParamsReceived(ps);
        }

        // These callbacks must be
        // * static as il2cpp can not marshall non-static delegates.
        // * non-generic
        [MonoPInvokeCallback(typeof(UploadCallback))]
        internal static void UploadCallbackImpl(IntPtr bytes, uint size)
        {
            if (onUploadReceived != null) onUploadReceived(bytes, size);
        }
    }
}