//-----------------------------------------------------------------------
// <copyright file="Names.cs" company="Google">
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


namespace Google.Android.PerformanceTuner.Editor
{
    /// <summary>
    ///     Common names across the plugin.
    /// </summary>
    public static class Names
    {
        public const string sceneEnumName = "Scene";
        public const string loadingStateEnumName = "LoadingState";
        public const string sceneFieldName = "scene";
        public const string loadingStateFieldName = "loading_state";

        public const string removeLoadingStateTooltip =
            "This parameter is obsolete. You should remove it from annotation and use \"StartRecordingLoadingTime\" " +
            "and \"StopRecordingLoadingTime\" instead.";

        public const string fixDefaultAnnotationMessage =
            "Loading state field in Annotation is deprecated." +
            "You should not use it any longer. \n" +
            "Use \"StartRecordingLoadingTime\" and \"StopRecordingLoadingTime\" instead. \n" +
            "Press \"Fix\" to remove loading state from default annotation. \n" +
            "Important: please remove any references to LoadingState from your project, you might get a compilier error " +
            "\" CS0117: 'Annotation' does not contain a definition for 'LoadingState'\" otherwise.";

        public const string fixDefaultAnnotationConsoleMessage =
            "Loading state field in Annotation is deprecated." +
            "You should not use it any longer. " +
            "Use \"StartRecordingLoadingTime\" and \"StopRecordingLoadingTime\" instead. " +
            "Go to \"Android Performance Tuner -> Setup -> Annotation parameters\" to fix it";
    }
}