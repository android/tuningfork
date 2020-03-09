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

using System;
using System.Reflection;
using Google.Protobuf;
using Google.Protobuf.Reflection;
using UnityEngine;

namespace Google.Android.PerformanceTuner
{
    /// <summary>
    ///     Helper class to set/get values for default fields via reflection.
    /// </summary>
    public static class MessageUtil
    {
        public enum LoadingState
        {
            NotLoading = 1,
            Loading = 2,
        }

        const string k_QualityLevelObjectField = "Level";

        const string k_SceneObjectField = "Scene";

        const string k_LoadingStateField = "LoadingState";

        /// <summary>
        ///     Check if message contains enum field with 0-value / invalid value.
        /// </summary>
        public static bool HasInvalidEnumField(IMessage message)
        {
            foreach (var field in message.Descriptor.Fields.InDeclarationOrder())
            {
                if (field.FieldType != FieldType.Enum) continue;
                int enumValue = (int) field.Accessor.GetValue(message);
                if (enumValue == 0) return true;
            }

            return false;
        }


        /// <summary>
        ///     Get QualityLevel field value via reflection
        /// </summary>
        /// <param name="message">Message contains quality level</param>
        /// <returns></returns>
        public static int GetQualityLevel(IMessage message)
        {
            var qualityLevelObj = message.GetType().InvokeMember(
                k_QualityLevelObjectField,
                BindingFlags.GetProperty,
                Type.DefaultBinder,
                message,
                null);
            return (int) qualityLevelObj;
        }


        public static bool HasQualityLevel<TFidelity>()
            where TFidelity : class, IMessage<TFidelity>, new()
        {
            var property = new TFidelity().GetType().GetProperty(k_QualityLevelObjectField);
            return property != null;
        }

        /// <summary>
        ///     Set value for QualityLevel field via reflection.
        /// </summary>
        /// <param name="message">The message to update</param>
        /// <param name="qualityLevel">The QualityLevel to set.</param>
        public static void SetQualityLevel(IMessage message, int qualityLevel)
        {
            message.GetType().InvokeMember(
                k_QualityLevelObjectField,
                BindingFlags.SetProperty,
                Type.DefaultBinder,
                message,
                new object[] {qualityLevel});
        }


        /// <summary>
        ///     Set loading state for LoadingState field via reflection.
        /// </summary>
        /// <param name="message">The message to update</param>
        /// <param name="loading">The loading state to set. false for Not Loading, true for Loading.</param>
        public static void SetLoadingState(IMessage message, LoadingState state)
        {
            var levelEnumType = message.GetType().GetProperty(k_LoadingStateField).PropertyType;

            message.GetType().InvokeMember(
                k_LoadingStateField,
                BindingFlags.SetProperty,
                Type.DefaultBinder,
                message,
                new object[1] {Enum.ToObject(levelEnumType, (int) state)});
        }

        public static bool HasLoadingState<TAnnotation>()
            where TAnnotation : class, IMessage<TAnnotation>, new()
        {
            var property = new TAnnotation().GetType().GetProperty(k_LoadingStateField);
            return property != null;
        }

        /// <summary>
        ///     Set value for Scene field via reflection.
        /// </summary>
        /// <param name="message">The message to update</param>
        /// <param name="sceneBuildIndex">The scene's build index to set.</param>
        public static void SetScene(IMessage message, int sceneBuildIndex)
        {
            // 0-index values in all enums are invalid.
            // Scene with index 0 has enum with index 1.
            // Increase sceneBuildIndex by one to match index in Scene enum.
            sceneBuildIndex++;
            var sceneEnumType = message.GetType().GetProperty(k_SceneObjectField).PropertyType;

            // To prevent incorrect enum value.
            var enumSize = Enum.GetValues(sceneEnumType).Length;
            sceneBuildIndex = Mathf.Clamp(sceneBuildIndex, 0, enumSize - 1);

            message.GetType().InvokeMember(
                k_SceneObjectField,
                BindingFlags.SetProperty,
                Type.DefaultBinder,
                message,
                new object[1] {Enum.ToObject(sceneEnumType, sceneBuildIndex)});
        }

        public static bool HasScene<TAnnotation>()
            where TAnnotation : class, IMessage<TAnnotation>, new()
        {
            var property = new TAnnotation().GetType().GetProperty(k_SceneObjectField);
            return property != null;
        }
    }
}