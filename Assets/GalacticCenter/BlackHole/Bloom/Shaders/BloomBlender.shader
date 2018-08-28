Shader "Custom/BloomBlender" {
	Properties {
		_MainTex ("Texture", 2D) = "white" {}
	}

	CGINCLUDE
		#include "UnityCG.cginc"

		sampler2D _MainTex,_SourceTex;
		float _Intensity, _Threshold, _SampleDeltaDown, _SampleDeltaUp;
		float4 _MainTex_TexelSize;

		struct appData {
			float4 vertex : POSITION;
			float2 uv : TEXCOORD0;
		};

		struct v2f {
			float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
		};

		v2f vert (appData v) {
			v2f i;
			i.pos = UnityObjectToClipPos(v.vertex);
			i.uv = v.uv;
			return i;
		}

		half3 Prefilter (half3 c) 
		{
			half brightness = max(c.r, max(c.g, c.b));
			half contribution = max(0, brightness - _Threshold);
			contribution /= max(brightness, 0.00001);
			return c * contribution; 
		}

		half3 Sample (float2 uv) 
		{
			return tex2D(_MainTex, uv).rgb;
		}

		half3 SampleBox (float2 uv, float delta) 
		{
			float4 o = _MainTex_TexelSize.xyxy * float2(-delta, delta).xxyy;
			half3 s =
				Sample(uv + o.xy) + Sample(uv + o.zy) +
				Sample(uv + o.xw) + Sample(uv + o.zw);
			return s * 0.25f;
		}
	ENDCG

	SubShader {
		Cull Off
		ZTest Always
		ZWrite Off

		// Prefilter and resample down
		Pass {
			// Blend One One
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				half4 frag (v2f i) : SV_Target 
				{
					return half4(Prefilter(SampleBox(i.uv, _SampleDeltaDown)), 1);
				}
			ENDCG
		}

		// Resample Down
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				half4 frag (v2f i) : SV_Target 
				{
					return half4(SampleBox(i.uv, _SampleDeltaDown), 1);
				}
			ENDCG
		}

		// Resample Up
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				half4 frag (v2f i) : SV_Target 
				{
					return half4(SampleBox(i.uv, _SampleDeltaUp), 1); 
				}
			ENDCG
		}

		// Final blend
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				half4 frag (v2f i) : SV_Target 
				{
					half4 source = tex2D(_SourceTex, i.uv);
					half4 mainTexCol = tex2D(_MainTex, i.uv);
					return source + mainTexCol * _Intensity;
					// return mainTexCol * _Intensity;
				}
			ENDCG
		}

		// Final upscale and blend
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				half4 frag (v2f i) : SV_Target 
				{
					return tex2D(_SourceTex, i.uv) + half4(SampleBox(i.uv, _SampleDeltaUp), 1) * _Intensity;
				}   
			ENDCG
		}

		// Final downscale and blend
		Pass {
			CGPROGRAM
				#pragma vertex vert
				#pragma fragment frag

				half4 frag (v2f i) : SV_Target 
				{
					return tex2D(_SourceTex, i.uv) + half4(SampleBox(i.uv, _SampleDeltaDown), 1) * _Intensity;
				}   
			ENDCG
		}
	}
}