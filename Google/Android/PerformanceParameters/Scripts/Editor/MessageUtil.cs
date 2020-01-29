//-----------------------------------------------------------------------
// <copyright file="MessageUtil.cs" company="Google">
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

using System.Collections.Generic;
using System.Linq;
using Google.Android.PerformanceParameters.Editor.Proto;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Google.Android.PerformanceParameters.Editor
{
    /// <summary>
    ///     Helper class to access Tuningfork proto messages - Annotation and Fidelity Parameters.
    /// </summary>
    public static class MessageUtil
    {
        public static readonly MessageDescriptor FidelityParamsDescriptor = FidelityParams.Descriptor;
        public static readonly MessageParser FidelityParamsParser = FidelityParams.Parser;
        public static readonly MessageDescriptor AnnotationDescriptor = Annotation.Descriptor;
        public static readonly MessageParser AnnotationParser = Annotation.Parser;
        public static readonly IList<EnumDescriptor> EnumTypes = DevTuningforkReflection.Descriptor.EnumTypes;
        public static readonly EnumInfoHelper EnumHelper = new EnumInfoHelper(DevTuningforkReflection.Descriptor);

        private static readonly FieldDescriptor LoadingField = AnnotationDescriptor.Fields
            .InDeclarationOrder()
            .FirstOrDefault(DefaultMessages.IsLoadingStateField);

        private static readonly FieldDescriptor SceneField = AnnotationDescriptor.Fields
            .InDeclarationOrder()
            .FirstOrDefault(DefaultMessages.IsSceneField);

        // As message field's indexes start from 1, return 0 if field is not found.
        public static readonly int LoadingAnnotationIndex = LoadingField?.FieldNumber ?? 0;

        public static readonly int SceneAnnotationIndex = SceneField?.FieldNumber ?? 0;

        public static readonly List<int> AnnotationEnumSizes =
            AnnotationDescriptor.Fields.InDeclarationOrder()
                .Select(x => x.EnumType.Values.Count).ToList();

        public static IMessage NewFidelityParams()
        {
            IMessage message = new FidelityParams();
            AdjustEnumValues(message);
            return message;
        }

        public static IMessage NewAnnotation()
        {
            IMessage message = new Annotation();
            AdjustEnumValues(message);
            return message;
        }

        /// <summary>
        ///     Adjust all enum values in the message to exclude 0-index value (Invalid)
        /// </summary>
        /// <param name="message">Proto message to adjust</param>
        public static void AdjustEnumValues(IMessage message)
        {
            foreach (var field in message.Descriptor.Fields.InFieldNumberOrder())
            {
                if (field.FieldType != FieldType.Enum) continue;
                if (field.EnumType.Values.Count <= 1) continue;
                field.Accessor.SetValue(message, 1);
            }
        }
    }
}