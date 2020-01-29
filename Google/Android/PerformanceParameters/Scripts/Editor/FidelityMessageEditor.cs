//-----------------------------------------------------------------------
// <copyright file="FidelityMessageEditor.cs" company="Google">
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

    internal class FidelityMessageEditor : MessageEditor
    {
        readonly string m_BasicInfo =
            "       Unity quality levels are used as Tuningfork fidelity levels.\n" +
            "       Unity quality settings are updated automatically when they're updated from the server.";

        protected override string BasicInfo
        {
            get { return m_BasicInfo; }
        }

        readonly string[] m_Headers = new string[3] {"Type", "Parameter name", ""};

        protected override string[] Headers
        {
            get { return m_Headers; }
        }

        internal FidelityMessageEditor(SetupConfig config, FileInfo protoFile) : base(
            config,
            MessageUtil.FidelityParamsDescriptor,
            ProtoMessageType.FidelityParams,
            DefaultMessages.FidelityMessage,
            protoFile)
        {
        }

        protected override FieldInfo RenderInfo(FieldInfo info, Rect rect, int index)
        {
            float blockWidth = rect.width / 3;

            Rect enumRect = new Rect(rect);
            enumRect.width = blockWidth;

            Rect enumSelectionRect = new Rect(rect);
            enumSelectionRect.x = enumRect.xMax;
            enumSelectionRect.width = blockWidth;

            Rect textRect = new Rect(rect);
            textRect.x = enumRect.xMax;
            textRect.width = 2 * blockWidth;

            info.fieldType = (FieldInfo.FieldType) EditorGUI.EnumPopup(enumRect, info.fieldType);

            if (info.fieldType == FieldInfo.FieldType.Enum)
            {
                Selection selection = RenderEnumSelection(enumSelectionRect, info.enumTypeIndex);
                info.enumTypeIndex = selection.id;
                info.enumType = selection.name;

                //Change textRect size if enum selection is rendered
                textRect.x = enumSelectionRect.xMax;
                textRect.width = blockWidth;
            }

            info.name = EditorGUI.TextField(textRect, info.name);
            return info;
        }
    }
}