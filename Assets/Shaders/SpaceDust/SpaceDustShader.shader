// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'
// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/SpaceDustShader"
{
	//Derek Edrich, 2016

	//the alpha channel of the input color controls the size of the actual dust particle relative to the out of focus circle. It also serves as a multiplier to the circle's alpha.
	Properties
	{
		_MainTex("Main (Halo) Texture (Greyscale)", 2D) = "gray" {}
		_DustTex("Hard Dust Texture", 2D) = "gray" {}
		_HaloDist("Distance to show a halo, should be less than _DustDist", Float) = 3
		_DustDist("Distance to show a hard dust texture", Float) = 2
		_HaloAlpha("HaloAlphaMultiplier", Range(0, 1)) = 0.25
		_DustStartSize("DustStartSize", Range(0, 1)) = 0.7
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "IgnoreProjector"="True" "Queue"="Transparent"  "PreviewType"="Plane"}
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Lighting Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float4 color : COLOR;
			};

			struct v2f
			{
				half4 color : COLOR;
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float dist : TEXCOORD1;
			};

			sampler2D _MainTex;
			sampler2D _DustTex;
			uniform float _HaloDist;
			uniform float _DustDist;
			uniform fixed _HaloAlpha;
			uniform fixed _DustStartSize;

			inline float inverseLerp(float min, float max, float value)
			{
				//inverse of lerp
				return (value - min) / (max - min);
			}

			inline float inverseEaseInExpo(float min, float max, float value)
			{
				//inverse of lerp
				float lerpVal = inverseLerp(min, max, value);
				return pow(2, -10 * lerpVal);
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);

				//get distance from camera
				//particles should be small & billboarded, and clip planes are a thing, so the true spherical values vs the linear interpolation shouldn't be an issue
				o.dist = distance(_WorldSpaceCameraPos, mul(unity_ObjectToWorld, v.vertex));

				o.uv = v.uv;
				o.color = v.color;

				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float4 haloColor = i.color * tex2D(_MainTex, i.uv);
				haloColor.a *= _HaloAlpha;

				//fade out mechanism for dust particles
				//the dust texture shrinks by mapping to smaller UV coords instead of becoming more transparent
				
				float2 shrinkedUV = i.uv;// +(((0.5 - i.uv) * (i.color.a)));

				/*float2 shrinkedUV = i.uv - 0.5;
				shrinkedUV /= i.color.a;
				shrinkedUV += 0.5;*/

				float4 dustColor = tex2D(_DustTex, shrinkedUV);
				dustColor.rgb *= i.color.rgb;

				// Display if within _HaloDist and _DustDist
				if(i.dist >= _HaloDist && i.dist <= _DustDist)
				{
					float lerpValue = inverseLerp(_HaloDist, _DustDist, i.dist);
					lerpValue = (2 * lerpValue) - 1;
					float haloAlpha = 1 - saturate(-lerpValue);
					haloAlpha *= haloAlpha;
					haloColor.a *= haloAlpha;
					//haloColor.a *= (1 - lerpValue) * _HaloAlpha;
					return lerp(haloColor, dustColor, saturate(lerpValue));
				}
				return float4(0, 0, 0, 0);
			}
			ENDCG
		}
	}
}