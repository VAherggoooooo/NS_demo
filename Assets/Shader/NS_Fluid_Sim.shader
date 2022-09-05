Shader "NS_Fluid_Sim"
{
    SubShader
    {
        Pass//0
        {
            Name "Advect"
            HLSLPROGRAM
            #pragma vertex vert_common
            #pragma fragment frag_advect
            #include "NS_Include.hlsl"
            ENDHLSL
        }
        Pass//1
        {
            Name "Diffusion"
            HLSLPROGRAM
            #pragma vertex vert_common
            #pragma fragment frag_diffusion
            #include "NS_Include.hlsl"
            ENDHLSL
        }
        Pass//2
        {
            Name "Force"
            HLSLPROGRAM
            #pragma vertex vert_common
            #pragma fragment frag_force
            #include "NS_Include.hlsl"
            ENDHLSL
        }
        Pass//3
        {
            Name "Divergence"
            HLSLPROGRAM
            #pragma vertex vert_common
            #pragma fragment frag_divergence
            #include "NS_Include.hlsl"
            ENDHLSL
        }
        Pass//4
        {
            Name "Presure"
            HLSLPROGRAM
            #pragma vertex vert_common
            #pragma fragment frag_presure
            #include "NS_Include.hlsl"
            ENDHLSL
        }
        Pass//5
        {
            Name "Gradient"
            HLSLPROGRAM
            #pragma vertex vert_common
            #pragma fragment frag_gradient
            #include "NS_Include.hlsl"
            ENDHLSL
        }
    }
}

