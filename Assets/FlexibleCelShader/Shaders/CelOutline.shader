Shader "FlexibleCelShader/Cel Outline"
{
	Properties
	{
		_Color("Global Color Modifier", Color) = (1, 1, 1, 1)
		_MainTex("Texture", 2D) = "white" {}
		_NormalTex("Normal", 2D) = "bump" {}
		_EmmisTex("Emission", 2D) = "black" {}

		_RampLevels("Ramp Levels", Range(2, 50)) = 2
		_LightScalar("Light Scalar", Range(0, 10)) = 1

		_HighColor("High Light Color", Color) = (1, 1, 1, 1)
		_HighIntensity("High Light Intensity", Range(0, 10)) = 1.5

		_LowColor("Low Light Color", Color) = (1, 1, 1, 1)
		_LowIntensity("Low Light Intensity", Range(0, 10)) = 1

		_OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
		_OutlineSize("Outline Size", float) = 10

		_RimColor("Hard Edge Light Color", Color) = (1, 1, 1, 1)
		_RimAlpha("Hard Edge Light Brightness", Range(0, 1)) = 0
		_RimPower("Hard Edge Light Size", Range(0,1)) = 0
		_RimDropOff("Hard Edge Light Dropoff", range(0, 1)) = 0

		_FresnelColor("Soft Edge Light Color", Color) = (1,1,1,1)
		_FresnelBrightness("Soft Edge Light Brightness", Range(0, 1)) = 0
		_FresnelPower("Soft Edge Light Size", Range(0, 1)) = 0
		_FresnelShadowDropoff("Soft Edge Light Dropoff", range(0, 1)) = 0
	}
	
	SubShader
	{
		
		// This pass renders the object
		Cull back
		Pass
		{
			Tags{ "LightMode" = "ForwardBase" }

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "Lighting.cginc"

			#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			#include "AutoLight.cginc"

			struct v2f
			{
				float2 uv : TEXCOORD0;
				SHADOW_COORDS(1)
				float3 worldNormal : TEXCOORD2;
				float3 worldTangent : TEXCOORD3;
				float3 worldBitangent : TEXCOORD4;
				float4 worldPos : TEXCOORD5;
				float4 pos : SV_POSITION;
			};

			v2f vert(appdata_tan v)
			{
				v2f o;

				// UV data
				o.uv = v.texcoord;

				// Position data
				o.worldPos = mul(unity_ObjectToWorld, v.vertex);
				o.pos = mul(UNITY_MATRIX_VP, o.worldPos);

				// Normal data
				o.worldNormal = UnityObjectToWorldNormal(v.normal);
				o.worldTangent = UnityObjectToWorldNormal(v.tangent);
				o.worldBitangent = cross(o.worldTangent, o.worldNormal);

				// Compute shadows data
				TRANSFER_SHADOW(o);

				return o;
			}

			float4    _Color;
			sampler2D _MainTex;
			uniform float4 _MainTex_ST;
			sampler2D _NormalTex;
			uniform float4 _NormalTex_ST;
			sampler2D _EmmisTex;
			uniform float4 _EmmisTex_ST;
			int       _RampLevels;
			float     _LightScalar;
			float     _HighIntensity;
			float4    _HighColor;
			float     _LowIntensity;
			float4    _LowColor;
			float     _RimPower;
			float	  _RimAlpha;
			float4    _RimColor;
			float     _RimDropOff;
			float     _FresnelBrightness;
			float     _FresnelPower;
			float4    _FresnelColor;
			float     _FresnelShadowDropoff;


			float4 hash4( float2 p ) { 
                return frac(sin(float4(    
                    1.0+dot(p,float2(37.0,17.0)), 
                    2.0+dot(p,float2(11.0,47.0)),
                    3.0+dot(p,float2(41.0,29.0)),
                    4.0+dot(p,float2(23.0,31.0))))
                *103.0); 
            }

            float4 textureNoTile( sampler2D samp, float2 uv )
            {
                float2 iuv = int2( floor( uv ) );
                float2 fuv = frac( uv );

                // generate per-tile transform
                
                float4 ofa = hash4(iuv + float2(0,0));
                float4 ofb = hash4(iuv + float2(1,0));
                float4 ofc = hash4(iuv + float2(0,1));
                float4 ofd = hash4(iuv + float2(1,1));

                float2 derdx = ddx( uv );
                float2 derdy = ddy( uv );

                // transform per-tile uvs
                ofa.zw = sign( ofa.zw-0.5 );
                ofb.zw = sign( ofb.zw-0.5 );
                ofc.zw = sign( ofc.zw-0.5 );
                ofd.zw = sign( ofd.zw-0.5 );
                
                // uv's, and derivatives (for correct mipmapping)
                float2 uva = uv*ofa.zw + ofa.xy, ddxa = derdx*ofa.zw, ddya = derdy*ofa.zw;
                float2 uvb = uv*ofb.zw + ofb.xy, ddxb = derdx*ofb.zw, ddyb = derdy*ofb.zw;
                float2 uvc = uv*ofc.zw + ofc.xy, ddxc = derdx*ofc.zw, ddyc = derdy*ofc.zw;
                float2 uvd = uv*ofd.zw + ofd.xy, ddxd = derdx*ofd.zw, ddyd = derdy*ofd.zw;
                    
                // fetch and blend
                float2 b = smoothstep( 0.25,0.75, fuv );

                return lerp( lerp( tex2Dgrad( samp, uva, ddxa, ddya ), 
                                tex2Dgrad( samp, uvb, ddxb, ddyb ), b.x ), 
                            lerp( tex2Dgrad( samp, uvc, ddxc, ddyc ),
                                tex2Dgrad( samp, uvd, ddxd, ddyd ), b.x), b.y );
            }
			
			fixed4 frag(v2f i) : SV_Target
			{
				_RampLevels -= 1;

				// Get view direction && light direction for rim lighting
				float3 viewDirection = normalize(_WorldSpaceCameraPos.xyz - i.worldPos.xyz);
				float3 lightDirection = normalize(_WorldSpaceLightPos0.xyz);

				// Sample textures
				fixed4 col = textureNoTile(_MainTex, i.uv);
				fixed3 tangentNormal = tex2D(_NormalTex, i.uv * _NormalTex_ST.xy + _NormalTex_ST.zw) * 2 - 1;
				fixed4 emmision = tex2D(_EmmisTex, i.uv * _EmmisTex_ST.xy + _EmmisTex_ST.zw);

				// Get normal
				float3 worldNormal = float3(i.worldTangent * tangentNormal.r + i.worldBitangent * tangentNormal.g + i.worldNormal * tangentNormal.b);

				// Rim Lighting
				half factor = dot(viewDirection, worldNormal);
				half fresnelFactor = 1 - min(pow(max(1 - factor, 0), (1 - _FresnelPower) * 10), 1);

				// Get shadow attenuation
				fixed shadow = SHADOW_ATTENUATION(i);

				// Calculate light intensity
				float intensity = dot(worldNormal, lightDirection);
				intensity = clamp(intensity * _LightScalar, 0, 1);

				// Factor in the shadow
				intensity *= shadow;

				// Determine level
				float rampLevel = round(intensity * _RampLevels);

				// Get light multiplier based on level
				float lightMultiplier = _LowIntensity + ((_HighIntensity - _LowIntensity) / (_RampLevels)) * rampLevel;

				// Get color multiplier based on level
				float4 highColor = (rampLevel / _RampLevels) * _HighColor;
				float4 lowColor = ((_RampLevels - rampLevel) / _RampLevels) * _LowColor;
				float4 mixColor = (highColor + lowColor) / 2;

				// Apply light multiplier and color
				col *= lightMultiplier;
				col *= _Color * mixColor;

				// Apply soft Fresnel
				float rampPercentSoftFresnel = 1 - ((1 - rampLevel / _RampLevels) * (1 - _FresnelShadowDropoff));
				col.rgb = col.rgb + _FresnelColor * (_FresnelBrightness * 10 - fresnelFactor * _FresnelBrightness * 10) * rampPercentSoftFresnel;

				// Apply hard rim lighting
				_RimAlpha *= 1 - ((1 - rampLevel / _RampLevels) * (1 - _RimDropOff));
				if (factor <= _RimPower) {
					col.rgb = _RimColor.rgb * _RimAlpha + col.rgb * (1 - _RimAlpha);
				}

				// Apply emmision lighting
				half eIntensity = max(emmision.r, emmision.g);
				eIntensity = max(eIntensity, emmision.b);
				col = emmision * eIntensity + col * (1 - eIntensity);

				return col;
			}

			ENDCG
		} // End Main Pass

		// This Pass Renders the outlines
		Cull Front
		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
			};

			float _OutlineSize;
			v2f vert(appdata v)
			{
				v2f o;
				float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
				half3 worldNormal = UnityObjectToWorldNormal(v.normal);
				worldPos.xyz = worldPos.xyz + worldNormal * _OutlineSize * 0.001;
				o.vertex = mul(UNITY_MATRIX_VP, worldPos);
				return o;
			}

			float4 _OutlineColor;
			fixed4 frag(v2f i) : SV_Target
			{
				return _OutlineColor;
			}
				ENDCG
		}// End Outline Pass

		// Shadow casting
		UsePass "Legacy Shaders/VertexLit/SHADOWCASTER"
	}

	CustomEditor "CelCustomEditor"
}