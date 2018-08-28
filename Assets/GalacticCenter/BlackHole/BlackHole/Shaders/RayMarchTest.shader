Shader "RayMarched/GravitationalLensing"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BlackHoleMass("Black Hole Mass", Range(0,10)) = 0.0000001
		_StepSize("RayMarch Step Size", Float) = .2
		_MaxStepCount("Max Step Count", Int) = 64
		_EventHorizonDistance("Event Horizon Distance", Float) = .001
	}
	SubShader
	{
		Cull Off
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
				float3 worldSpacePosition : TEXCOORD1;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float _BlackHoleMass, _StepSize, _EventHorizonDistance;
			int _MaxStepCount;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.worldSpacePosition = mul(unity_ObjectToWorld, v.vertex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}

			// Torus
			// t.x: diameter
			// t.y: thickness
			// Adapted from: http://iquilezles.org/www/articles/distfunctions/distfunctions.htm
			float sdTorus(float3 p, float2 t)
			{
				float2 q = float2(length(p.xz) - t.x, p.y);
				return length(q) - t.y;
			}

			float sdSphere(float3 p, float s )
			{
				return length(p)-s;
			}

			// This is the distance field function.  The distance field represents the closest distance to the surface
			// of any object we put in the scene.  If the given point (point p) is inside of an object, we return a
			// negative answer.
			float map(float3 p) 
			{
				return sdSphere(p, 1);
			}

			fixed4 gravityRayMarch(float3 rayOrigin, float3 rayDirection)
			{
				float3 massCentre = float3(0,0,0);

				fixed4 returnColour = fixed4(1,1,1,1);

				float3 currentRayPosition = rayOrigin;
				float3 currentRayDirection = rayDirection;

				for (int i = 0; i < _MaxStepCount; ++i) 
				{
					float3 displacement = massCentre - currentRayPosition;

					float squareDistance = dot(displacement, displacement);

					if(squareDistance < (_EventHorizonDistance * _EventHorizonDistance) * (_BlackHoleMass * _BlackHoleMass))
					{
						return (0,0,0,0);
					}

					float forceMagnitude = _BlackHoleMass / squareDistance;

					currentRayDirection += forceMagnitude * displacement * _StepSize;

					currentRayPosition += normalize(currentRayDirection) * _StepSize;
				}

				// sample the default reflection cubemap, using the reflection vector
                half4 skyData = UNITY_SAMPLE_TEXCUBE(unity_SpecCube0, currentRayDirection);
                // decode cubemap data into actual color
                half3 skyColor = DecodeHDR (skyData, unity_SpecCube0_HDR);

				returnColour.rgb = skyColor;

				return returnColour;
			}

			// Raymarch along given ray
			// ro: ray origin
			// rd: ray direction
			fixed4 raymarch(float3 ro, float3 rd) 
			{
				fixed4 ret = fixed4(0,0,0,0);

				const int maxstep = 64;
				float t = 0; // current distance traveled along ray
				for (int i = 0; i < maxstep; ++i) {
					float3 p = ro + rd * t; // World space position of sample
					float d = map(p);       // Sample of distance field (see map())

					// If the sample <= 0, we have hit something (see map()).
					if (d < 0.001) {
						// Simply return a gray color if we have hit an object
						// We will deal with lighting later.
						ret = fixed4(1, 1, 1, 1);
						break;
					}

					// If the sample > 0, we haven't hit anything yet so we should march forward
					// We step forward by distance d, because d is the minimum distance possible to intersect
					// an object (see map()).
					t += d;
				}

    			return ret;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float3 rayDirection = normalize(i.worldSpacePosition - _WorldSpaceCameraPos);

				// ray origin (camera position)
				float3 ro = _WorldSpaceCameraPos;

				// fixed3 col = tex2D(_MainTex,i.uv); // Color of the scene before this shader was run
				return gravityRayMarch(ro, rayDirection);
			}
			ENDCG
		}
	}
}
