Shader "SpaceEngine/Ocean/WhiteCapsPrecompute" 
{
	SubShader 
	{
		Pass 
		{
			ZTest Always 
			Cull Off 
			ZWrite Off
			Fog { Mode Off }
			
			CGPROGRAM
			#include "UnityCG.cginc"

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			
			uniform sampler2D _Map5;
			uniform sampler2D _Map6;
			uniform sampler2D _Map7;
			uniform float4 _Choppyness;

			struct a2v
			{
				float4 vertex : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f 
			{
				float4  pos : SV_POSITION;
				float2  uv : TEXCOORD0;
			};
			
			struct f2a
			{
				float4 col0 : COLOR0;
				float4 col1 : COLOR1;
			};

			void vert(in a2v i, out v2f o)
			{
				o.pos = UnityObjectToClipPos(i.vertex);
				o.uv = i.texcoord;
			}
			
			void frag(in v2f IN, out f2a OUT)
			{ 
				float2 uv = IN.uv;
				
				// Store Jacobian coeff value and variance
				float4 Jxx = _Choppyness * tex2D(_Map5, uv);
				float4 Jyy = _Choppyness * tex2D(_Map6, uv);
				float4 Jxy = _Choppyness * _Choppyness * tex2D(_Map7, uv);
			
				// Store partial jacobians
				float4 res = 0.25 + Jxx + Jyy + _Choppyness * Jxx * Jyy - Jxy * Jxy;
				float4 res2 = res*res;
				
				OUT.col0 = float4(res.x, res2.x, res.y, res2.y);
				OUT.col1 = float4(res.z, res2.z, res.w, res2.w);
			}
			
			ENDCG
		}
	}
}
