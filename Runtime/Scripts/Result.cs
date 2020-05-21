//-----------------------------------------------------------------------
// <copyright file="Result.cs" company="Google">
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
    /// Represents a value returned by an operation,
    /// with the associated error code.
    /// </summary>
    /// <typeparam name="T">The type of the value contained in the result.</typeparam>
    public class Result<T>
    {
        /// <summary>
        /// The error code returned by the operation that led to this result.
        /// </summary>
        public readonly ErrorCode errorCode;

        /// <summary>
        /// The value returned by the operation that led to this result.
        /// </summary>
        public readonly T value;

        /// <summary>
        /// Create a result with the given value and the associated error code.
        /// </summary>
        /// <param name="errorCode">The error code.</param>
        /// <param name="result">The value.</param>
        public Result(ErrorCode errorCode, T result)
        {
            this.errorCode = errorCode;
            this.value = result;
        }
    }
}
