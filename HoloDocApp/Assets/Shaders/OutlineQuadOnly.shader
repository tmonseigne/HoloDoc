Shader "Outlined/Quad" 
{
	Properties
	{
		_OutlineColor("Outline Color", Color) = (1,0,0,1)
		_OutlineWidth("Outline Width", Range(1, 2)) = 1.1
		_Centroid("Centroid", Vector) = (0,0,0,0)
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" }

		CGPROGRAM
		#pragma surface surf Lambert

		struct Input 
		{
			float2 uv_MainTex;
		};
		
		void surf(Input IN, inout SurfaceOutput o) {}
		ENDCG

		Pass
		{
			Blend One OneMinusDstColor // Soft Additive

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			uniform float4 _OutlineColor;
			uniform float  _OutlineWidth;
			uniform float4 _Centroid;

			struct appdata
			{
				float4 vertex : POSITION;
			};

			struct v2f {
				float4 pos : POSITION;
				float4 color : COLOR;
			};

			v2f vert(appdata v)
			{
				v2f o;
				// Translate each vertex depending on the centroid of the mesh.
				v.vertex = _Centroid + _OutlineWidth * (v.vertex - _Centroid) + float4(0, 0, 0.001, 1);
				o.pos = UnityObjectToClipPos(v.vertex);
				o.color = _OutlineColor;
				return o;
			}

			half4 frag(v2f i) : COLOR 
			{
				return i.color;
			}
			ENDCG
		}
	}
	Fallback "Standard"
}
