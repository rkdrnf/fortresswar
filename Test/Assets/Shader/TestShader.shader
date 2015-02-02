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
			Tags { "LightMode" = "ForwardBase" } 

			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }

			Blend SrcAlpha OneMinusSrcAlpha

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
				half2 lightPower : TEXCOORD1;
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
				return float4(_Color.r, _Color.g, _Color.b, min(alphaSum / 8, 1 - attenuation));
			}
			ENDCG
		}


		Pass 
		{
			Tags { "LightMode" = "ForwardAdd" } 

			Cull Off
			Lighting Off
			ZWrite Off
			Fog { Mode Off }

			Blend One One

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
				half2 lightPower : TEXCOORD1;
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
				   attenuation = 1.0; // no attenuation
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
				return float4(_Color.r, _Color.g, _Color.b, min(alphaSum / 8, 1 - attenuation));
			}
			ENDCG
		}

		
	} 
	//FallBack "Diffuse"
}
