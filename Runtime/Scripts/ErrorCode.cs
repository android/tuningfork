//-----------------------------------------------------------------------
// <copyright file="ErrorCode.cs" company="Google">
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

namespace Google.Android.PerformanceTuner
{
    /// <summary>
    ///     All the error codes that can be returned by Tuning Fork functions.
    /// </summary>
    public enum ErrorCode
    {
        /// <summary>
        /// No error.
        /// </summary>
        Ok = 0,

        /// <summary>
        ///     No tuningfork_settings.bin found in assets/tuningfork.
        /// </summary>
        NoSettings = 1,

        /// <summary>
        ///     Not able to find the required Swappy functions.
        /// </summary>
        NoSwappy = 2,

        /// <summary>
        ///     `fpDefaultFileNum` is out of range.
        /// </summary>
        InvalidDefaultFidelityParams = 3,

        /// <summary>
        ///     No fidelity parameters found at initialization.
        /// </summary>
        NoFidelityParams = 4,

        /// <summary>
        ///     A call was made before Tuning Fork was initialized.
        /// </summary>
        TuningforkNotInitialized = 5,

        /// <summary>
        ///     Invalid parameter to `TuningFork_setCurrentAnnotation`.
        /// </summary>
        InvalidAnnotation = 6,

        /// <summary>
        ///     Invalid instrument key passed to a tick function.
        /// </summary>
        InvalidInstrumentKey = 7,

        /// <summary>
        ///     Invalid handle passed to `TuningFork_endTrace`.
        /// </summary>
        InvalidTraceHandle = 8,

        /// <summary>
        ///     Timeout in request for fidelity parameters.
        /// </summary>
        Timeout = 9,

        /// <summary>
        ///     Generic bad parameter.
        /// </summary>
        BadParameter = 10,

        /// <summary>
        ///     Could not encode a protobuf.
        /// </summary>
        B64EncodeFailed = 11,

        /// <summary>
        ///     Jni error - obsolete
        /// </summary>
        [System.Obsolete] JniBadVersion = 12,

        /// <summary>
        ///     Jni error - obsolete
        /// </summary>
        [System.Obsolete] JniBadThread = 13,

        /// <summary>
        ///     Jni error - obsolete
        /// </summary>
        [System.Obsolete] JniBadEnv = 14,

        /// <summary>
        ///     Jni error - an exception was thrown. See logcat output.
        /// </summary>
        JniException = 15,

        /// <summary>
        ///     Jni error - obsolete
        /// </summary>
        [System.Obsolete] JniBadJvm = 16,

        /// <summary>
        ///     Obsolete
        /// </summary>
        [System.Obsolete] NoClearcut = 17,

        /// <summary>
        ///     No dev_tuningfork_fidelityparams_#.bin found in assets/tuningfork.
        /// </summary>
        NoFidelityParamsInApk = 18,

        /// <summary>
        ///     Error calling `TuningFork_saveOrDeleteFidelityParamsFile`.
        /// </summary>
        CouldntSaveOrDeleteFps = 19,

        /// <summary>
        ///     Can't upload since another request is pending.
        /// </summary>
        PreviousUploadPending = 20,

        /// <summary>
        ///     Too frequent calls to `TuningFork_flush`.
        /// </summary>
        UploadTooFrequent = 21,

        /// <summary>
        ///     No such key when accessing file cache.
        /// </summary>
        NoSuchKey = 22,

        /// <summary>
        ///     General file error.
        /// </summary>
        BadFileOperation = 23,

        /// <summary>
        ///     Invalid tuningfork_settings.bin file.
        /// </summary>
        BadSettings = 24,

        /// <summary>
        ///     TuningFork_init was called more than once.
        /// </summary>
        AlreadyInitialized = 25,

        /// <summary>
        ///     Missing part of tuningfork_settings.bin.
        /// </summary>
        NoSettingsAnnotationEnumSizes = 26,

        /// <summary>
        ///     `TuningFork_startFidelityParamDownloadThread` was called more than once, or called when
        ///     TuningFork_init has already started download.
        /// </summary>
        DownloadThreadAlreadyStarted = 27,

        // PlatformNotSupported = 28 is obsolete.

        /// <summary>
        ///     An error occurred parsing the response to generateTuningParameters.
        /// </summary>
        GenerateTuningParametersError = 29,

        /// <summary>
        ///     The response from generateTuningParameters was not a success code.
        /// </summary>
        GenerateTuningParametersResponseNotSuccess = 30,

        // Unity only codes have reserved range 100-150

        /// <summary>
        ///     The game or app is run on a platform not supporting Tuning fork.
        /// </summary>
        PlatformNotSupported = 100,

        /// <summary>
        ///     Fidelity message is invalid.
        /// </summary>
        InvalidFidelity = 101,

        /// <summary>
        ///     Using incorrect API for default/custom modes.
        /// </summary>
        InvalidMode = 102,
    }
}