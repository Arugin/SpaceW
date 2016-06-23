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

using UnityEngine;

public class Cloudsphere : MonoBehaviour
{
    public Planetoid planetoid;

    public Mesh CloudsphereMesh;

    public Shader CloudShader;
    public Material CloudMaterial;

    public EngineRenderQueue RenderQueue = EngineRenderQueue.Transparent;
    public int RenderQueueOffset = 0;

    public float Radius;
    public float Height;

    [Range(0.0f, 1.0f)]
    public float TransmittanceOffset = 0.625f;

    public Cubemap CloudTexture;

    private void Start()
    {
        InitMaterials();
    }

    private void Update()
    {

    }

    public void Render(int drawLayer = 8)
    {
        SetUniforms(CloudMaterial);

        CloudMaterial.renderQueue = (int)RenderQueue + RenderQueueOffset;

        if (CloudsphereMesh == null) return;

        Matrix4x4 CloudsTRS = Matrix4x4.TRS(planetoid.transform.position, transform.rotation, Vector3.one * (Radius + Height));

        Graphics.DrawMesh(CloudsphereMesh, CloudsTRS, CloudMaterial, drawLayer, CameraHelper.Main(), 0, planetoid.QuadAtmosphereMPB);
    }

    public void InitMaterials()
    {
        if (CloudMaterial == null)
        {
            CloudMaterial = new Material(CloudShader);
            CloudMaterial.name = "Clouds" + "(Instance)" + Random.Range(float.MinValue, float.MaxValue);
        }
    }

    public void InitUniforms()
    {
        InitUniforms(CloudMaterial);
    }

    public void InitUniforms(Material mat)
    {
        if (mat == null) return;

        if (planetoid != null)
        {
            if (planetoid.Atmosphere != null)
            {
                planetoid.Atmosphere.InitUniforms(planetoid.QuadAtmosphereMPB, mat, true);
            }
        }

        if (CloudTexture != null) mat.SetTexture("_Cloud", CloudTexture);

        mat.SetFloat("_TransmittanceOffset", TransmittanceOffset);
    }

    public void SetUniforms()
    {
        SetUniforms(CloudMaterial);
    }

    public void SetUniforms(Material mat)
    {
        if (mat == null) return;

        if (planetoid != null)
        {
            if (planetoid.Atmosphere != null)
            {
                planetoid.Atmosphere.SetUniforms(planetoid.QuadAtmosphereMPB, mat, true);
            }
        }

        if (CloudTexture != null) mat.SetTexture("_Cloud", CloudTexture);

        mat.SetFloat("_TransmittanceOffset", TransmittanceOffset);
    }
}