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
//     notice, this list of conditions and the following disclaimer.
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
// Creation Date: 2017.03.06
// Creation Time: 12:38 AM
// Creator: zameran
#endregion

using System;
using System.Collections.Generic;

using UnityEngine;

namespace SpaceEngine.Managers
{
    [Serializable]
    public class SequenceEntry
    {
        public class Sort : IComparer<SequenceEntry>
        {
            int IComparer<SequenceEntry>.Compare(SequenceEntry a, SequenceEntry b)
            {
                if (a.TimeSinceStartup > b.TimeSinceStartup)
                    return 1;
                if (a.TimeSinceStartup < b.TimeSinceStartup)
                    return -1;
                else
                    return 0;
            }
        }

        public string Name { get; set; }

        public float Time { get; private set; }
        public float TimeSinceStartup { get; private set; }

        public int Frame { get; private set; }

        public SequenceEntry(string name)
        {
            Name = name;

            Time = UnityEngine.Time.time;
            TimeSinceStartup = UnityEngine.Time.realtimeSinceStartup;

            Frame = UnityEngine.Time.frameCount;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;

            var p = obj as SequenceEntry;

            if (p == null) return false;

            return (Name == p.Name) && (Frame == p.Frame);
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ TimeSinceStartup.GetHashCode() + Frame.GetHashCode();
        }
    }

    public class DebugSequenceManager : MonoSingleton<DebugSequenceManager>
    {
        public List<SequenceEntry> Sequence = new List<SequenceEntry>();
        public List<SequenceEntry> NestedSequence = new List<SequenceEntry>();

        public SequenceEntry.Sort Sorter = new SequenceEntry.Sort();

        private void Awake()
        {
            Instance = this;
        }

        public void Debug(MonoBehaviour owner)
        {
            var entry = new SequenceEntry(string.Format("{0} [{1}]", owner.name, owner.GetType().Name));

            if (Sequence.Contains(entry)) { NestedSequence.Add(entry); }
            else Sequence.Add(entry);

            Sequence.Sort(Sorter);
            NestedSequence.Sort(Sorter);
        }
    }
}