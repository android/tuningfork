//-----------------------------------------------------------------------
// <copyright file="LoadingSceneTracker.cs" company="Google">
//
// Copyright 2022 Google Inc. All Rights Reserved.
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

// Enabled in Initializer after having generated the classes this script depends on.
// Delete this symbol from Unity if the APT Plugin is removed from the project.
#if ANDROID_PERFORMANCE_TUNER_UTILITIES
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Google.Android.PerformanceTuner
{
    /// <summary>
    ///     Base utility script for tracking the duration of loading scenes. Extend this class and override the
    ///     functions to retrieve an AndroidPerformanceTuner, then attach to a GameObject in the scene to track
    ///     the time from Awake to OnDestroy. Don't attach to DontDestroyOnLoad objects. If there's no existing
    ///     AndroidPerformanceTuner, create one in the overridden and the script will track the time from
    ///     Start to OnDestroy.
    /// </summary>
    [Serializable]
    public abstract class LoadingSceneTracker: MonoBehaviour
    {
        [SerializeField]
        [Tooltip("If there's no existing tuner, one needs to be created and started. For compatibility with the " +
                 "Optimized Frame Pacing library Swappy, an AndroidPerformanceTuner can only be started at the end" +
                 "of the Start function. In this case, only the time from Start to OnDestroy will be tracked.")]
        private bool tunerAlreadyExists = false;

        [SerializeField]
        [Tooltip("If true, a new loading group will be started. If another loading group was already in " +
                 "progress, it will be stopped. If false, please start your loading group before this script starts" +
                 "recording and stop it after this script's OnDestroy.")]
        private bool startLoadingGroup = true;

        [Header("LoadingTimeMetadata parameters")]

        [SerializeField]
        [Tooltip("Loading state set to InterLevel by default as we want to track loading scenes.")]
        private LoadingTimeMetadata.LoadingState sceneLoadingState = LoadingTimeMetadata.LoadingState.InterLevel;

        [SerializeField]
        [Tooltip("Where data is being loaded from.")]
        private LoadingTimeMetadata.LoadingSource sceneLoadingSource = LoadingTimeMetadata.LoadingSource.UnknownSource;

        #if UNITY_2018_3_OR_NEWER
        [Min(0)]
        #endif
        [SerializeField]
        [Tooltip("0 = no compression, 100 = max compression.")]
        private int compressionLevel;

        [SerializeField]
        [Tooltip("Type of network connectivity, e.g., WiFi, cellular network.")]
        private LoadingTimeMetadata.NetworkConnectivity
            networkConnectivity = LoadingTimeMetadata.NetworkConnectivity.Unknown;

        [SerializeField]
        [Tooltip("Bandwidth in bits per second.")]
        private ulong networkTransferSpeedBps = 0;

        [SerializeField]
        [Tooltip("Latency in nanoseconds.")]
        private ulong networkLatencyNs = 0;

        [HideInInspector]
        public List<AnnotationFieldSaver> annotationFields;

        private ulong eventHandle;
        private ulong groupHandle;

        public AndroidPerformanceTuner<FidelityParams, Annotation> Tuner => tuner;
        private AndroidPerformanceTuner<FidelityParams, Annotation> tuner;

        /// <summary>
        ///     Implement to create a new AndroidPerformanceTuner. Only needed if tunerAlreadyExists is false.
        ///     The tuner must also be started before being returned.
        /// </summary>
        protected abstract AndroidPerformanceTuner<FidelityParams, Annotation> CreateAndStartPerformanceTuner();

        /// <summary>
        ///     Implement to retrieve an existing AndroidPerformanceTuner. Only needed if tunerAlreadyExists is true.
        /// </summary>
        protected abstract AndroidPerformanceTuner<FidelityParams, Annotation> GetPerformanceTuner();
        public void Awake()
        {
            if (tunerAlreadyExists)
            {
                tuner = GetPerformanceTuner();
                RecordLoadingTime();
            }
        }

        public IEnumerator Start()
        {
            if (!tunerAlreadyExists)
            {
                yield return new WaitForEndOfFrame(); // Needed to make sure Vulkan backend is fully ready, after the first frame.

                tuner = CreateAndStartPerformanceTuner();
                RecordLoadingTime();
            }
        }

        public void OnDestroy()
        {
            ErrorCode errorCode = tuner.StopRecordingLoadingTime(eventHandle);
            if (errorCode != ErrorCode.Ok)
            {
                Debug.LogErrorFormat("StopRecordingLoadingTime in GameObject {0} returned error: {1}", gameObject.name, errorCode);
            }

            if (startLoadingGroup)
            {
                errorCode = tuner.StopLoadingGroup(groupHandle);
                if (errorCode != ErrorCode.Ok)
                {
                    Debug.LogErrorFormat("StopLoadingGroup in GameObject {0} returned error: {1}", gameObject.name, errorCode);
                }
            }
        }

        private void RecordLoadingTime()
        {
            LoadingTimeMetadata sceneLoadMetadata = new LoadingTimeMetadata()
            {
                state = sceneLoadingState,
                source = sceneLoadingSource,
                compression_level = compressionLevel,
                network_connectivity = networkConnectivity,
                network_transfer_speed_bps = networkTransferSpeedBps,
                network_latency_ns = networkLatencyNs
            };

            Annotation annotation = BuildAnnotation();

            if (startLoadingGroup)
            {
                // The metadata and annotation are currently not used by the Play backend.
                var groupMetadata = new LoadingTimeMetadata()
                {
                    state = LoadingTimeMetadata.LoadingState.InterLevel
                };
                Result<ulong> groupResult = tuner.StartLoadingGroup(groupMetadata, annotation);
                groupHandle = groupResult.value;

                if (groupResult.errorCode != ErrorCode.Ok)
                {
                    Debug.LogErrorFormat("StartLoadingGroup in GameObject {0} returned error: {1}", gameObject.name, groupResult.errorCode);
                }
            }
            Result<ulong> eventResult = tuner.StartRecordingLoadingTime(sceneLoadMetadata, annotation);
            eventHandle = eventResult.value;

            if (eventResult.errorCode != ErrorCode.Ok)
            {
                Debug.LogErrorFormat("StartRecordingLoadingTime in GameObject {0} returned error: {1}", gameObject.name, eventResult.errorCode);

            }
        }

        // Creates the annotation to use during loading from the values saved in the inspector.
        private Annotation BuildAnnotation()
        {
            // When default annotations are enabled only create an annotation with the scene field.
            if (annotationFields == null || annotationFields.Count == 0)
            {
                // Annotation scene index starts from 1 instead of 0
                return new Annotation() {Scene = (Scene) (SceneManager.GetActiveScene().buildIndex + 1)};
            }

            Annotation annotation = new Annotation();
            Type annotationType = typeof(Annotation);
            for (int i = 0; i < annotationFields.Count; i++)
            {
                PropertyInfo propertyInfo = annotationType.GetProperty(annotationFields[i].fieldName);
                if (propertyInfo == null)
                {
                    Debug.LogErrorFormat("No {0} property defined in Annotation.", annotationFields[i].fieldName);
                    return null;
                }

                if (Enum.IsDefined(propertyInfo.PropertyType, annotationFields[i].value))
                {
                    propertyInfo.SetValue(annotation, annotationFields[i].value);
                }else
                {
                    // Set default value in case of error
                    propertyInfo.SetValue(annotation, 0);
                    Debug.LogErrorFormat("Value {0} is not defined in enum {1}, field {2}", 
                        annotationFields[i].value, annotationFields[i].enumName, annotationFields[i].fieldName);
                }
            }
            return annotation;
        }
    }

    [System.Serializable]
    public class AnnotationFieldSaver
    {
        public int value;
        public string enumName; // e.g., NewEnum1
        public string fieldName; // e.g., NewField1
    }
}
#endif