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

using System.Text.RegularExpressions;
using Google.Protobuf.Reflection;
using UnityEditor;
using UnityEngine;
using Google.Android.PerformanceTuner.Editor.Proto;

namespace Google.Android.PerformanceTuner.Editor
{
    public class FidelityMessageEditor : MessageEditor
    {
        const string k_BasicInfo =
            "       Unity quality levels are used as Android Performance Tuner quality levels.\n" +
            "       Unity quality settings are updated automatically when they're updated from the server.";

        protected override string basicInfo
        {
            get { return k_BasicInfo; }
        }

        readonly string[] m_Headers = new string[3] {"Type", "Parameter name", ""};

        protected override string[] headers
        {
            get { return m_Headers; }
        }

        public FidelityMessageEditor(ProjectData projectData, SetupConfig config, FileInfo protoFile,
            MessageDescriptor descriptor,
            EnumInfoHelper enumInfoHelper) : base(
            projectData,
            config,
            descriptor,
            ProtoMessageType.FidelityParams,
            DefaultMessages.fidelityMessage,
            protoFile,
            enumInfoHelper)
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

            // String should contain only a-z, A-Z, numbers and '_' symbols.
            string input = EditorGUI.TextField(textRect, info.name);
            info.name = Regex.Replace(input, "[^a-zA-Z0-9_]", "");

            return info;
        }

        protected override void ApplyState(EditorState state)
        {
            if (!state.useAdvanced)
            {
                m_ProjectData.DeleteAllFidelityMessages();
            }
        }
    }
}