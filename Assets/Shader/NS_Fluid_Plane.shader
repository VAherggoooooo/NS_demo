Shader "NS_Fluid_Plane"
{
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "NS_Include.hlsl"    

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.texcoord;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_FinalCTex, i.uv);
                return col;
            }
            ENDHLSL
        }
    }
}
