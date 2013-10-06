Shader "Diffuse Rim" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,1)
	_MainTex ("Base (RGB)", 2D) = "white" {}
	_RimPower ("Rim Power", Range(0.5,8.0)) = 3.0
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 200

CGPROGRAM
#pragma surface surf Lambert

sampler2D _MainTex;
fixed4 _Color;
float _RimPower;

struct Input {
	float2 uv_MainTex;
	float3 viewDir;
};

void surf (Input IN, inout SurfaceOutput o) {
	fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
	o.Albedo = c.rgb;
	o.Alpha = c.a;
	half rim = 1.0 - saturate(dot (normalize(IN.viewDir), o.Normal));
	o.Emission = c.rgb * pow (rim, _RimPower) * _Color * 2.0;
}
ENDCG
}

Fallback "VertexLit"
}
