//-----------------------------------------------------------------------
// <copyright file="Submission.cs" company="Google">
//
// Copyright 2021 Google Inc. All Rights Reserved.
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
    ///     The set of submission types that the SetAggregationStrategyInterval method accepts.
    /// </summary>
    public enum Submission
    {
        /// <summary>
        /// Undefined submission method. Should not be set to this.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// Submission based on elapsed time.
        /// </summary>
        TimeBased = 1,

        /// <summary>
        /// Submission based on the elapsed number of ticks.
        /// </summary>
        TickBased = 2,
    }
}