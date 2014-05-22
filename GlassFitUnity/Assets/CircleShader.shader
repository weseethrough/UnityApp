Shader "CircleShader" {
	Properties
	{
		_MainTex ("Base (RGB), Alpha (A)", 2D) = "white" {}
		_AlphaTex ("MaskTexture", 2D) = "white" {}
	}
 
	SubShader
	{
		LOD 100
 
		Tags
		{
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
			"RenderType" = "Transparent"
		}
 
		Cull Off
		Lighting Off
		ZWrite Off
		Fog { Mode Off }
		Offset -1, -1
		Blend SrcAlpha OneMinusSrcAlpha
 
		Pass
		{
			CGPROGRAM
				#pragma vertex vertexProgram
				#pragma fragment fragmentProgram
 
				#include "UnityCG.cginc"
 
				struct appdata_t
				{
					float4 vertex : POSITION;
					float2 textureCoordinate : TEXCOORD0;
					fixed4 color : COLOR;
				};
 
				struct vertexToFragment
				{
					float4 vertex : SV_POSITION;
					half2 textureCoordinate : TEXCOORD0;
					fixed4 color : COLOR;
				};
 
				sampler2D _MainTex;
				float4 _MainTex_ST;
				sampler2D _AlphaTex;
 
				vertexToFragment vertexProgram (appdata_t vertexData)
				{
					vertexToFragment output;
					output.vertex = mul(UNITY_MATRIX_MVP, vertexData.vertex);
					output.textureCoordinate = TRANSFORM_TEX(vertexData.textureCoordinate, _MainTex);
					output.color = vertexData.color;
					return output;
				}
 
				fixed4 fragmentProgram (vertexToFragment input) : COLOR
				{
					fixed4 computedColor = tex2D(_MainTex, input.textureCoordinate) * input.color;
					fixed4 alphaGuide = tex2D(_AlphaTex, input.textureCoordinate);
 
					if (alphaGuide.a < computedColor.a) computedColor.a = alphaGuide.a;
 
					return computedColor;
				}
			ENDCG
		}
	}
}

