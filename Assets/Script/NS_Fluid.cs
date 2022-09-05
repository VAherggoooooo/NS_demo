using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NS_Fluid : MonoBehaviour
{       
   public Texture2D oriTex;
    public Texture2D velTex;
    public Material simMat;
    [Range(0, 55)]public int diffusionTimes = 5;
    public float forceScale = 1.0f;
    public float radius = 1.0f;
    public float advectSpeed = 1.0f;
    public float vscosity = 1.0f;
    private Camera _camera;
    private Vector3 lastPos;
    private Vector3 curPos;
    private Material targetMat;

    private RenderTexture vel_0, vel_1, col_0, col_1, div_0;
    private int texRes = 512;

    private void Start() 
    {
        _camera = Camera.main;
        targetMat = GetComponent<MeshRenderer>().material;

        targetMat.SetTexture("_FinalCTex", oriTex);

        InitTextures();   

        Graphics.Blit(null, col_0, targetMat);
    }
    private void Update() 
    {
        //render 
        SimilateFluid();

    }
    private void OnDisable() 
    {
        //release all textures
        ReleaseTexures();
    }

    private void InitTextures()
    {
        vel_0 = new RenderTexture(texRes, texRes, 0, RenderTextureFormat.RGHalf);
        vel_1 = new RenderTexture(texRes, texRes, 0, RenderTextureFormat.RGHalf);
        vel_0.wrapMode = TextureWrapMode.Clamp;
        vel_1.wrapMode = TextureWrapMode.Clamp;

        col_0 = new RenderTexture(texRes, texRes, 0, RenderTextureFormat.ARGBHalf);
        col_1 = new RenderTexture(texRes, texRes, 0, RenderTextureFormat.ARGBHalf);

        div_0 = new RenderTexture(texRes, texRes, 0, RenderTextureFormat.RGHalf);
    }
    private void ReleaseTexures()
    {
        vel_0.Release();
        vel_1.Release();
        col_0.Release();
        col_1.Release();
        div_0.Release();
    }
    private Vector3 SetInput()
    {
        Vector3 t_curPos = Vector3.zero;
        Vector3 mouseWPos = _camera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit hitInfo;
        if(Physics.Raycast(mouseWPos, _camera.transform.forward, out hitInfo))
        {
            t_curPos = hitInfo.textureCoord;
        }
        return t_curPos;
    }
    private void SimilateFluid()
    {
        //advect
        simMat.SetTexture("_Tex0", vel_0);
        simMat.SetTexture("_Tex1", vel_0);
        simMat.SetFloat("advectSpeed", advectSpeed);
        Graphics.Blit(null, vel_1, simMat, 0);
        SwapTexture(ref vel_0, ref vel_1);//->0

        //diffusion
        simMat.SetFloat("vscosity", vscosity);        
        for (int i = 0; i < diffusionTimes; i++)
        {
            simMat.SetTexture("_Tex0", vel_0);
            simMat.SetTexture("_Tex1", vel_0);
            Graphics.Blit(null, vel_1, simMat, 1);
            SwapTexture(ref vel_0, ref vel_1);//->0
        }


        //force
        if(Input.GetMouseButtonDown(0))
        {
            curPos = SetInput();
            lastPos = curPos;
        }
        if(Input.GetMouseButton(0))   
        {
            curPos = SetInput();
        }
        simMat.SetVector("inputPos", curPos);
        simMat.SetVector("forceVec", (curPos - lastPos) * forceScale);        
        lastPos = curPos;
        simMat.SetTexture("_Tex0", vel_0);
        simMat.SetFloat("radius", radius);
        Graphics.Blit(null, vel_1, simMat, 2);
        SwapTexture(ref vel_0, ref vel_1);//->0

        //divergence
        simMat.SetTexture("_Tex0", vel_0);
        Graphics.Blit(null, div_0, simMat, 3);
        //
        RenderTexture pres_0 = new RenderTexture(texRes, texRes, 0, RenderTextureFormat.RGHalf);
        RenderTexture pres_1 = new RenderTexture(texRes, texRes, 0, RenderTextureFormat.RGHalf);
        pres_0.wrapMode = TextureWrapMode.Clamp;
        pres_1.wrapMode = TextureWrapMode.Clamp;

        for (int i = 0; i < 55; i++)
        {
            simMat.SetTexture("_Tex0", pres_0);
            simMat.SetTexture("_Tex1", div_0);
            Graphics.Blit(null, pres_1, simMat, 4);
            SwapTexture(ref pres_0, ref pres_1);//->0
        }
        //gradient
        simMat.SetTexture("_Tex0", pres_0);
        simMat.SetTexture("_Tex1", vel_0);
        Graphics.Blit(null, vel_1, simMat, 5);
        SwapTexture(ref vel_0, ref vel_1);//->0

        //final color 
        simMat.SetTexture("_Tex0", vel_0);
        simMat.SetTexture("_Tex1", col_0);
        simMat.SetFloat("advectSpeed", advectSpeed);
        Graphics.Blit(null, col_1, simMat, 0);
        targetMat.SetTexture("_FinalCTex", col_1);
        //targetMat.SetTexture("_FinalCTex", vel_0);
        SwapTexture(ref col_0, ref col_1);//->0

        pres_0.Release();
        pres_1.Release();
    }

    private void SwapTexture(ref RenderTexture t0, ref RenderTexture t1)
    {
        RenderTexture tt = t0;
        t0 = t1;
        t1 = tt;
    }
}
