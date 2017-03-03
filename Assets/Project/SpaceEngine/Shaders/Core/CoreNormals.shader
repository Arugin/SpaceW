﻿Shader "SpaceEngine/Terrain/CoreNormals" 
{
	SubShader 
	{
		CGINCLUDE

		#include "UnityCG.cginc"

		uniform sampler2D _ElevationSampler; 

		uniform float3 _TileSD;

		uniform float4 _ElevationOSL; 			
		uniform float4 _Deform;
		uniform float4 _PatchCornerNorms;

		uniform float4x4 _PatchCorners;
		uniform float4x4 _PatchVerticals;
		uniform float4x4 _WorldToTangentFrame;

		struct v2f 
		{
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
			float2 st : TEXCOORD1;
		};

		float3 GetWorldPosition(float2 uv, float h) 
		{
			uv = uv / (_TileSD.x - 1.0);
				
			if (_Deform.w == 0.0) 
			{
				return float3(_Deform.xy + _Deform.z * uv, h);
			} 
			else 
			{
				float R = _Deform.w;

				float4 L = _PatchCornerNorms;
				float4 uvUV = float4(uv, float2(1.0, 1.0) - uv);
				float4 aL = uvUV.zxzx * uvUV.wwyy;
				float4 alphaPrime = aL * L / dot(aL, L);
				float4 up = mul(_PatchVerticals, alphaPrime);

				float k = lerp(length(up.xyz), 1.0, smoothstep(R / 32.0, R / 64.0, _Deform.z));
				float hPrime = (h + R * (1.0 - k)) / k;
				
				return (mul(_PatchCorners, alphaPrime) + hPrime * up).xyz;
			}

			return 0;
		}

		float4 CalculateNormal(float2 uv)
		{
			uv = floor(uv);

			float4 uv0 = floor(uv.xyxy + float4(-1.0, 0.0, 1.0, 0.0)) * _ElevationOSL.z + _ElevationOSL.xyxy;
			float4 uv1 = floor(uv.xyxy + float4(0.0, -1.0, 0.0, 1.0)) * _ElevationOSL.z + _ElevationOSL.xyxy;
				
			float z0 = tex2Dlod(_ElevationSampler, float4(uv0.xy, 0.0, 0.0)).x;
			float z1 = tex2Dlod(_ElevationSampler, float4(uv0.zw, 0.0, 0.0)).x;
			float z2 = tex2Dlod(_ElevationSampler, float4(uv1.xy, 0.0, 0.0)).x;
			float z3 = tex2Dlod(_ElevationSampler, float4(uv1.zw, 0.0, 0.0)).x;

			float3 p0 = GetWorldPosition(uv + float2(-1.0, 0.0), z0).xyz;
			float3 p1 = GetWorldPosition(uv + float2(+1.0, 0.0), z1).xyz;
			float3 p2 = GetWorldPosition(uv + float2(0.0, -1.0), z2).xyz;
			float3 p3 = GetWorldPosition(uv + float2(0.0, +1.0), z3).xyz;
				
			float3 nf = (mul((float3x3)_WorldToTangentFrame, normalize(cross(p1 - p0, p3 - p2)))).xyz;
				
			return float4(nf, 0);
		}

		void vert(in appdata_base v, out v2f o)
		{	
			o.pos = mul(UNITY_MATRIX_MVP, v.vertex);
			o.uv = v.texcoord.xy;
			o.st = v.texcoord.xy * _TileSD.x;
		}

		void frag(in v2f IN, out float4 output : COLOR)
		{
			output = CalculateNormal(IN.st);
		}

		ENDCG

		Pass
		{
			ZTest Always 
			Cull Off 
			ZWrite Off
			Fog { Mode Off }

			CGPROGRAM
			#pragma target 4.0
			#pragma vertex vert
			#pragma fragment frag			
			ENDCG
		}
	}
}