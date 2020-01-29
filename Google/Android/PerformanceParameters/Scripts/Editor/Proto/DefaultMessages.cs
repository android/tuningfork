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
using Google.Protobuf.Reflection;

namespace Google.Android.PerformanceParameters.Editor.Proto
{
    public class DefaultMessages
    {
        private static readonly FieldInfo LoadingStateField = new FieldInfo()
        {
            name = Names.LoadingStateFieldName,
            fieldType = FieldInfo.FieldType.Enum,
            enumType = Names.LoadingStateEnumName,
        };

        private static readonly FieldInfo SceneField = new FieldInfo()
        {
            name = Names.SceneFieldName,
            fieldType = FieldInfo.FieldType.Enum,
            enumType = Names.SceneEnumName,
        };

        private static readonly FieldInfo QualityLevelField = new FieldInfo()
        {
            name = "quality_level",
            fieldType = FieldInfo.FieldType.Int32,
        };


        public static readonly MessageInfo AnnotationMessage = new MessageInfo
        {
            name = "Annotation",
            fields = new List<FieldInfo> {LoadingStateField, SceneField}
        };


        public static readonly MessageInfo FidelityMessage = new MessageInfo
        {
            name = "FidelityParams",
            fields = new List<FieldInfo> {QualityLevelField}
        };

        public static bool IsSceneField(FieldDescriptor field)
        {
            return field.Name == SceneField.name &&
                   field.FieldType == FieldType.Enum &&
                   field.EnumType.Name == SceneField.enumType;
        }

        public static bool IsLoadingStateField(FieldDescriptor field)
        {
            return field.Name == LoadingStateField.name &&
                   field.FieldType == FieldType.Enum &&
                   field.EnumType.Name == LoadingStateField.enumType;
        }
    }
}