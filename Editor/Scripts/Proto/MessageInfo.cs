//-----------------------------------------------------------------------
// <copyright file="MessageInfo.cs" company="Google">
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
using System.Linq;
using Google.Protobuf.Reflection;

namespace Google.Android.PerformanceTuner.Editor.Proto
{
    /// <summary>
    ///     Class to keep information about a proto message.
    ///     Used to describe and generate proto files.
    /// </summary>
    [Serializable]
    public class MessageInfo
    {
        const int k_FirstFieldIndex = 1;
        public List<FieldInfo> fields = new List<FieldInfo>();
        public string name;

        public MessageInfo()
        {
        }

        public MessageInfo(MessageInfo other)
        {
            name = other.name;
            fields = new List<FieldInfo>(other.fields);
        }

        /// <summary>
        ///     Create MessageInfo from MessageDescriptor.
        ///     Each field in descriptor is mapped to field info.
        ///     Only three types of fields are supported - int32, float and enum.
        /// </summary>
        /// <param name="messageDescriptor">MessageDescriptor to parse fields from</param>
        /// <exception cref="Exception">throw Exception if field type is not suuported</exception>
        public MessageInfo(MessageDescriptor messageDescriptor)
        {
            name = messageDescriptor.Name;
            fields = new List<FieldInfo>();
            foreach (var field in messageDescriptor.Fields.InDeclarationOrder())
            {
                var info = new FieldInfo {name = field.Name};
                switch (field.FieldType)
                {
                    case FieldType.Enum:
                        info.enumType = field.EnumType.Name;
                        info.fieldType = FieldInfo.FieldType.Enum;
                        info.enumTypeIndex = field.EnumType.Index;
                        break;
                    case FieldType.Int32:
                        info.enumType = string.Empty;
                        info.fieldType = FieldInfo.FieldType.Int32;
                        break;
                    case FieldType.Float:
                        info.enumType = string.Empty;
                        info.fieldType = FieldInfo.FieldType.Float;
                        break;
                    default:
                        throw new Exception("Field type is not supported");
                }

                fields.Add(info);
            }
        }

        public void RemoveEnum(string enumType)
        {
            fields = fields.Where(field => field.enumType != enumType).ToList();
        }

        /// <summary>
        ///     Convert MessageInfo into string formatted as it should appear in .proto file.
        /// </summary>
        public string ToProtoString()
        {
            var str = "\nmessage " + name + " {\n";
            var index = k_FirstFieldIndex;
            foreach (var info in fields)
            {
                string typeName;
                if (info.fieldType == FieldInfo.FieldType.Enum) typeName = info.enumType;
                else typeName = info.fieldType.ToString().ToLower();
                str += string.Format("    {0} {1} = {2};\n", typeName, info.name, index);
                index++;
            }

            str += "}\n";
            return str;
        }
    }
}