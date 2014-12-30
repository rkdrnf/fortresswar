Shader "Custom/Glow"
{
	Properties
	{
		[PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
		_Color ("Tint", Color) = (1,1,1,1)
		[MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
		_GlowColor ("Glow Color", Color) = (1, 0, 0, 1)
		_Bound ("Bound", Float) = 0
	}

	SubShader
	{
		Tags
		{ 
			"Queue"="Transparent" 
			"IgnoreProjector"="True" 
			"RenderType"="Transparent" 
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		Blend One OneMinusSrcAlpha

		Pass
		{
		CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile DUMMY PIXELSNAP_ON
			#include "UnityCG.cginc"
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
			};
			
			fixed4 _Color;


			v2f vert(appdata_t IN)
			{
				v2f OUT;
				OUT.vertex = mul(UNITY_MATRIX_MVP, IN.vertex);
				OUT.texcoord = IN.texcoord;
				OUT.color = IN.color * _Color;
				#ifdef PIXELSNAP_ON
				OUT.vertex = UnityPixelSnap (OUT.vertex);
				#endif

				return OUT;
			}

			sampler2D _MainTex;
			float _Bound;
			fixed4 _GlowColor;

			fixed4 frag(v2f IN) : SV_Target
			{
				fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;
				c.rgb *= c.a;

				if(c.a == 0)
				{
					float alphaSum = 0.0f;
					alphaSum += tex2D(_MainTex, IN.texcoord + half2(-_Bound, -_Bound)).a;
					alphaSum += tex2D(_MainTex, IN.texcoord + half2(-_Bound, 0.0f)).a;
					alphaSum += tex2D(_MainTex, IN.texcoord + half2(-_Bound, _Bound)).a;
					alphaSum += tex2D(_MainTex, IN.texcoord + half2(0.0f, -_Bound)).a;
					alphaSum += tex2D(_MainTex, IN.texcoord + half2(0.0f, _Bound)).a;
					alphaSum += tex2D(_MainTex, IN.texcoord + half2(_Bound, -_Bound)).a;
					alphaSum += tex2D(_MainTex, IN.texcoord + half2(_Bound, 0.0f)).a;
					alphaSum += tex2D(_MainTex, IN.texcoord + half2(_Bound, _Bound)).a;

					if(alphaSum > 0.0f)
					{
						c = _GlowColor;
					}
				}

				return c;
			}
		ENDCG
		}
	}
}
