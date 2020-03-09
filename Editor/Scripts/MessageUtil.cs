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

using Google.Protobuf;
using Google.Protobuf.Reflection;
using UnityEngine;
using System.Collections.Generic;

namespace Google.Android.PerformanceTuner.Editor
{
    /// <summary>
    ///     Helper class to access Android Performance Tuner proto messages - Annotation and Fidelity Parameters.
    /// </summary>
    public static class MessageUtil
    {
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

        /// <summary>
        ///     Create set of fidelity parameters with quality levels
        /// </summary>
        public static List<IMessage> FidelityMessagesWithQualityLevels()
        {
            var messages = new List<IMessage>();
            for (int i = 0; i < QualitySettings.names.Length; ++i)
            {
                var message = FidelityBuilder.builder.CreateNew();
                AdjustEnumValues(message);
                PerformanceTuner.MessageUtil.SetQualityLevel(message, i);
                messages.Add(message);
            }

            return messages;
        }
    }
}