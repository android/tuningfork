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
        * enum Scene {
        *     SCENE_INVALID = 0;
        * }
        * message Annotation {
        *     Scene scene = 1;
        * }
        * message FidelityParams {
        *     int32 level = 1;
        * }
        */

        public static readonly ByteString serializedDefaultProto = ByteString.FromBase64(string.Concat(
            "ChRkZXZfdHVuaW5nZm9yay5wcm90byIjCgpBbm5vdGF0aW9uEhUKBXNjZW5l",
            "GAEgASgOMgYuU2NlbmUiHwoORmlkZWxpdHlQYXJhbXMSDQoFbGV2ZWwYASAB",
            "KAUqGgoFU2NlbmUSEQoNU0NFTkVfSU5WQUxJRBAAQiKqAh9Hb29nbGUuQW5k",
            "cm9pZC5QZXJmb3JtYW5jZVR1bmVyYgZwcm90bzM="));


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
            fields = new List<FieldInfo> {k_SceneField}
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
            return field.Name == Names.loadingStateFieldName &&
                   field.FieldType == FieldType.Enum &&
                   field.EnumType.Name == Names.loadingStateEnumName;
        }

        public static bool IsSceneField(FieldInfo field)
        {
            return field.name == k_SceneField.name &&
                   field.fieldType == FieldInfo.FieldType.Enum &&
                   field.enumType == k_SceneField.enumType;
        }

        public static bool IsLoadingStateField(FieldInfo field)
        {
            return field.name == Names.loadingStateFieldName &&
                   field.fieldType == FieldInfo.FieldType.Enum &&
                   field.enumType == Names.loadingStateEnumName;
        }
    }
}