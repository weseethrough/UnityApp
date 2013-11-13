
Shader "Custom/MapStencil" 
{
	Properties { 
		_MainTex ("Texture", any) = "" {} 
		_Rectangle ("Rectangle", Vector) = (0,0,0,0) 
		_Rotation ("Rotation", Float) = 0
 	} 

	SubShader {

		Tags { "ForceSupported" = "True" "RenderType"="Overlay" } 
		
		Lighting Off 
		Blend SrcAlpha OneMinusSrcAlpha 
		Cull Off 
		ZWrite Off 
		Fog { Mode Off } 
		ZTest Always 
		
		Pass {	
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma fragmentoption ARB_precision_hint_fastest

			#include "UnityCG.cginc"

			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
				float2 texcoordgen : TEXCOORD1;
			};

			sampler2D _MainTex;

			float4 _Rectangle;
			float _Rotation;

			uniform float4 _MainTex_ST;
			
			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
				o.color = v.color;
				// Rotation around origo
				v.texcoord.xy -=0.5;
				float s = -sin ( _Rotation );
				float c = cos ( _Rotation );
				float2x2 rotationMatrix = float2x2( c, -s, s, c);
				rotationMatrix *=0.5;
				rotationMatrix +=0.5;
				rotationMatrix = rotationMatrix * 2-1;
				v.texcoord.xy = mul ( v.texcoord.xy, rotationMatrix );				
				v.texcoord.xy +=0.5;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				// Index into atlas
				o.texcoordgen.x = _Rectangle.x + o.texcoord.x*_Rectangle.z;
				o.texcoordgen.y = _Rectangle.y + o.texcoord.y*_Rectangle.w;
				return o;
			}

			fixed4 frag (v2f i) : COLOR
			{				
				fixed4 o = 2.0f * tex2D(_MainTex, i.texcoordgen) * i.color;
				// Distance to center
				float d = distance(i.texcoord, float2(0.5, 0.5));
				if (d > 0.5) o.a = 0;
				else if (d > 0.47) o.a = 0.25;
				else o.a = 0.8;
				return o;
			}
			ENDCG 
		}
	} 	
 
	
	SubShader { 

		Tags { "ForceSupported" = "True" "RenderType"="Overlay" } 

		Lighting Off 
		Blend SrcAlpha OneMinusSrcAlpha 
		Cull Off 
		ZWrite Off 
		Fog { Mode Off } 
		ZTest Always 
		
		BindChannels { 
			Bind "vertex", vertex 
			Bind "color", color 
			Bind "TexCoord", texcoord 
		} 
		
		Pass { 
			SetTexture [_MainTex] {
				combine primary * texture DOUBLE, primary * texture DOUBLE
			} 
		} 
	} 

	Fallback off 
}
