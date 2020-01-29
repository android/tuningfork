//-----------------------------------------------------------------------
// <copyright file="AnnotationMessageEditor.cs" company="Google">
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

namespace Google.Android.PerformanceParameters.Editor
{
    using UnityEditor;
    using UnityEngine;
    using Google.Android.PerformanceParameters.Editor.Proto;

    public class AnnotationMessageEditor : MessageEditor
    {
        readonly string m_BasicInfo =
            "       The scene is used as an annotation.\n" +
            "       The annotation is set automatically when active scene is changed.";

        protected override string BasicInfo
        {
            get { return m_BasicInfo; }
        }

        readonly string[] m_Headers = new string[2] {"Type", "Parameter name "};

        protected override string[] Headers
        {
            get { return m_Headers; }
        }

        internal AnnotationMessageEditor(SetupConfig config, FileInfo protoFile) : base(
            config,
            MessageUtil.AnnotationDescriptor,
            ProtoMessageType.Annotation,
            DefaultMessages.AnnotationMessage,
            protoFile)
        {
        }

        protected override FieldInfo RenderInfo(FieldInfo info, Rect rect, int index)
        {
            Rect enumRect = new Rect(rect);
            enumRect.width = rect.width / 3;

            Rect textRect = new Rect(rect);
            textRect.x = enumRect.xMax;
            textRect.width = rect.width - enumRect.width;

            Selection selection = RenderEnumSelection(enumRect, info.enumTypeIndex);
            info.enumTypeIndex = selection.id;
            info.enumType = selection.name;
            info.name = EditorGUI.TextField(textRect, info.name);
            return info;
        }
    }
}