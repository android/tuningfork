//-----------------------------------------------------------------------
// <copyright file="DevDescriptor.cs" company="Google">
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
using Google.Protobuf;
using Google.Protobuf.Reflection;
using UnityEngine;

namespace Google.Android.PerformanceTuner.Editor
{
    /// <summary>
    ///    Wrapper around DevTuningfork.cs.
    ///    Loads message descriptors and parsers from file descriptor.
    ///    File descriptor must contain - `Annotation` and `FidelityParams` messages.
    /// </summary>
    public class DevDescriptor
    {
        /// <summary>
        ///    Create new wrapper for DevTuningfork proto.
        /// </summary>
        /// <param name="serializedData">serialized data for file descriptor</param>
        public DevDescriptor(ByteString serializedData)
        {
            m_SerializedData = serializedData;
        }

        /// <summary>
        ///     Create new wrapper for DevTuningfork proto.
        /// </summary>
        /// <param name="bytes">bytes as read from .descriptor file</param>
        public DevDescriptor(byte[] bytes)
        {
            m_SerializedBytes = bytes;
        }

        public bool Build()
        {
            try
            {
                IReadOnlyList<FileDescriptor> files;
                if (m_SerializedData != null)
                {
                    files = FileDescriptor.BuildFromByteStrings(new[] {m_SerializedData});
                }
                else if (m_SerializedBytes != null)
                {
                    var fileDescriptorSet = FileDescriptorSet.Parser.ParseFrom(m_SerializedBytes);
                    files = FileDescriptor.BuildFromByteStrings(fileDescriptorSet.File);
                }
                else
                {
                    throw new InvalidOperationException("DevDescriptor is not properly initialized.");
                }

                fileDescriptor = files[0];
                fidelityMessage = fileDescriptor.MessageTypes.First(x => x.Name == "FidelityParams");
                annotationMessage = fileDescriptor.MessageTypes.First(x => x.Name == "Annotation");
                var loadingField = annotationMessage.Fields
                    .InDeclarationOrder()
                    .FirstOrDefault(DefaultMessages.IsLoadingStateField);
                var sceneField = annotationMessage.Fields
                    .InDeclarationOrder()
                    .FirstOrDefault(DefaultMessages.IsSceneField);
                // As message field's indexes start from 1, return 0 if field is not found.
                loadingAnnotationIndex = loadingField?.FieldNumber ?? 0;
                sceneAnnotationIndex = sceneField?.FieldNumber ?? 0;
                annotationEnumSizes =
                    annotationMessage.Fields.InDeclarationOrder()
                        .Select(x => x.EnumType.Values.Count).ToList();
                fidelityFieldNames = fidelityMessage.Fields.InDeclarationOrder().Select(x => x.Name).ToList();
                return true;
            }
            catch (Exception e)
            {
                Debug.Log("Could not build DevDescriptor, " + e);
                return false;
            }
        }


        // ByteString or byte[] must be set.
        readonly ByteString m_SerializedData;
        readonly byte[] m_SerializedBytes;
        public FileDescriptor fileDescriptor { get; private set; }
        public MessageDescriptor fidelityMessage { get; private set; }
        public MessageDescriptor annotationMessage { get; private set; }
        public int loadingAnnotationIndex { get; private set; }
        public int sceneAnnotationIndex { get; private set; }
        public IList<string> fidelityFieldNames { get; private set; }
        public List<int> annotationEnumSizes { get; private set; }
    }
}