//-----------------------------------------------------------------------
// <copyright file="ProtocCompiler.cs" company="Google">
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
using System.IO;
using System.Diagnostics;

namespace Google.Android.PerformanceTuner.Editor
{
    public class ProtocCompiler
    {
        static string binaryPath
        {
            get
            {
                var path = Paths.protocPath;

#if UNITY_EDITOR_WIN
                path = Path.Combine(path, "win32", "protoc.exe");
#elif UNITY_EDITOR_OSX
                path = Path.Combine(path, "mac", "protoc");
#else
                path = Path.Combine(path, "linux64", "protoc");
#endif
                return path;
            }
        }

        public static bool GenerateProtoAndDesc()
        {
            var inFolder = Path.GetDirectoryName(Paths.devProtoPath);
            Directory.CreateDirectory(Paths.devCsOutDirectoryPath);
            return CompileProto(Paths.devProtoPath, new string[1] {inFolder}, Paths.devCsOutDirectoryPath) &&
                   CreateDescriptor(Paths.devProtoPath, Paths.devDescriptorPath);
        }

        static bool CompileProto(string protoFilePath, string[] includePaths, string outputPath)
        {
            if (Path.GetExtension(protoFilePath) != ".proto") return false;
            if (!File.Exists(binaryPath))
            {
                UnityEngine.Debug.LogErrorFormat("Protoc binary file does not exist: {0}", binaryPath);
                return false;
            }

            if (!File.Exists(protoFilePath))
            {
                UnityEngine.Debug.LogErrorFormat("Proto file does not exist: {0}", protoFilePath);
                return false;
            }

            Directory.CreateDirectory(outputPath);

            string args = string.Format("\"{0}\" --csharp_out \"{1}\" ", protoFilePath, outputPath);
            foreach (string s in includePaths)
            {
                args += string.Format(" --proto_path \"{0}\" ", s);
            }

            var startInfo = new ProcessStartInfo() {FileName = binaryPath, Arguments = args};

            var proc = new Process() {StartInfo = startInfo};
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();

            string output = proc.StandardOutput.ReadToEnd();
            string error = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (!string.IsNullOrEmpty(output))
            {
                UnityEngine.Debug.Log("ProtocCompiler output:\n" + output);
            }

            if (!string.IsNullOrEmpty(error))
            {
                UnityEngine.Debug.LogErrorFormat("ProtocCompiler error:\n" + error);
            }

            AssetDatabase.Refresh();

            return string.IsNullOrEmpty(error);
        }

        static bool CreateDescriptor(string protoFilePath, string outputPath)
        {
            if (Path.GetExtension(protoFilePath) != ".proto") return false;
            if (!File.Exists(binaryPath))
            {
                UnityEngine.Debug.LogErrorFormat("Protoc binary file does not exist: {0}", binaryPath);
                return false;
            }

            if (!File.Exists(protoFilePath))
            {
                UnityEngine.Debug.LogErrorFormat("Proto file does not exist: {0}", protoFilePath);
                return false;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(outputPath));

            string args = string.Format(
                "\"{0}\" -I\"{1}\" -o\"{2}\"", protoFilePath, Path.GetDirectoryName(protoFilePath), outputPath);

            var startInfo = new ProcessStartInfo() {FileName = binaryPath, Arguments = args};

            var proc = new Process() {StartInfo = startInfo};
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;
            proc.Start();

            string output = proc.StandardOutput.ReadToEnd();
            string error = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (!string.IsNullOrEmpty(output))
            {
                UnityEngine.Debug.Log("ProtocCompiler output:\n" + output);
            }

            if (!string.IsNullOrEmpty(error))
            {
                UnityEngine.Debug.LogError("ProtocCompiler error:\n" + error);
            }

            AssetDatabase.Refresh();
            return string.IsNullOrEmpty(error);
        }

        public static string GetVersion()
        {
            if (!File.Exists(binaryPath)) return string.Empty;

            var info = new ProcessStartInfo()
            {
                FileName = binaryPath,
                Arguments = "--version"
            };
            var proc = new Process() {StartInfo = info};
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            string output = proc.StandardOutput.ReadToEnd();
            string error = proc.StandardError.ReadToEnd();

            proc.WaitForExit(120);

            if (!string.IsNullOrEmpty(error))
            {
                UnityEngine.Debug.LogFormat("getProtocVersion.error:{0}", error);
            }

            return output.Trim();
        }
    }
}