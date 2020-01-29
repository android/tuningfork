//-----------------------------------------------------------------------
// <copyright file="TuningforkSample.cs" company="Google">
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

using UnityEngine;

namespace Google.Android.PerformanceParameters.Sample
{
    public class TuningforkSample : MonoBehaviour
    {
        Tuningfork m_Tuningfork = new Tuningfork();

        private void Start()
        {
            // Always init Tuningfork before calling any other function
            var errorCode = m_Tuningfork.Start();
            if (errorCode != TFErrorCode.Ok)
            {
                Debug.Log("Tuningfork is not initialized");
                return;
            }

            // Custom Fidelity Parameters:
            // If you are using custom fidelity parameters, subscribe to OnReceiveFidelityParameters event
            // and update your game quality when new fidelity parameters are received.
            // Default Fidelity Parameters:
            // If you are using default fidelity parameters, your QualitySettings will be updated by Tuningfork,
            // no need to subscribe to OnReceiveFidelityParameters in that case.
            m_Tuningfork.OnReceiveFidelityParameters += (fidelityParameters) =>
            {
                // Update your game quality here
                // e.g, set texture size, lights count, shadow distance, etc.
            };


            // Custom Annotation:
            // If you are using custom annotation you need to call SetCurrentAnnotation.
            // All frame rate information collected by Tuningfork will be associated with that annotation.
            // Set annotation each time your game state has changed.
            // Default Annotation:
            // If you are using default annotation, don't call SetCurrentAnnotation.
            // Every time your active scene has changed, SetCurrentAnnotation will be called by Tuningfork.
            var annotation = new Annotation()
            {
                // Set annotation values describing your current game state.
            };
            m_Tuningfork.SetCurrentAnnotation(annotation);
        }

        public void GameSettingsChanged(params string[] yourGameSpecificSettings)
        {
            // If your game settings was changed in some other way (e.g. by user from game settings menu),
            // you need to call SetFidelityParameters.
            var fidelityParams = new FidelityParams()
            {
                // Set fidelity parameters your game is currently in.
            };
            m_Tuningfork.SetFidelityParameters(fidelityParams);
        }
    }
}