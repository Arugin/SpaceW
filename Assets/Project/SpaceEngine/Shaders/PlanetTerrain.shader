﻿// Procedural planet generator.
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

Shader "SpaceEngine/Planet/Terrain (Deferred)"
{
	Properties
	{
		[NoScaleOffset] _Elevation_Tile("Elevation", 2D) = "white" {}
		[NoScaleOffset] _Normals_Tile("Normals", 2D) = "white" {}
		[NoScaleOffset] _Color_Tile("Color", 2D) = "white" {}
		[NoScaleOffset] _Ortho_Tile("Ortho", 2D) = "white" {}

		[NoScaleOffset] _Ground_Diffuse("Ground Diffuse", 2D) = "white" {}
		[NoScaleOffset] _Ground_Normal("Ground Normal", 2D) = "white" {}
	}
	SubShader 
	{
		CGINCLUDE
		#include "Core.cginc"
		ENDCG

		Pass 
		{
			Name "Planet"
			Tags 
			{
				"Queue"					= "Geometry"
				"RenderType"			= "Geometry"
				"ForceNoShadowCasting"	= "True"
				"IgnoreProjector"		= "True"

				"LightMode"				= "Deferred"
			}

			//Blend SrcAlpha OneMinusSrcColor
			Cull Back
			ZWrite On
			ZTest LEqual

			CGPROGRAM
			#pragma target 4.0
			#pragma only_renderers d3d11 glcore
			#pragma vertex vert
			#pragma fragment frag

			#pragma multi_compile ATMOSPHERE_ON ATMOSPHERE_OFF
			#pragma multi_compile SHINE_ON SHINE_OFF
			#pragma multi_compile ECLIPSES_ON ECLIPSES_OFF
			#pragma multi_compile OCEAN_ON OCEAN_OFF
			#pragma multi_compile OCEAN_DEPTH_ON OCEAN_DEPTH_OFF
			#pragma multi_compile SHADOW_0 SHADOW_1 SHADOW_2 SHADOW_3 SHADOW_4

			#include "SpaceStuff.cginc"
			#include "SpaceEclipses.cginc"
			#include "SpaceAtmosphere.cginc"
			#include "Ocean/OceanBRDF.cginc"
			
			struct v2f_planetTerrain
			{
				float4 pos : POSITION;
				float2 texcoord : TEXCOORD0;
				float3 localVertex : TEXCOORD1;
				float3 direction : TEXCOORD2;
			};

			void vert(in a2v_planetTerrain v, out v2f_planetTerrain o)
			{	
				VERTEX_POSITION(v.vertex, v.texcoord.xy, o.pos, o.localVertex, o.texcoord);

				v.vertex = o.pos;

				o.direction = (_Atmosphere_WorldCameraPos + _Atmosphere_Origin) - (mul(_Globals_CameraToWorld, float4((mul(_Globals_ScreenToCamera, v.vertex)).xyz, 0.0))).xyz;
			}

			void frag(in v2f_planetTerrain i, out DeferredOutput o)
			{
				float3 WCP = _Globals_WorldCameraPos;
				float3 WSD = _Sun_WorldDirections_1[0];
				float4 WSPR = _Sun_Positions_1[0];
				float3 position = i.localVertex;
				float2 texcoord = i.texcoord;

				float height = texTile(_Elevation_Tile, texcoord, _Elevation_TileCoords, _Elevation_TileSize).x;
				float4 ortho = texTile(_Ortho_Tile, texcoord, _Ortho_TileCoords, _Ortho_TileSize);
				float4 color = texTile(_Color_Tile, texcoord, _Color_TileCoords, _Color_TileSize);
				float4 normal = texTile(_Normals_Tile, texcoord, _Normals_TileCoords, _Normals_TileSize);

				normal.xyz = DecodeNormal(normal.xyz);

				/*
				float2 vert = (texcoord * _TileSD.y - _TileSD.x) * _Offset.z + _Offset.xy;
				float4 scaledUV = float4(vert / _Offset.w, 0.0, 0.0);
				float4 realUV = frac(scaledUV);
				float4 planetUVColor = tex2D(_PlanetUV, realUV.xy);
				*/

				float3 V = normalize(position);
				float3 P = V * max(length(position), _Deform_Radius + _Globals_RadiusOffset);
				float3 d = normalize(i.direction);

				#if ATMOSPHERE_ON
					#if OCEAN_ON
						if (height <= _Ocean_Level && _Ocean_DrawBRDF == 1.0) {	normal = float4(0.0, 0.0, 1.0, 1.0); }
					#endif
				#endif
				
				normal.xyz = mul(_Deform_TangentFrameToWorld, normal.xyz);

				float4 reflectance = lerp(ortho, color, clamp(length(color.xyz), 0.0, 1.0)); // Just for tests...
				float cTheta = dot(normal.xyz, WSD);

				#if ECLIPSES_ON
					float eclipse = 1.0;

					float3 invertedLightDistance = rsqrt(dot(WSPR.xyz, WSPR.xyz));
					float3 lightPosition = WSPR.xyz * invertedLightDistance;

					float lightAngularRadius = asin(WSPR.w * invertedLightDistance);

					eclipse *= EclipseShadow(P, lightPosition, lightAngularRadius);
				#endif

				#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
					// TODO : Use planet origin instead...
					float3 P_A = P - _Atmosphere_Origin;
					float shadow = ShadowColor(float4(P_A, 1.0));	// Body origin take in to account...
				#endif

				#if ATMOSPHERE_ON
					float3 WCP_A = _Atmosphere_WorldCameraPos;

					float3 sunL = 0.0;
					float3 skyE = 0.0;
					SunRadianceAndSkyIrradiance(P, normal.xyz, WSD, sunL, skyE);

					float3 groundColor = 1.5 * RGB2Reflectance(reflectance).rgb * (sunL * max(cTheta, 0) + skyE) / M_PI;
					
					#if OCEAN_ON
						if (height <= _Ocean_Level && _Ocean_DrawBRDF == 1.0)
						{
							float3 oceanColor = 0.0;

							#if OCEAN_DEPTH_ON
								float depthCoeff = ShoreCoefficient(_Ocean_Level, height);
								
								float3 shoreColor = AbsorbtionColor(_Ocean_AbsorbtionRGBA, _Ocean_AbsorbtionTint, depthCoeff);

								shoreColor = lerp(reflectance.rgb * depthCoeff, shoreColor, 0.5);
								oceanColor = lerp(_Ocean_Color, shoreColor, depthCoeff);
							#else
								oceanColor = _Ocean_Color;
							#endif

							float3 v = normalize(P - WCP - _Atmosphere_Origin); // Body origin take in to account...

							groundColor = OceanRadiance(WSD, -v, V, _Ocean_Sigma, sunL, skyE, oceanColor, P);
						}
					#endif
					
					float darknessAccumulation = 1.0;
					float3 extinction;
					float3 glowExtinction;
					float3 inscatter = 0.0;

					inscatter += InScattering(WCP_A, P, float3(0.0, 0.0, 0.0), glowExtinction, 0.0) * _Atmosphere_GlowColor;
					inscatter += InScattering(WCP_A, P, WSD, extinction, 0.0);

					#if ECLIPSES_ON
						inscatter *= eclipse;
					#endif

					#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
						inscatter *= shadow;
					#endif

					#if SHINE_ON
						inscatter += SkyShineRadiance(P, d);
					#endif

					#if ECLIPSES_ON
						#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
							darknessAccumulation = eclipse * shadow;
						#else
							darknessAccumulation = eclipse;
						#endif
					#else
						#if SHADOW_1 || SHADOW_2 || SHADOW_3 || SHADOW_4
							darknessAccumulation = shadow;
						#endif
					#endif

					extinction = GroundFade(_ExtinctionGroundFade, extinction, darknessAccumulation);

					float3 finalColor = hdr(groundColor * extinction + inscatter);
				#elif ATMOSPHERE_OFF			
					normal.xyz = -normal.xyz * 0.5 + 0.5; // Encode normal... (Using inversed normal)

					float3 finalColor = 1.5 * reflectance * max(cTheta, 0);
				#endif

				o.diffuse = float4(finalColor, 1.0);
				o.specular = float4(0.0, 0.0, 0.0, 1.0);
				o.normal = float4(normal.xyz, 1.0);
				o.emission = float4(0.0, 0.0, 0.0, 1.0);
			}
			
			ENDCG
		}
	}
}