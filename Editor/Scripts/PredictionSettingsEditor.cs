//-----------------------------------------------------------------------
// <copyright file="FidelityParametersEditor.cs" company="Google">
//
// Copyright 2023 Google Inc. All Rights Reserved.
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
using UnityEditor;
using UnityEngine;

namespace Google.Android.PerformanceTuner.Editor
{
    public class PredictionSettingsEditor
    {
        readonly ProjectData m_ProjectData;
        readonly EditorState m_EditorState;
        readonly EditorStatePrefs<EditorState> m_EditorStatePrefs;
        private readonly SetupConfig m_SetupConfig;

        const string k_DefaultInfo =
            " Quality level prediction API may be used.\n" +
            " Make sure that Application.TargetFrameRate is set to the required value.";

        const string k_AdvancedInfoHeader =
            "When using Advanced fidelity paramaters, you can call the predictability API to fetch expected framerates for each quality level.\n";
        const string k_AdvancedAPIInfo = "public Result<List<QualityLevelPredictions<TFidelity>>> GetQualityLevelPredictions(UInt32 timeoutMs) \n";
        const string k_AdvancedInfoAPIUsage =
            "The call can be made on your performance tuner instance as the following :\n";
        const string k_AdvancedIntegration = "var output = tuner.GetQualityLevelPredictions(timeoutMS);\n";
        const string k_AdvancedInfoAPIreturn =
            "Then the output.errorCode holds the error code from the API call, it will be ErrorCode.Ok on success.\n" +
            "output.value is a list of a tuple {fidelity, predictedTimeUs} where fidelity is the fidelity message for a quality level and predictedTimeUs is the predicted time in micro seconds for that level. \n" +
            "You can iterate through all the levels and pick one that has the predicted frame time close to your expected frame time. \n \n" +
            "The levels can be iterated in the following way :\n";
        const string k_AdvancedInfoIteration =
            "for (int i = 0; i < output.value.Count; i++)\n" +
            "{ \n" +
            "\t //output.value[i].predictedTimeUs is the frame time for level i \n" +
            "\t //output.value[i].fidelity is the fidelity parameter message \n" +
            "}";

        public PredictionSettingsEditor(ProjectData projectData, SetupConfig setupConfig)
        {
            m_ProjectData = projectData;
            m_SetupConfig = setupConfig;
            m_EditorStatePrefs = new EditorStatePrefs<EditorState>("prediction-settings", new EditorState()
            {
                enableDefault = m_SetupConfig.defaultPredictionEnabled
            });
            m_EditorState = m_EditorStatePrefs.Get();
        }

        public void OnGUI()
        {
            // All elements in the tuningfork plugin have an offset of 15 from the top.
            GUILayout.Space(15);

            if (!m_SetupConfig.useAdvancedFidelityParameters)
            {
                ShowDefaultSettings();
                EditorGUI.indentLevel++;
                using (var group = new EditorGUI.ChangeCheckScope())
                {
                    m_EditorState.enableDefault = GUILayout.Toggle(m_EditorState.enableDefault, "  Enable Prediction API(alpha)");

                    if (group.changed)
                    {
                        setDefaultSettings(m_EditorState.enableDefault);
                    }
                }
                EditorGUI.indentLevel--;
            }
            else
            {
                // Reset previously set default enable option
                setDefaultSettings(false);
                ShowAdvancedIntegration();
            }
        }

        void ShowDefaultSettings()
        {
            EditorGUILayout.LabelField(k_DefaultInfo, EditorStyles.wordWrappedLabel, GUILayout.ExpandWidth(true));
        }

        void ShowAdvancedIntegration()
        {
            GUIStyle m_CodeStyle = new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleLeft,
                fontStyle = FontStyle.BoldAndItalic
            };
            EditorGUILayout.LabelField(k_AdvancedInfoHeader, EditorStyles.wordWrappedLabel, GUILayout.ExpandWidth(true));
            GUILayout.Box(k_AdvancedAPIInfo, m_CodeStyle);
            EditorGUILayout.LabelField(k_AdvancedInfoAPIUsage, EditorStyles.wordWrappedLabel, GUILayout.ExpandWidth(true));
            GUILayout.Box(k_AdvancedIntegration, m_CodeStyle);
            EditorGUILayout.LabelField(k_AdvancedInfoAPIreturn, EditorStyles.wordWrappedLabel, GUILayout.ExpandWidth(true));
            GUILayout.Box(k_AdvancedInfoIteration, m_CodeStyle);
        }

        void setDefaultSettings(bool enable)
        {
            m_EditorState.enableDefault = enable;
            m_SetupConfig.SetDefaultPredictions(m_EditorState.enableDefault);
            m_EditorStatePrefs.Set(m_EditorState);
            EditorUtility.SetDirty(m_SetupConfig);
        }

        [Serializable]
        public class EditorState
        {
            public bool enableDefault;
        }

    }
}