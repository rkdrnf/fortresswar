Shader "Custom/TestShader" {
	Properties {
		_Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_MainTex ("Shadow Texture", 2D) = "white" {}
		_BoundX ("BoundX", Float) = 0
		_BoundY ("BoundY", Float) = 0
	}
	SubShader 
	{
		Tags {"Queue" = "Transparent" 
			"RenderType" = "Transparent" 
			"IgnoreProjector" = "True" 
			"ForceNoShadowCasting" = "True"
			}
		
		ZWrite Off
		AlphaTest Greater 0.01

		Pass 
		{
			Tags { "LightMode" = "ForwardAdd" } 

			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }

			Blend Zero One, One Zero
			AlphaTest Greater 0.01

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform float4 _Color;
			uniform half _BoundX;
			uniform half _BoundY;

			sampler2D _MainTex;

			uniform float4 _LightColor0;
			uniform float4x4 _LightMatrix0; // transformation 
            // from world to light space (from Autolight.cginc)
	        uniform samplerCUBE _LightTexture0; 
            // cookie alpha texture map (from Autolight.cginc)

			struct vertexInput 
			{
				float4 vertex: POSITION;
				float3 normal: NORMAL;
				float4 texcoord0: TEXCOORD0;
			};

			struct vertexOutput
			{
				float4 worldPos : TEXCOORD2;
				float4 position : SV_POSITION;
				half2 texcoord : TEXCOORD0;
				float4 col : COLOR;
				float4 posLight: TEXCOORD1;
			};

			vertexOutput vert (vertexInput IN)
			{
				vertexOutput o;

				vertexOutput output;
 
				float3 normalDirection = normalize(
				   mul(float4(IN.normal, 0.0), _World2Object).xyz);

				float4 worldVertex = mul(_Object2World, IN.vertex);
				float3 vertexToLightSource = _WorldSpaceLightPos0.xyz - worldVertex.xyz;

				
				o.posLight = mul(_LightMatrix0, worldVertex);
				o.worldPos = worldVertex;
				o.col = float4(vertexToLightSource.xyz, 0);
				o.position = mul(UNITY_MATRIX_MVP, IN.vertex);
				o.texcoord = IN.texcoord0;
				
				return o;
			}

			float4 frag(vertexOutput IN) : SV_Target
			{
				float3 vertexToLightSource;

				float distance = length(IN.posLight.xy);
                  // use z coordinate in light space as signed distance

				float attenuation = texCUBE(_LightTexture0, 
                  IN.posLight.xyz).a;

				return float4(0, 0, 0, pow(distance, 2) * (1 + 1 / length(_LightColor0)));
			}
			ENDCG
		}

		Pass 
		{
			Tags { "LightMode" = "ForwardBase" } 

			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }

			Blend Zero One, SrcAlpha DstAlpha
			BlendOp Min

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			uniform float4 _Color;
			uniform half _BoundX;
			uniform half _BoundY;

			sampler2D _MainTex;

			struct vertexInput 
			{
				float4 vertex: POSITION;
				float4 texcoord0: TEXCOORD0;
			};

			struct vertexOutput
			{
				float4 position: SV_POSITION;
				half2 texcoord : TEXCOORD0;
			};

			vertexOutput vert (vertexInput IN)
			{
				vertexOutput o;

				o.position = mul(UNITY_MATRIX_MVP, IN.vertex);
				o.texcoord = IN.texcoord0;

				return o;
			}

			float4 frag(vertexOutput IN) : SV_Target
			{
				float attenuation;

				if (0.0 == _WorldSpaceLightPos0.w) // directional light?
				{
				   attenuation = 0.0; // no attenuation
				} 
				else // point or spot light
				{
					float3 vertexToLightSource = _WorldSpaceLightPos0.xyz
				      - mul(_Object2World, IN.position).xyz;
					float distance = length(vertexToLightSource);
					attenuation = 1.0 / distance; // linear attenuation 
				}
				

				float alphaSum = 0.0f;
				
				alphaSum += min(tex2D(_MainTex, IN.texcoord + half2(0, _BoundY)).a, tex2D(_MainTex, IN.texcoord + half2(0, _BoundY * 2)).a);
				alphaSum += tex2D(_MainTex, IN.texcoord + half2(_BoundX, _BoundY)).a;
				alphaSum += min(tex2D(_MainTex, IN.texcoord + half2(_BoundX, 0)).a, tex2D(_MainTex, IN.texcoord + half2(_BoundX * 2, 0)).a);
				alphaSum += tex2D(_MainTex, IN.texcoord + half2(_BoundX, -_BoundY)).a;
				alphaSum += min(tex2D(_MainTex, IN.texcoord + half2(0, -_BoundY)).a, tex2D(_MainTex, IN.texcoord + half2(0, -_BoundY * 2)).a);
				alphaSum += tex2D(_MainTex, IN.texcoord + half2(-_BoundX, -_BoundY)).a;
				alphaSum += min(tex2D(_MainTex, IN.texcoord + half2(-_BoundX, 0)).a, tex2D(_MainTex, IN.texcoord + half2(-_BoundX * 2, 0)).a);
				alphaSum += tex2D(_MainTex, IN.texcoord + half2(-_BoundX, _BoundY)).a;
				alphaSum *= tex2D(_MainTex, IN.texcoord).a;


				half2 tendency = half2(fmod(IN.texcoord.x, 8) - 3.5, fmod(IN.texcoord.y, 8) - 3.5);
				return float4(1, 1, 1, min(alphaSum / 8, 1 - attenuation));
			}
			ENDCG
		}

		Pass 
		{
			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }

			Blend DstAlpha OneMinusDstAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			uniform float4 _Color;
			uniform half _BoundX;
			uniform half _BoundY;

			sampler2D _MainTex;

			struct vertexInput 
			{
				float4 vertex: POSITION;
				float4 texcoord0: TEXCOORD0;
			};

			struct vertexOutput
			{
				float4 position: SV_POSITION;
				half2 texcoord : TEXCOORD0;
			};

			vertexOutput vert (vertexInput IN)
			{
				vertexOutput o;

				o.position = mul(UNITY_MATRIX_MVP, IN.vertex);
				o.texcoord = IN.texcoord0;

				return o;
			}

			float4 frag(vertexOutput IN) : SV_Target
			{
				return float4(_Color.r, _Color.g, _Color.b, 1);
			}
			ENDCG
		}


		
		
	} 
	//FallBack "Diffuse"
}
