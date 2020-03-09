//-----------------------------------------------------------------------
// <copyright file="FileInfo.cs" company="Google">
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
using Google.Protobuf.Reflection;

namespace Google.Android.PerformanceTuner.Editor.Proto
{
    /// <summary>
    ///     Class to keep information about a proto file.
    ///     Includes information about enums and messages.
    ///     Used to describe and generate proto files.
    /// </summary>
    [Serializable]
    public class FileInfo
    {
        string m_Comment = string.Empty;

        const string k_DefaultOptions =
            "syntax = \"proto3\";\n\n" +
            "option csharp_namespace = \"{0}\";\n";

        string m_CSharpNamespace = "Google.Android.PerformanceTuner";

        readonly Dictionary<string, EnumInfo> m_Enums = new Dictionary<string, EnumInfo>();
        readonly Dictionary<string, MessageInfo> m_Messages = new Dictionary<string, MessageInfo>();

        /// <summary>
        ///     Create FileInfo from FileDescriptor.
        ///     It includes information for each message and enum in FileDescriptor.
        /// </summary>
        /// <param name="fileDescriptor">FileDescriptor to parse messages and enums from</param>
        public FileInfo(FileDescriptor fileDescriptor)
        {
            EnumInfoHelper enumHelper = new EnumInfoHelper(fileDescriptor);
            AddEnums(enumHelper.CreateInfoList());
            foreach (var messageType in fileDescriptor.MessageTypes)
            {
                AddMessage(new MessageInfo(messageType));
            }
        }

        public void SetNamespace(string cSharpNamespace)
        {
            m_CSharpNamespace = cSharpNamespace;
        }

        public void SetComment(string comment)
        {
            m_Comment = comment;
        }

        /// <summary>
        ///     Action called every time there are changes in messages or enums.
        /// </summary>
        public Action onUpdate;

        public void AddMessage(MessageInfo message)
        {
            m_Messages[message.name] = message;
            if (onUpdate != null)
                onUpdate();
        }

        void AddEnums(IEnumerable<EnumInfo> enums)
        {
            foreach (var enumInfo in enums)
                m_Enums[enumInfo.name] = enumInfo;
            if (onUpdate != null)
                onUpdate();
        }

        public void AddEnum(EnumInfo enumInfo)
        {
            m_Enums[enumInfo.name] = enumInfo;
            if (onUpdate != null)
                onUpdate();
        }

        public void DeleteEnum(string enumType)
        {
            if (m_Enums.ContainsKey(enumType))
                m_Enums.Remove(enumType);
            foreach (var message in m_Messages.Values)
                message.RemoveEnum(enumType);
            if (onUpdate != null)
                onUpdate();
        }


        /// <summary>
        ///     Convert FileInfo into string formatted as it should appear in proto file.
        /// </summary>
        public string ToProtoString()
        {
            string protoStr = String.Empty;
            if (m_Comment != string.Empty)
                protoStr += string.Format("// {0}\n", m_Comment);
            protoStr += string.Format(k_DefaultOptions, m_CSharpNamespace);
            foreach (var info in m_Enums.Values)
                protoStr += info.ToProtoString();
            foreach (var message in m_Messages.Values)
                protoStr += message.ToProtoString();
            return protoStr;
        }
    }
}