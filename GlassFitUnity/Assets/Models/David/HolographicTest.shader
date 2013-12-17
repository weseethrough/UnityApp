Shader "Custom/HolographicTest" {
Properties {
	_rimFactor ( "Rim glow spread factor", Float) = 0.75
	_baseGlow ("Base brightness", Float) = 0.2
	_color ("Colourise", Color) = (1,1,1,1)
	_expandDistance("Expand Mesh min distance", Float) = 2000
}
  SubShader {
    Pass {
      CGPROGRAM
// Upgrade NOTE: excluded shader from DX11 and Xbox360; has structs without semantics (struct v2f members glowFactor)
#pragma exclude_renderers d3d11 xbox360
      #pragma vertex vert
      #pragma fragment frag
      #include "UnityCG.cginc"

      struct v2f {
      	  float glowFactor;
          float4 pos : SV_POSITION;
          fixed4 color : COLOR;
      };

	float _rimFactor;
	float _baseGlow;
	float4 _color;
	float _expandDistance;

      v2f vert (appdata_base v)
      {
          v2f o;
          
          o.pos = mul (UNITY_MATRIX_MVP, v.vertex);
          
          float3 camera = float3(0, 0, 1.0);
          
          float3 normal_screenspace = mul(UNITY_MATRIX_IT_MV, float4(v.normal,0.0));
          float dotp = dot(camera, normal_screenspace.xyz);
          
          float transparency = 1 - pow(dotp, _rimFactor) * float3(1.0,1.0,1.0);
          
          o.color.xyz = min(transparency + _baseGlow, float3(1,1,1));
          //o.color.xyz = normal_screenspace;
          o.color.w = transparency;
          o.color *= _color;
          
          //shift the vert out a bit if he's far away
          float distance = o.pos.z;
          
          
          
          //TODO Friday, make this progress smoothly from normal to expanded, and clamp expansion?
          
          if (distance > _expandDistance)
          {
          		float fExpansion = (distance - _expandDistance) * 0.001f;
          		//phase in the expansion gradually over 1/10th of the expand threshold
          		float fExpandRange = _expandDistance * 0.5;
          		float fExpandFactor = distance / fExpandRange;
          		
          		o.pos.xy += fExpansion * fExpandFactor * normal_screenspace.xy;
          }
          
          return o;
      }

      fixed4 frag (v2f i) : COLOR0 { return i.color; }
      ENDCG
    }
  } 
}