Shader "Custom/VirtualTrack" {
	Properties {
		_Repeats ("repeats", Float) = 120.0
		_Phase ( "scroll phase", Float) = 0.0
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_FadeDist( "fade start distance", Float) = 2000
		_FadeRange( "fade over range", Float) = 1000
	}
	SubShader {
	Pass {
		Tags { "RenderType"="Opaque" }
		LOD 200
		
		CGPROGRAM
		    #pragma vertex vert
	   		#pragma fragment frag
	      	#include "UnityCG.cginc"

		

		sampler2D _MainTex;
		float _Repeats;
		float _Phase;
		float _FadeDist;
		float _FadeRange;

      	struct v2f {
      		float4 pos : SV_POSITION;
			float2 uv : TEXCOORD0;
			float4 col : COLOR;
      	};

		v2f vert (appdata_base v)
    	{
      		v2f o;
          	o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
			o.uv = v.texcoord;
			o.col = float4(1.0,1.0,1.0,1.0);
			//apply the scroll factor - uv comes in as [0,1]
			o.uv.y = -o.uv.y * _Repeats + _Phase;
		
//			if(o.pos.z > _FadeDist)
//			{
//				float DistIntoFade = o.pos.z - _FadeDist;
//				float Fade = clamp(DistIntoFade/_FadeRange, 0.0, 1.0);
//				o.col.b = Fade;
//				o.col.a = 1-Fade;
//				//o.col.r = o.pos.z / (_FadeDist * 50);
//			}
			
      		return o;
      	}
      	
      	
      	
      
      	fixed4 frag (v2f i) : COLOR0 { 
      		float4 texCol = tex2D( _MainTex, i.uv);
      		if(texCol.a < 0.05)
      		{
      			discard;
      		}
      		return i.col * texCol;
//      		if(i.pos.z > _FadeDist)
//      		{
//      			//fade out over 1000m
//      			float Fade = min( (i.pos.z - _FadeDist)/_FadeRange, 1.0);
//      			Fade = 1 - Fade;
//      		}
      	}

		ENDCG
	}
	}
	FallBack "Diffuse"
}
