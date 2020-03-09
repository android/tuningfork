//-----------------------------------------------------------------------
// <copyright file="UploadTelemetryRequest.cs" company="Google">
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
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using System.Collections.Generic;

// ReSharper disable InconsistentNaming - names must match json names.
namespace Google.Android.PerformanceTuner
{
    [Serializable]
    public class UploadTelemetryRequest
    {
        public string name;
        public SessionContext session_context;
        public List<Telemetry> telemetry;


        [Serializable]
        public class SessionContext
        {
            public DeviceSpec device;
            public GameSdkInfo game_sdk_info;
            public TimePeriod time_period;
        }

        [Serializable]
        public class DeviceSpec
        {
            public string fingerprint;
            public int total_memory_bytes;
            public string build_version;
            public GlesVersion gles_version;
        }

        [Serializable]
        public class GlesVersion
        {
            public int major;
            public int minor;
        }

        [Serializable]
        public class GameSdkInfo
        {
            public string version;
            public string session_id;
        }

        [Serializable]
        public class TimePeriod
        {
            public Timestamp start_time;
            public Timestamp end_time;
        }

        [Serializable]
        public class Timestamp
        {
            public int seconds;
            public int nanos;
        }

        [Serializable]
        public class Telemetry
        {
            public TelemetryContext context;
            public TelemetryReport report;
        }

        [Serializable]
        public class TelemetryContext
        {
            public TuningParameters tuning_parameters;
        }

        [Serializable]
        public class TuningParameters
        {
            public string experiment_id;
        }

        [Serializable]
        public class TelemetryReport
        {
            public RenderingReport rendering;
            public LoadingReport loading;
        }

        [Serializable]
        public class RenderingReport
        {
            public List<RenderTimeHistogram> render_time_histogram;
        }

        [Serializable]
        public class RenderTimeHistogram
        {
            public int instrument_id;
            public List<int> counts;
        }

        [Serializable]
        public class LoadingReport
        {
            public LoadingTimeEvents loading_events;
        }

        [Serializable]
        public class LoadingTimeEvents
        {
            public List<int> time_ms;
        }

        public static UploadTelemetryRequest Parse(IntPtr bytes, uint size)
        {
            if (size <= 0)
            {
                Debug.LogWarningFormat("Cannot parse message, invalid size {0}", size);
                return null;
            }

            var received = new byte[size];
            Marshal.Copy(bytes, received, 0, (int) size);
            var log = Encoding.UTF8.GetString(received);
            try
            {
                var parsedLog = JsonUtility.FromJson<UploadTelemetryRequest>(log);
                return parsedLog;
            }
            catch (Exception e)
            {
                Debug.LogFormat("Cannot parse message, error {0}", e);
                return null;
            }
        }
    }
}