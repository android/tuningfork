//-----------------------------------------------------------------------
// <copyright file="CProtobufSerialization.cs" company="Google">
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

using System;
using System.Runtime.InteropServices;
using AOT;
using Google.Protobuf;
using UnityEngine;

namespace Google.Android.PerformanceTuner
{
    /// <summary>
    /// Structure that helps to keep, serialize and deserialize data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct CProtobufSerialization
    {
        public IntPtr bytes;
        public UInt32 size;
        public IntPtr dealloc;

        /// <summary>
        /// Create serialization from proto message.
        /// </summary>
        /// <param name="message">proto message</param>
        /// <returns></returns>
        public static CProtobufSerialization Create(IMessage message)
        {
            byte[] bmsg = message.ToByteArray();
            IntPtr p = Marshal.AllocHGlobal(bmsg.Length);
            Marshal.Copy(bmsg, 0, p, bmsg.Length);
            return new CProtobufSerialization()
            {
                bytes = p,
                size = (UInt32) bmsg.Length,
                dealloc = Marshal.GetFunctionPointerForDelegate<DeallocCallback>(DeallocCallbackImpl)
            };
        }

        delegate void DeallocCallback(ref CProtobufSerialization parameters);

        [MonoPInvokeCallback(typeof(DeallocCallback))]
        static void DeallocCallbackImpl(ref CProtobufSerialization parameters)
        {
            Debug.LogFormat("DeallocCallbackImpl.size: {0}", parameters.size);
            if (parameters.bytes != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(parameters.bytes);
            }

            parameters.bytes = parameters.dealloc = IntPtr.Zero;
            parameters.size = 0;
        }

        /// <summary>
        /// Free all memory allocated by serialization.
        /// </summary>
        /// <param name="ps">The serialized proto message.</param>
        public static void CallDealloc(ref CProtobufSerialization ps)
        {
            if (ps.dealloc == IntPtr.Zero)
            {
                return;
            }

            DeallocCallback dealloc =
                Marshal.GetDelegateForFunctionPointer<DeallocCallback>(ps.dealloc);
            if (dealloc != null)
            {
                dealloc(ref ps);
            }
        }

        /// <summary>
        /// Parse a proto message from serialization.
        /// </summary>
        /// <typeparam name="T">Parser for proto message.</typeparam>
        public T ParseMessage<T>() where T : class, IMessage<T>, new()
        {
            var parser = new MessageParser<T>(() => new T());
            if (size == 0) return new T();
            try
            {
                byte[] received = new byte[size];
                Marshal.Copy((IntPtr) bytes, received, 0, (int) size);

                // TODO(b/120588304) Check and remove if it is not needed.
                int zero_trail = 0;
                for (; zero_trail < received.Length; zero_trail++)
                {
                    if (received[received.Length - 1 - zero_trail] != 0)
                    {
                        break;
                    }
                }

                ByteString toParse = ByteString.CopyFrom(received, 0, received.Length - zero_trail);

                T message = parser.ParseFrom(toParse);
                return message;
            }
            catch (Exception e)
            {
                Debug.LogWarning("Cannot parse message, " + e.ToString());
                return null;
            }
        }
    }
}