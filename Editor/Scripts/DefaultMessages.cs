//-----------------------------------------------------------------------
// <copyright file="DefaultMessages.cs" company="Google">
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
using Google.Android.PerformanceTuner.Editor.Proto;
using Google.Protobuf;
using Google.Protobuf.Reflection;

namespace Google.Android.PerformanceTuner.Editor
{
    public class DefaultMessages
    {
        /*
        * Default proto:
        * syntax = "proto3";
        * option csharp_namespace = "Google.Android.PerformanceTuner";
        * enum LoadingState {
        *     LOADINGSTATE_INVALID = 0;
        *     LOADINGSTATE_NOT_LOADING = 1;
        *     LOADINGSTATE_LOADING = 2;
        * }
        * enum Scene {
        *     SCENE_INVALID = 0;
        * }
        * message Annotation {
        *     LoadingState loading_state = 1;
        *     Scene scene = 2;
        * }
        * message FidelityParams {
        *     int32 level = 1;
        * }
        */

        public static readonly ByteString serializedDefaultProto = ByteString.FromBase64(string.Concat(
            "ChRkZXZfdHVuaW5nZm9yay5wcm90byJJCgpBbm5vdGF0aW9uEiQKDWxvYWRp",
            "bmdfc3RhdGUYASABKA4yDS5Mb2FkaW5nU3RhdGUSFQoFc2NlbmUYAiABKA4y",
            "Bi5TY2VuZSIfCg5GaWRlbGl0eVBhcmFtcxINCgVsZXZlbBgBIAEoBSpgCgxM",
            "b2FkaW5nU3RhdGUSGAoUTE9BRElOR1NUQVRFX0lOVkFMSUQQABIcChhMT0FE",
            "SU5HU1RBVEVfTk9UX0xPQURJTkcQARIYChRMT0FESU5HU1RBVEVfTE9BRElO",
            "RxACKksKBVNjZW5lEhEKDVNDRU5FX0lOVkFMSUQQABIvCitTQ0VORV9BU1NF",
            "VFNfUFJJVkFURV9SVU5USU1FX1NDRU5FU19ERUZBVUxUEAFCIqoCH0dvb2ds",
            "ZS5BbmRyb2lkLlBlcmZvcm1hbmNlVHVuZXJiBnByb3RvMw=="));

        public static EnumInfo loadingStateEnum = new EnumInfo
        {
            name = Names.loadingStateEnumName,
            values = new List<string> {"INVALID", "NOT_LOADING", "LOADING"}
        };

        static readonly FieldInfo k_LoadingStateField = new FieldInfo()
        {
            name = Names.loadingStateFieldName,
            fieldType = FieldInfo.FieldType.Enum,
            enumType = Names.loadingStateEnumName,
        };

        static readonly FieldInfo k_SceneField = new FieldInfo()
        {
            name = Names.sceneFieldName,
            fieldType = FieldInfo.FieldType.Enum,
            enumType = Names.sceneEnumName,
        };

        static readonly FieldInfo k_QualityLevelField = new FieldInfo()
        {
            name = "level",
            fieldType = FieldInfo.FieldType.Int32,
        };


        public static readonly MessageInfo annotationMessage = new MessageInfo
        {
            name = "Annotation",
            fields = new List<FieldInfo> {k_LoadingStateField, k_SceneField}
        };


        public static readonly MessageInfo fidelityMessage = new MessageInfo
        {
            name = "FidelityParams",
            fields = new List<FieldInfo> {k_QualityLevelField}
        };

        public static bool IsSceneField(FieldDescriptor field)
        {
            return field.Name == k_SceneField.name &&
                   field.FieldType == FieldType.Enum &&
                   field.EnumType.Name == k_SceneField.enumType;
        }

        public static bool IsLoadingStateField(FieldDescriptor field)
        {
            return field.Name == k_LoadingStateField.name &&
                   field.FieldType == FieldType.Enum &&
                   field.EnumType.Name == k_LoadingStateField.enumType;
        }

        public static bool IsSceneField(FieldInfo field)
        {
            return field.name == k_SceneField.name &&
                   field.fieldType == FieldInfo.FieldType.Enum &&
                   field.enumType == k_SceneField.enumType;
        }

        public static bool IsLoadingStateField(FieldInfo field)
        {
            return field.name == k_LoadingStateField.name &&
                   field.fieldType == FieldInfo.FieldType.Enum &&
                   field.enumType == k_LoadingStateField.enumType;
        }
    }
}