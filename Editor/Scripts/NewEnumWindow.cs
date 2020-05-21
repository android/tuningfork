//-----------------------------------------------------------------------
// <copyright file="NewEnumWindow.cs" company="Google">
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

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using Google.Android.PerformanceTuner.Editor.Proto;

namespace Google.Android.PerformanceTuner.Editor
{
    public class NewEnumWindow : EditorWindow
    {
        public static void ShowNewEnumWindow(FileInfo protoFile, EnumInfoHelper enumInfoHelper)
        {
            var displayInfo = new EnumInfo()
            {
                name = "NewEnum" + enumInfoHelper.GetAllNames().Count(),
                values = new List<string>()
                {
                    "VALUE_0", "VALUE_1", "VALUE_2"
                }
            };
            ShowWindow(protoFile, enumInfoHelper, displayInfo);
        }

        public static void ShowWindow(FileInfo protoFile, EnumInfoHelper enumInfoHelper, EnumInfo displayInfo)
        {
            var window = ScriptableObject.CreateInstance<NewEnumWindow>();
            window.titleContent = new GUIContent("Add / Update enum");
            window.position = new Rect(Screen.width / 2f, Screen.height / 2f, 300, 500);
            window.m_ProtoFile = protoFile;
            window.m_EnumInfoHelper = enumInfoHelper;
            window.m_Info = displayInfo;
            window.ShowUtility();
            window.MakeChecks();
        }

        EnumInfo m_Info;
        ReorderableList m_EnumList;
        string m_ErrorMessage;
        bool m_EnumExist;
        FileInfo m_ProtoFile;
        EnumInfoHelper m_EnumInfoHelper;

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            m_Info.name = EditorGUILayout.TextField("Enum name", m_Info.name);
            if (m_EnumList == null) RebuildList();
            m_EnumList.DoLayoutList();
            if (EditorGUI.EndChangeCheck())
            {
                MakeChecks();
            }

            GUILayout.Space(5);
            if (!string.IsNullOrEmpty(m_ErrorMessage))
            {
                GUILayout.Space(5);
                EditorGUILayout.HelpBox(m_ErrorMessage, MessageType.Error);
            }
            else if (m_EnumExist)
            {
                EditorGUILayout.HelpBox("An enum with this name already exists", MessageType.Warning);
            }

            GUILayout.Space(5);
            using (new EditorGUI.DisabledScope(!string.IsNullOrEmpty(m_ErrorMessage)))
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button(
                    m_EnumExist ? "Update Enum" : "Add New Enum", Styles.button, GUILayout.ExpandWidth(false)))
                {
                    m_ProtoFile.AddEnum(m_Info);
                    this.Close();
                }
            }
        }

        void MakeChecks()
        {
            m_ErrorMessage = CheckForErrors(m_Info);
            m_EnumExist = EnumExist(m_Info);
        }

        bool EnumExist(EnumInfo info)
        {
            var existingEnumNames = m_EnumInfoHelper.GetAllNames();
            return existingEnumNames.Contains(info.name);
        }

        static string CheckForErrors(EnumInfo info)
        {
            string errors = string.Empty;

            if (string.IsNullOrEmpty(info.name)) errors += "\n► Name should not be empty";
            if (info.values.Count == 0) errors += "\n► Enum should has at least one value";
            if (info.values.Distinct().Count() != info.values.Count) errors += "\n► Values could not be duplicated";
            if (info.values.Any(x => string.IsNullOrEmpty(x))) errors += "\n► Fields can not have empty names";
            foreach (var reservedEnumName in EnumInfoHelper.reservedEnumNames)
            {
                if (info.name == reservedEnumName)
                    errors += string.Format("\n► {0} is reserved enum name", reservedEnumName);
            }

            if (!string.IsNullOrEmpty(errors)) errors = "Please fix all errors before adding new enum" + errors;
            return errors;
        }

        void RebuildList()
        {
            m_EnumList = new ReorderableList(m_Info.values, typeof(string), true, false, true, true);
            m_EnumList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var fieldName = EditorGUI.TextField(rect, m_Info.values[index]);
                m_Info.values[index] = Regex.Replace(fieldName, "[^a-zA-Z0-9_]", "").ToUpper();
            };
            m_EnumList.onAddCallback = (ReorderableList list) =>
            {
                m_Info.values.Add("VALUE_" + m_Info.values.Count.ToString());
            };
            m_EnumList.onRemoveCallback = (ReorderableList list) => { m_Info.values.RemoveAt(list.index); };
        }
    }
}