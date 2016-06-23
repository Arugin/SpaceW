﻿#region License
// Procedural planet generator.
// 
// Copyright (C) 2015-2016 Denis Ovchinnikov [zameran] 
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

using System;

using UnityEngine;

using ZFramework.Unity.Common.Threading;

[Serializable]
public sealed class QuadTextureCache : QuadCache
{
    public RenderTexture HeightTexture;
    public RenderTexture NormalTexture;

    public QuadTextureCache(Quad.Id id, QuadStorage owner) : base(id, owner)
    {

    }

    public override void Init()
    {
        this.HeightTexture = RTExtensions.CreateRTexture(QuadSettings.nVertsPerEdgeSub, 0);
        this.NormalTexture = RTExtensions.CreateRTexture(QuadSettings.nVertsPerEdgeSub, 0);

        RTUtility.ClearColor(new RenderTexture[] { this.HeightTexture, this.NormalTexture });

        base.Init();
    }

    public override void TransferTo(Quad q)
    {
        RenderTexture.active = null;

        if (Owner.Multithreaded)
        {
            Dispatcher.InvokeAsync(() =>
            {
                Graphics.Blit(this.HeightTexture, q.HeightTexture);
                Graphics.Blit(this.NormalTexture, q.NormalTexture);
            });
        }
        else
        {
            Dispatcher.Invoke(() =>
            {
                Graphics.Blit(this.HeightTexture, q.HeightTexture);
                Graphics.Blit(this.NormalTexture, q.NormalTexture);
            });
        }

        base.TransferTo(q);
    }

    public override void TransferFrom(Quad q)
    {
        RenderTexture.active = null;

        if (Owner.Multithreaded)
        {
            Dispatcher.InvokeAsync(() =>
            {
                Graphics.Blit(q.HeightTexture, this.HeightTexture);
                Graphics.Blit(q.NormalTexture, this.NormalTexture);
            });
        }
        else
        {
            Dispatcher.Invoke(() =>
            {
                Graphics.Blit(q.HeightTexture, this.HeightTexture);
                Graphics.Blit(q.NormalTexture, this.NormalTexture);
            });
        }

        base.TransferTo(q);
    }

    public override void OnDestroy()
    {
        if (this.HeightTexture != null)
            this.HeightTexture.ReleaseAndDestroy();

        if (this.NormalTexture != null)
            this.NormalTexture.ReleaseAndDestroy();

        base.OnDestroy();
    }
}