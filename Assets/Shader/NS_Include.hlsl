#ifndef NS_INCLUDE
#define NS_INCLUDE
#include "UnityCG.cginc"

#ifndef dx
#define dx 1.0
#endif

#ifndef dt
#define dt unity_DeltaTime.z
#endif

sampler2D _MainTex;
float4 _MainTex_ST;

float2 inputPos;
float2 forceVec;
float radius;
float advectSpeed;
float vscosity;

sampler2D _FinalCTex;
sampler2D _Tex0, _Tex1;
float4 _Tex0_TexelSize, _Tex1_TexelSize;



struct appdata
{
    float4 vertex : POSITION;
    float2 texcoord : TEXCOORD0;
};
struct v2f
{
    float2 uv : TEXCOORD0;
    float4 pos : SV_POSITION;
};

v2f vert_common (appdata v)
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = v.texcoord;
    return o;
}

//advect
float4 frag_advect (v2f i) : SV_Target
{
    float2 vel = tex2D(_Tex0, i.uv).xy;
    float2 newUV = i.uv - vel * dt * dx * advectSpeed * 0.2;
    return tex2D(_Tex1, newUV);
}

//diffusion
float4 frag_diffusion (v2f i) : SV_Target
{
    float2 size = _Tex0_TexelSize.xy;
    float4 L = tex2D(_Tex0, i.uv - float2(1, 0) * size);
    float4 R = tex2D(_Tex0, i.uv + float2(1, 0) * size);
    float4 T = tex2D(_Tex0, i.uv + float2(0, 1) * size);
    float4 B = tex2D(_Tex0, i.uv - float2(0, 1) * size);

    float4 bC = tex2D(_Tex1, i.uv);
    float alpha = dx * dx / (vscosity * dt);
    float beta = 4 + alpha;

    return (L + R + T + B + alpha * bC) / beta;
}

//force
float4 frag_force (v2f i) : SV_Target
{
    float2 vel = tex2D(_Tex0, i.uv).xy;
    float2 pos = inputPos - i.uv;
    vel += forceVec * exp(-dot(pos, pos) / (radius * 0.001)) * dt * 200;
    return float4(vel, 0, 1);
}

//divergence
float4 frag_divergence (v2f i) : SV_Target
{
    float2 size = _Tex0_TexelSize.xy;
    float2 uvL = i.uv - float2(1, 0) * size;
    float2 uvR = i.uv + float2(1, 0) * size;
    float2 uvT = i.uv + float2(0, 1) * size;
    float2 uvB = i.uv - float2(0, 1) * size;
    float4 L = tex2D(_Tex0, uvL);
    float4 R = tex2D(_Tex0, uvR);
    float4 T = tex2D(_Tex0, uvT);
    float4 B = tex2D(_Tex0, uvB);
    float4 C = tex2D(_Tex0, i.uv);

    
    if(uvL.x <= 0) L = -C;
    if(uvR.x >= 1) R = -C;
    if(uvT.y >= 1) T = -C;
    if(uvB.y <= 0) B = -C;

    float halfrdx = 1 / dx * 0.5;
    return halfrdx * (R.x - L.x + T.y - B.y);
}

//presure
float4 frag_presure (v2f i) : SV_Target
{
    float2 size = _Tex0_TexelSize.xy;
    float2 uvL = i.uv - float2(1, 0) * size;
    float2 uvR = i.uv + float2(1, 0) * size;
    float2 uvT = i.uv + float2(0, 1) * size;
    float2 uvB = i.uv - float2(0, 1) * size;
    float L = tex2D(_Tex0, uvL).x;
    float R = tex2D(_Tex0, uvR).x;
    float T = tex2D(_Tex0, uvT).x;
    float B = tex2D(_Tex0, uvB).x;

    float4 bC = tex2D(_Tex1, i.uv);
    float alpha = -dx * dx;
    float beta = 4;

    return (L + R + T + B + alpha * bC) / beta;
}

//gradient
float4 frag_gradient (v2f i) : SV_Target
{
    float2 size = _Tex0_TexelSize.xy;
    float2 uvL = i.uv - float2(1, 0) * size;
    float2 uvR = i.uv + float2(1, 0) * size;
    float2 uvT = i.uv + float2(0, 1) * size;
    float2 uvB = i.uv - float2(0, 1) * size;
    float L = tex2D(_Tex0, uvL).x;
    float R = tex2D(_Tex0, uvR).x;
    float T = tex2D(_Tex0, uvT).x;
    float B = tex2D(_Tex0, uvB).x;

    float4 bC = tex2D(_Tex1, i.uv);
    
    float halfrdx = 1 / dx * 0.5;
    bC.xy -= halfrdx * float2(R - L, T - B);
    return bC;
}

#endif