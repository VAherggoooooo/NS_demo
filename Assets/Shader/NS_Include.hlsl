#ifndef NS_INCLUDE
#define NS_INCLUDE
#include "UnityCG.cginc"

#ifndef dx
#define dx 1.0
#endif

#ifndef dx2
#define dx2 (dx * dx)
#endif

#ifndef dt
#define dt unity_DeltaTime.z
#endif

#ifndef halfrdx
#define halfrdx (1 / dx * 0.5)
#endif


sampler2D _MainTex;

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
    float2 uvL : TEXCOORD1;
    float2 uvR : TEXCOORD2;
    float2 uvT : TEXCOORD3;
    float2 uvB : TEXCOORD4;
};

////////////////////////////////////////////////////////////////////////


v2f vert_common (appdata v)
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = v.texcoord;
    return o;
}
v2f vert_neighbor (appdata v)
{
    v2f o;
    o.pos = UnityObjectToClipPos(v.vertex);//TransformObjectToHClip(v.vertex.xyz) for URP
    o.uv = v.texcoord;

    float2 size = _Tex0_TexelSize.xy;
    o.uvL = o.uv - float2(1, 0) * size;
    o.uvR = o.uv + float2(1, 0) * size;
    o.uvT = o.uv + float2(0, 1) * size;
    o.uvB = o.uv - float2(0, 1) * size;
    return o;
}

////////////////////////////////////////////////////////////////////////

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
    float4 L = tex2D(_Tex0, i.uvL);
    float4 R = tex2D(_Tex0, i.uvR);
    float4 T = tex2D(_Tex0, i.uvT);
    float4 B = tex2D(_Tex0, i.uvB);

    float4 bC = tex2D(_Tex1, i.uv);
    float alpha = dx2 / (vscosity * dt);
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
    float4 L = tex2D(_Tex0, i.uvL);
    float4 R = tex2D(_Tex0, i.uvR);
    float4 T = tex2D(_Tex0, i.uvT);
    float4 B = tex2D(_Tex0, i.uvB);
    float4 C = tex2D(_Tex0, i.uv);

    //边界处理
    if(i.uvL.x <= 0) L = -C;
    if(i.uvR.x >= 1) R = -C;
    if(i.uvT.y >= 1) T = -C;
    if(i.uvB.y <= 0) B = -C;

    return halfrdx * (R.x - L.x + T.y - B.y);
}

//presure
float4 frag_presure (v2f i) : SV_Target
{
    float L = tex2D(_Tex0, i.uvL).x;
    float R = tex2D(_Tex0, i.uvR).x;
    float T = tex2D(_Tex0, i.uvT).x;
    float B = tex2D(_Tex0, i.uvB).x;

    float4 bC = tex2D(_Tex1, i.uv);
    float alpha = -dx2;
    float beta = 4;

    return (L + R + T + B + alpha * bC) / beta;
}

//gradient
float4 frag_gradient (v2f i) : SV_Target
{
    float L = tex2D(_Tex0, i.uvL).x;
    float R = tex2D(_Tex0, i.uvR).x;
    float T = tex2D(_Tex0, i.uvT).x;
    float B = tex2D(_Tex0, i.uvB).x;

    float4 bC = tex2D(_Tex1, i.uv);
    bC.xy -= halfrdx * float2(R - L, T - B);
    return bC;
}

#endif