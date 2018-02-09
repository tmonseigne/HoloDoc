Shader "Outlined/Quad" {
	Properties{
		_OutlineColor("Outline Color", Color) = (0,0,0,1)
		_OutlineWidth("Outline Width", Range(0, 1)) = .1
	}

	CGINCLUDE
	#include "UnityCG.cginc"

	struct appdata {
	float4 vertex : POSITION;
	float3 normal : NORMAL;
	};

	struct v2f {
		float4 pos : POSITION;
		float4 color : COLOR;
	};

	uniform float _OutlineWidth;
	uniform float4 _OutlineColor;
	uniform float4 _centroid;

	v2f vert(appdata v) {
		v2f o;
		// Translate each vertex depending on the centroid of the mesh.
		v.vertex = (1 + _OutlineWidth) * (v.vertex - _centroid) + _centroid + float4(0, 0, 0.01, 1);
		o.pos = UnityObjectToClipPos(v.vertex);
		o.color = _OutlineColor;
		return o;
	}
	ENDCG

	SubShader {
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" }
		Cull Back
		Blend Zero One

		CGPROGRAM
		#pragma surface surf Lambert
		struct Input {
			float2 uv_MainTex;
		};
		void surf(Input IN, inout SurfaceOutput o) {}
		ENDCG


		Pass {
			Name "OUTLINE"
			Cull Back

			Blend One OneMinusDstColor // Soft Additive

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			half4 frag(v2f i) : COLOR{
				return i.color;
			}
		ENDCG
		}
	}
	Fallback "Diffuse"
}