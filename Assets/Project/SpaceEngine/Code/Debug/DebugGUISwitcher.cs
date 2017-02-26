﻿#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2017 Denis Ovchinnikov [zameran] 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions
// are met:
// 1. Redistributions of source code must retain the above copyright
//    notice, this list of conditions and the following disclaimer.
// 2. Redistributions in binary form must reproduce the above copyright
//    notice, this list of conditions and the following disclaimer in the
//    documentation and/or other materials provided with the distribution.
// 3. Neither the name of the copyright holders nor the names of its
//    contributors may be used to endorse or promote products derived from
//    this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
// INTERRUPTION)HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
// 
// Creation Date: Undefined
// Creation Time: Undefined
// Creator: zameran
#endregion

using UnityEngine;

namespace SpaceEngine.Debugging
{
    public sealed class DebugGUISwitcher : MonoBehaviour, IDebugSwitcher
    {
        public GUISkin skin;

        public DebugGUI[] DebugComponents;

        public int state = 0;

        private void Start()
        {
            if (DebugComponents == null || DebugComponents.Length == 0)
            {
                DebugComponents = GetComponents<DebugGUI>();
            }

            ToogleAll(DebugComponents, false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F5))
            {
                if (state == DebugComponents.Length)
                {
                    state = 0;
                    ToogleAll(DebugComponents, false);
                    return;
                }

                ToogleAll(DebugComponents, false);
                state++;
                ToogleAt(DebugComponents, true, state);
            }
        }

        public void Toogle(DebugGUI GUI, bool state)
        {
            GUI.enabled = state;
        }

        public void ToogleAt(DebugGUI[] GUIs, bool state, int index)
        {
            GUIs[index - 1].enabled = state;
        }

        public void ToogleAll(DebugGUI[] GUIs, bool state)
        {
            for (int i = 0; i < GUIs.Length; i++)
            {
                GUIs[i].enabled = false;
            }
        }
    }
}