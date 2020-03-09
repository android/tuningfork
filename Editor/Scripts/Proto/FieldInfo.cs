//-----------------------------------------------------------------------
// <copyright file="FieldInfo.cs" company="Google">
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
using Google.Protobuf.Reflection;

namespace Google.Android.PerformanceTuner.Editor.Proto
{
    /// <summary>
    ///     Class to keep information about proto field.
    ///     Used to describe and generate proto files.
    /// </summary>
    [Serializable]
    public struct FieldInfo
    {
        const string k_DefaultFieldName = "new_field_";

        /// <summary>
        ///     Construct a FieldInfo with the default values for a field.
        ///     FidelityParameters message's field has default Int32 type.
        ///     Annotation message's field has default Enum type.
        /// </summary>
        public FieldInfo(FileDescriptor fileDescriptor, ProtoMessageType type, int index)
        {
            name = k_DefaultFieldName + index;
            enumTypeIndex = 0;

            // Only enums are allowed for annotations.
            if (type == ProtoMessageType.Annotation)
            {
                fieldType = FieldType.Enum;
                enumType = fileDescriptor.EnumTypes[enumTypeIndex].Name;
            }
            else
            {
                fieldType = FieldType.Int32;
                enumType = null;
            }
        }

        public enum FieldType
        {
            Int32 = 0,
            Float = 1,
            Enum = 2
        }

        public string name;
        public FieldType fieldType;
        public string enumType;
        public int enumTypeIndex;
    }
}