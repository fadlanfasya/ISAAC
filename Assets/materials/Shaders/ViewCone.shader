Shader "Custom/ViewCone" {
	Properties {
		_TintColor ("Tint Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}		
		_Pulse ("Pulse (RGB)", 2D) = "white" {}		
	}
	SubShader {
		//Tags { "RenderType" = "Transparent" }
		
    	Tags { "Queue"="Transparent" "RenderType"="Additive"}
		
		CGPROGRAM	
		#include "UnityCG.cginc"	
		#pragma surface surf Unlit decal:add		

		sampler2D _MainTex;
		sampler2D _Pulse;
		uniform float4 _TintColor;

		struct Input {			
			float2 uv_MainTex;	
			float4 _Time;		
		};
		
		half4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
	    {
	         return half4(s.Albedo, s.Alpha);
	    }
		

		void surf (Input IN, inout SurfaceOutput o) {
			// Albedo comes from a texture tinted by color
			
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
			fixed4 p = tex2D (_Pulse, float2(IN.uv_MainTex.x - _Time.y * 0.4, _TintColor.a));
			
			c *= p;
			c *= _TintColor;
			
			float3 tint = _TintColor;			
			c.rgb = pow(c * 4, tint + 1);
			
			o.Albedo = c.rgb * IN.uv_MainTex.y;
			//o.Albedo = _TintColor.a;
			
			o.Alpha = c.a;
		}
		ENDCG
	} 
	FallBack "Unlit"
}
