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
    /// <summary>
    ///     Request message of 'UploadTelemetry'.
    /// </summary>
    [Serializable]
    public class UploadTelemetryRequest
    {
        /// <summary>
        ///     Resource name for the tuning parameters of an apk, identified by package name and version code.
        /// </summary>
        public string name;

        /// <summary>
        ///     Context that do not change as a game session progresses.
        /// </summary>
        public SessionContext session_context;

        /// <summary>
        ///     Series of telemetry to be logged.
        ///      Each member of this list is associated with a different telemetry context.
        /// </summary>
        public List<Telemetry> telemetry;


        /// <summary>
        ///     All context coming from the requesting device that is immutable during a gameplay session.
        /// </summary>
        [Serializable]
        public class SessionContext
        {
            /// <summary>
            ///    The list of crashes that happened during the last session. 
            /// </summary>
            public List<CrashReport> crash_reports;

            /// <summary>
            ///     The specs of the device doing the request.
            /// </summary>
            public DeviceSpec device;

            /// <summary>
            ///     Details specific to the Android Game SDK.
            /// </summary>
            public GameSdkInfo game_sdk_info;

            /// <summary>
            ///     The period of time of this collection.
            /// </summary>
            public TimePeriod time_period;
        }

        /// <summary>
        ///     A message representing performance characteristics of a requesting device.
        /// </summary>
        [Serializable]
        public class DeviceSpec
        {
            /// <summary>
            ///     Public build fingerprint of the device making this request.
            ///     See: https://developer.android.com/reference/android/os/Build.html#FINGERPRINT
            /// </summary>
            public string fingerprint;

            /// <summary>
            ///     The total accessible memory in bytes.
            ///     API >=16 devices can look at
            ///     http://developer.android.com/reference/android/app/ActivityManager.MemoryInfo.html#totalMem
            /// </summary>
            public int total_memory_bytes;

            /// <summary>
            ///     Value of build property "ro.build.version.codename". The current development codename,
            ///     or the string "REL" if this is a release build.
            ///     See: https://developer.android.com/reference/android/os/Build.VERSION
            /// </summary>
            public string build_version;

            /// <summary>
            ///     The GLES version on the device.
            /// </summary>
            public GlesVersion gles_version;

            /// <summary>
            ///     The maximum clock speed of all the CPUs cores on the device, in Hz.
            ///     Read from: /sys/devices/system/cpu/cpu0/cpufreq/cpuinfo_max_freq
            ///     Example: [ 1400, 3600 ]
            /// </summary>
            public List<int> cpu_core_freqs_hz;

            /// <summary>
            ///     Public model of the device making this request.
            ///     See: https://developer.android.com/reference/android/os/Build.html#MODEL
            /// </summary>
            public string model;

            /// <summary>
            ///     Public brand of the device making this request.
            ///     See: https://developer.android.com/reference/android/os/Build.html#BRAND
            /// </summary>
            public string brand;

            /// <summary>
            ///     Public product of the device making this request.
            ///     See: https://developer.android.com/reference/android/os/Build.html#PRODUCT
            /// </summary>
            public string product;

            /// <summary>
            ///     See: https://developer.android.com/reference/android/os/Build.html#DEVICE
            /// </summary>
            public string device;
        }

        /// <summary>
        ///     Message representing a version of OpenGL ES.
        ///     For further information on OpenGL ES and Android
        ///     See: https://developer.android.com/guide/topics/graphics/opengl
        /// </summary>
        [Serializable]
        public class GlesVersion
        {
            /// <summary>
            ///     The major GL ES version.
            ///     For example, for GL ES 3.1, this field would be 3.
            /// </summary>
            public int major;

            /// <summary>
            ///     The minor GL ES version.
            ///     For example, for GL ES 3.1, this field would be 1.
            /// </summary>
            public int minor;
        }

        /// <summary>
        ///     Log info specific to the Android Game SDK.
        ///     See more at:
        ///     https://android.googlesource.com/platform/frameworks/opt/gamesdk/
        /// </summary>
        [Serializable]
        public class GameSdkInfo
        {
            /// <summary>
            ///     The version of the Game SDK from include/tuningfork/tuningfork.h.
            ///     Major.Minor.Bugfix format, e.g. 1.2.0
            /// </summary>
            public string version;

            /// <summary>
            ///     A unique id generated by the Game SDK during a gameplay session.
            /// </summary>
            public string session_id;

            /// <summary>
            ///     The version of Swappy, if it was passed at runtime.
            ///     Major.Minor.Bugfix format, e.g. 1.2
            /// </summary>
            public string swappy_version;
        }

        [Serializable]
        public class TimePeriod
        {
            public string start_time;
            public string end_time;
        }

        [Serializable]
        public class Timestamp
        {
            public int seconds;
            public int nanos;
        }

        /// <summary>
        ///     Telemetry report, containing a context and the associated metrics.
        /// </summary>
        [Serializable]
        public class Telemetry
        {
            /// <summary>
            ///     The context of the device at the time of this telemetry collection.
            /// </summary>
            public TelemetryContext context;

            /// <summary>
            ///     The collection of telemetry data for this capture.
            /// </summary>
            public TelemetryReport report;
        }

        /// <summary>
        ///     Self reported context from a device requesting telemetry logging.
        /// </summary>
        [Serializable]
        public class TelemetryContext
        {
            /// <summary>
            ///     Serialized protocol buffer including metadata associated with the capture time.
            /// </summary>
            public string annotations;

            /// <summary>
            ///     The tuning parameters in use at the time of the capture.
            /// </summary>
            public TuningParameters tuning_parameters;

            /// <summary>
            ///     The total time spent on this context during the collection. Can represent disjoint period of time.
            ///     For example, if the context was active in between 10, 100] ms and [200, 220] ms,
            ///     the duration would be 110ms.
            /// </summary>
            public string duration;
        }


        /// <summary>
        ///     Message defining tunable parameters returned to the caller.
        /// </summary>
        [Serializable]
        public class TuningParameters
        {
            /// <summary>
            ///     The Play Console experiment id from which this set of parameters belongs to.
            /// </summary>
            public string experiment_id;

            /// <summary>
            ///     The serialized protocol buffer representing the parameters requested. 
            /// </summary>
            public string serialized_fidelity_parameters;
        }

        /// <summary>
        ///      Metrics collected from a gameplay/usage session in a particular context.
        /// </summary>
        [Serializable]
        public class TelemetryReport
        {
            /// <summary>
            ///     Render time data collected in the time period of the report.
            /// </summary>
            public RenderingReport rendering;

            /// <summary>
            ///     Information on scene loading times.
            /// </summary>
            public LoadingReport loading;

            /// <summary>
            ///      Information on memory usage.
            /// </summary>
            public MemoryReport memory;

            /// <summary>
            ///     Loading events that may have been abandoned.
            /// </summary>
            public PartialLoadingReport partial_loading;

            /// <summary>
            ///     Information on battery usage.
            /// </summary>
            public BatteryReport battery;
        }

        /// <summary>
        ///     A report of the rendering for a period of gameplay/usage.
        /// </summary>
        [Serializable]
        public class RenderingReport
        {
            /// <summary>
            ///     Distribution of render times, distributed into buckets.
            /// </summary>
            public List<RenderTimeHistogram> render_time_histogram;
        }

        /// <summary>
        ///     Representation of a histogram with pre-defined buckets, representing render time.
        /// </summary>
        [Serializable]
        public class RenderTimeHistogram
        {
            /// <summary>
            ///     ID of the frame capture point of the frame, if this is coming from the Android Game SDK.
            ///     The capture point refers to the point in the rendering pipeline the frame is recorded.
            /// </summary>
            public int instrument_id;

            /// <summary>
            ///     Bucket counts.
            ///     Assumes the buckets are defined elsewhere per apk;
            ///     only the counts are logged. Buckets correspond to render time (ms).
            /// </summary>
            public List<int> counts;
        }

        /// <summary>
        ///     A report of loading times, a set of events.
        ///     App start-up times for different app state changes can be recorded as well as inter-level times.
        ///     For inter-level times, Tuning Fork records the time between the 'loading' annotation changing from
        ///     LOADING to NOT_LOADING and this is then stored as an event associated with the annotation.
        /// </summary>
        [Serializable]
        public class LoadingReport
        {
            /// <summary>
            ///     Actual loading data
            /// </summary>
            public List<LoadingTimeEvents> loading_events;
        }

        /// <summary>
        ///     A report of loading times for active loading events when an app lifecycle
        ///     event occurred. Currently it is only app start and stop events that trigger
        ///     these reports.
        /// </summary>
        [Serializable]
        public class PartialLoadingReport
        {
            /// <summary>
            ///     Lifecycle event that caused a partial loading report.
            /// </summary>
            public enum LifecycleEventType
            {
                /// <summary>
                ///     Unspecified.
                /// </summary>
                LIFECYCLE_EVENT_TYPE_UNSPECIFIED = 0,

                /// <summary>
                ///     App start event received (i.e. app was foregrounded).
                /// </summary>
                START = 1,

                /// <summary>
                ///     App stop event received (i.e. app was backgrounded).
                /// </summary>
                STOP = 2
            }

            // Lifecycle event that caused this partial loading report.
            public LifecycleEventType event_type;

            // A loading report with the loading events that were active when the
            // lifecycle event happened.
            public LoadingReport report;
        }

        /// <summary>
        ///     Representation of a set of loading times.
        ///     Where developers use start+stop functions to record loading time events (the enouraged behaviour),
        ///     they are recorded in 'intervals'. 'times_ms' will contain events that are recorded as durations.
        /// </summary>
        [Serializable]
        public class LoadingTimeEvents
        {
            /// <summary>
            ///     Times in milliseconds
            /// </summary>
            public List<int> times_ms;

            /// <summary>
            ///     Metadata recorded with these loading events.
            /// </summary>
            public LoadingTimeMetadata loading_metadata;

            /// <summary>
            ///     Events recorded with both start and end times. Times are durations from the app process' start time.
            /// </summary>
            public List<ProcessTimePeriod> intervals;
        }

        /// <summary>
        ///     Metadata recorded with loading times.
        /// </summary>
        [Serializable]
        public class LoadingTimeMetadata
        {
            public string state;

            /// <summary>
            ///     The source of the event, e.g. memory, network, etc.
            /// </summary>
            public string source;

            /// <summary>
            ///     Compression level: 0 = no compression, 100 = maximum.
            /// </summary>
            public string compression_level;

            /// <summary>
            ///     Network information associated with download events.
            /// </summary>
            public NetworkInfo network_info;

            /// <summary>
            ///     Loading event group ID. Uniquely generated by the APT client library when
            ///     the game calls TuningFork_startLoadingGroup.
            /// </summary>
            public string group_id;
        }

        /// <summary>
        ///      Information about network conditions.
        /// </summary>
        [Serializable]
        public class NetworkInfo
        {
            /// <summary>
            ///     Wifi, mobile, etc.
            /// </summary>
            public enum NetworkConnectivity
            {
                /// <summary>
                ///     Unspecified.
                /// </summary>
                NETWORK_CONNECTIVITY_UNSPECIFIED = 0,

                /// <summary>
                ///     Wifi.
                /// </summary>
                WIFI = 1,

                /// <summary>
                ///     Cellular/ mobile network.
                /// </summary>
                CELLULAR_NETWORK = 2,
            }

            /// <summary>
            ///     Wifi, mobile, etc.
            /// </summary>
            public string connectivity;

            /// <summary>
            ///     Bandwidth in bits per second.
            /// </summary>
            public string bandwidth_bps;

            /// <summary>
            ///     Latency in nanoseconds.
            /// </summary>
            public string latency;
        }

        [Serializable]
        public class ProcessTimePeriod
        {
            public string start;
            public string end;
        }

        /// <summary>
        ///     A report of the memory usage of a device while a game is playing.
        /// </summary>
        [Serializable]
        public class MemoryReport
        {
            /// <summary>
            ///     Distribution of memory usage
            /// </summary>
            public List<MemoryHistogram> memory_histogram;
        }

        /// <summary>
        ///     A histogram describing memory usage.
        /// </summary>
        [Serializable]
        public class MemoryHistogram
        {
            /// <summary>
            ///     Enum describing how the memory records were obtained.
            /// </summary>
            public enum MemoryRecordType
            {
                /// <summary>
                /// 
                /// </summary>
                INVALID = 0,

                /// <summary>
                ///     From calls to android.os.Debug.getNativeHeapAllocatedSize.
                /// </summary>
                ANDROID_DEBUG_NATIVE_HEAP = 1,

                /// <summary>
                ///     From /proc/<PID>/oom_score file.
                /// </summary>
                ANDROID_OOM_SCORE = 2,

                /// <summary>
                ///     From /proc/meminfo and /proc/<PID>/status files.
                /// </summary>
                ANDROID_MEMINFO_ACTIVE = 3,

                /// <summary>
                ///     From /proc/meminfo and /proc/<PID>/status files.
                /// </summary>
                ANDROID_MEMINFO_ACTIVEANON = 4,

                /// <summary>
                ///     From /proc/meminfo and /proc/<PID>/status files.
                /// </summary>
                ANDROID_MEMINFO_ACTIVEFILE = 5,

                /// <summary>
                ///     From /proc/meminfo and /proc/<PID>/status files.
                /// </summary>
                ANDROID_MEMINFO_ANONPAGES = 6,

                /// <summary>
                ///     From /proc/meminfo and /proc/<PID>/status files.
                /// </summary>
                ANDROID_MEMINFO_COMMITLIMIT = 7,

                /// <summary>
                ///     From /proc/meminfo and /proc/<PID>/status files.
                /// </summary>
                ANDROID_MEMINFO_HIGHTOTAL = 8,

                /// <summary>
                ///     From /proc/meminfo and /proc/<PID>/status files.
                /// </summary>
                ANDROID_MEMINFO_LOWTOTAL = 9,

                /// <summary>
                ///     From /proc/meminfo and /proc/<PID>/status files.
                /// </summary>
                ANDROID_MEMINFO_MEMAVAILABLE = 10,

                /// <summary>
                ///     From /proc/meminfo and /proc/<PID>/status files.
                /// </summary>
                ANDROID_MEMINFO_MEMFREE = 11,

                /// <summary>
                ///     From /proc/meminfo and /proc/<PID>/status files.
                /// </summary>
                ANDROID_MEMINFO_MEMTOTAL = 12,

                /// <summary>
                ///     From /proc/meminfo and /proc/<PID>/status files.
                /// </summary>
                ANDROID_MEMINFO_VMDATA = 13,

                /// <summary>
                ///     From /proc/meminfo and /proc/<PID>/status files.
                /// </summary>
                ANDROID_MEMINFO_VMRSS = 14,

                /// <summary>
                ///     From /proc/meminfo and /proc/<PID>/status files.
                /// </summary>
                ANDROID_MEMINFO_VMSIZE = 15
            }

            /// <summary>
            ///     The type of memory record.
            /// </summary>
            public MemoryRecordType type;

            /// <summary>
            ///     How often a memory record was taken in milliseconds.
            /// </summary>
            public int period_ms;

            public MemoryHistogramConfig histogram_config;


            /// <summary>
            ///     Bucket counts.
            ///     Note that the first bucket is for memory records < bucket_min_kB and
            ///     the last bucket is for memory records > bucket_min_kB so there must be at least 2 buckets.
            /// </summary>
            public List<int> counts;
        }

        [Serializable]
        public class MemoryHistogramConfig
        {
            /// <summary>
            ///     The range of the buckets in bytes.
            /// </summary>
            public long bucket_min_bytes;

            /// <summary>
            ///     The range of the buckets in bytes.
            /// </summary>
            public long bucket_max_bytes;
        }

        /// <summary>
        ///     A report containing a single crash report.
        /// </summary>
        [Serializable]
        public class CrashReport
        {
            /// <summary>
            ///     Enum listing possible crash reasons for the app.
            /// </summary>
            public enum CrashReason
            {
                // Unspecified.
                CRASH_REASON_UNSPECIFIED = 0,

                LOW_MEMORY = 1
            }

            /// <summary>
            ///     The reason for why the crash occurred.
            /// </summary>
            public int crash_reason;

            /// <summary>
            ///     The session during which the crash occurred (e.g. previous session).
            /// </summary>
            public string session_id;
        }

        /// <summary>
        ///     A report of battery usage.
        /// </summary>
        [Serializable]
        public class BatteryReport
        {
            /// <summary>
            ///     A series of collected battery events.
            /// </summary>
            public List<BatteryEvent> battery_event;
        }

        /// <summary>
        ///     A single battery event describing battery information.
        /// </summary>
        [Serializable]
        public class BatteryEvent
        {
            /// <summary>
            ///     Duration from process start to the time the event is collected.
            /// </summary>
            public string event_time;

            /// <summary>
            ///     The remaining battery percentage (out of 100) at the time of collection.
            /// </summary>
            public int percentage;

            /// <summary>
            ///     The remaining battery charge (in micro-amp hour) at the time of collection.
            /// </summary>
            public int current_charge_microampere_hours;

            /// <summary>
            ///     Whether the battery is currently charging.
            /// </summary>
            public bool charging;

            /// <summary>
            ///     Whether the app is on foreground at the time of collection.
            /// </summary>
            public bool app_on_foreground;
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
                return JsonUtility.FromJson<UploadTelemetryRequest>(log);
            }
            catch (Exception e)
            {
                Debug.LogFormat("Cannot parse message, error {0}", e);
                return null;
            }
        }
    }
}