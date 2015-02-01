Shader "Custom/TestShader" {
	Properties {
		_Color ("Color", Color) = (1.0, 1.0, 1.0, 1.0)
		_MainTex ("Shadow Texture", 2D) = "white" {}
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
		
		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }

		Blend SrcAlpha OneMinusSrcAlpha

		Pass 
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			uniform float4 _Color;

			sampler2D _MainTex;

			struct vertexInput 
			{
				float4 position: POSITION;
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

				o.position = mul(UNITY_MATRIX_MVP, IN.position);
				o.texcoord = IN.texcoord0;

				return o;
			}

			float4 frag(vertexOutput IN) : SV_Target
			{
				return float4(0, 0, 0, tex2D(_MainTex, IN.texcoord).a);
			}
			ENDCG
		}
	} 
	//FallBack "Diffuse"
}
