//-----------------------------------------------------------------------
// <copyright file="MessageBuilder.cs" company="Google">
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
using Google.Protobuf;

namespace Google.Android.PerformanceTuner.Editor
{
    public class MessageBuilder
    {
        public class Parameters
        {
            // e.g "Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null"
            public string assemblyFullName;

            // e.g. "Google.Android.PerformanceTuner.FidelityParams"
            public string fullName;
        }

        public class GenericParser<T> : Parser where T : class, IMessage<T>, new()
        {
            public override IMessage ParseFrom(ByteString data)
            {
                var parser = new MessageParser<T>(() => new T());
                return parser.ParseFrom(data);
            }
        }

        public abstract class Parser
        {
            public abstract IMessage ParseFrom(ByteString data);
        }

        public MessageBuilder(Parameters parameters)
        {
            m_Parameters = parameters;

            // Try to find in default assemble first.
            m_Type = FindAssemblyType(true);
            if (m_Type == null) m_Type = FindAssemblyType(false);
        }

        public bool valid
        {
            get { return m_Type != null; }
        }

        Parser m_Parser;

        public IMessage CreateNew()
        {
            if (m_Type == null)
                throw new TypeAccessException(String.Format("Message type {0} doesn't exist", m_Parameters.fullName));
            return (IMessage) Activator.CreateInstance(m_Type);
        }

        public IMessage ParseFrom(ByteString data)
        {
            if (m_Parser == null)
            {
                // Create a parser for the message type.
                Type genericParserType = typeof(GenericParser<>);
                Type parserType = genericParserType.MakeGenericType(m_Type);
                m_Parser = (Parser) Activator.CreateInstance(parserType);
            }

            return m_Parser.ParseFrom(data);
        }

        readonly Type m_Type;

        readonly Parameters m_Parameters;

        Type FindAssemblyType(bool searchInDefaultOnly)
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                //TODO(kseniia): Check if type can be found properly in different assemblies
                if (searchInDefaultOnly && assembly.FullName != m_Parameters.assemblyFullName) continue;
                if (assembly.IsDynamic) continue;
                foreach (var type in assembly.ExportedTypes)
                {
                    if (type.FullName == m_Parameters.fullName && typeof(IMessage).IsAssignableFrom(type))
                    {
                        return type;
                    }
                }
            }

            return null;
        }
    }
}