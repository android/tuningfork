//-----------------------------------------------------------------------
// <copyright file="EnumInfo.cs" company="Google">
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
using System.Collections.Generic;

namespace Google.Android.PerformanceTuner.Editor.Proto
{
    /// <summary>
    ///     Class to keep information about proto enum.
    ///     Used to describe and generate proto files.
    /// </summary>
    [Serializable]
    public struct EnumInfo
    {
        const string k_InvalidValue = "INVALID";
        public string name;
        public List<string> values;

        /// <summary>
        ///     Convert EnumInfo into a string formatted as it should appear in .proto file.
        /// </summary>
        public string ToProtoString()
        {
            AddInvalidValue();
            var str = "\nenum " + name + " {\n";
            for (var i = 0; i < values.Count; ++i)
                str += string.Format("    {0}_{1} = {2};\n", name.ToUpper(), values[i].ToUpper(), i);

            str += "}\n";
            return str;
        }

        /// <summary>
        ///     0-index element should always be *enum_name*_Invalid value.
        /// </summary>
        void AddInvalidValue()
        {
            if (values.Count == 0 || !values[0].Equals(k_InvalidValue, StringComparison.OrdinalIgnoreCase))
                values.Insert(0, k_InvalidValue);
        }
    }
}