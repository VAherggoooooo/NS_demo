using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NS_Fluid : MonoBehaviour
{       
    #region variables
   public Texture2D oriTex;//原始纹理
    public Material simMat;//流体模拟材质
    private Material targetMat;//最终渲染到的材质
    [Range(0, 55)]public int diffusionTimes = 8;
    public float forceScale = 200.0f;
    public float radius = 0.2f;
    public float advectSpeed = 1.0f;
    public float vscosity = 0.5f;
    private Camera _camera;
    private Vector3 lastPos, curPos;    

    private RenderTexture vel_0, vel_1, col_0, col_1, div_0;
    private int texRes = 512;
    #endregion

    #region Other functions
    private void Start() 
    {
        _camera = Camera.main;
        targetMat = GetComponent<MeshRenderer>().material;

        if(targetMat == null || simMat == null)
        {
            Debug.Log("材质未找到");
            enabled = false;
            return;
        }
        targetMat.SetTexture("_FinalCTex", oriTex);

        //创建并初始化纹理
        InitTextures();   

        Graphics.Blit(oriTex, col_0);
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
        vel_0 = new RenderTexture(texRes, texRes, 0, RenderTextureFormat.RGHalf);//可以储存负值的纹理
        vel_1 = new RenderTexture(texRes, texRes, 0, RenderTextureFormat.RGHalf);//可以储存负值的纹理

        col_0 = new RenderTexture(texRes, texRes, 0, RenderTextureFormat.ARGBHalf);
        col_1 = new RenderTexture(texRes, texRes, 0, RenderTextureFormat.ARGBHalf);

        div_0 = new RenderTexture(texRes, texRes, 0, RenderTextureFormat.RHalf);
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
        //检测鼠标输入
        Vector3 t_curPos = Vector3.zero;
        Vector3 mouseWPos = _camera.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit hitInfo;
        if(Physics.Raycast(mouseWPos, _camera.transform.forward, out hitInfo))
        {
            t_curPos = hitInfo.textureCoord;
        }
        return t_curPos;
    }

    /// <summary>
    /// 交换纹理 
    /// </summary>
    /// <param name="t0"></param>
    /// <param name="t1"></param>
    private void SwapTexture(ref RenderTexture t0, ref RenderTexture t1)
    {
        RenderTexture tt = t0;
        t0 = t1;
        t1 = tt;
    }
    #endregion


    // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

    #region NS Similate
    private void SimilateFluid()
    {
        //>>>>>>>>>>>>>>>>>>>>>>>>>>>advect
        simMat.SetTexture("_Tex0", vel_0);
        simMat.SetTexture("_Tex1", vel_0);
        simMat.SetFloat("advectSpeed", advectSpeed);
        Graphics.Blit(null, vel_1, simMat, 0);//render
        SwapTexture(ref vel_0, ref vel_1);//->0

        //>>>>>>>>>>>>>>>>>>>>>>>>>>>diffusion
        simMat.SetFloat("vscosity", vscosity);        
        for (int i = 0; i < diffusionTimes; i++)
        {
            simMat.SetTexture("_Tex0", vel_0);
            simMat.SetTexture("_Tex1", vel_0);
            Graphics.Blit(null, vel_1, simMat, 1);//render
            SwapTexture(ref vel_0, ref vel_1);//->0
        }

        //>>>>>>>>>>>>>>>>>>>>>>>>>>>force
        if(Input.GetMouseButtonDown(0)) lastPos = SetInput();
        if(Input.GetMouseButton(0)) curPos = SetInput();
        simMat.SetVector("inputPos", curPos);
        simMat.SetVector("forceVec", (curPos - lastPos) * forceScale);               
        lastPos = curPos;
        simMat.SetTexture("_Tex0", vel_0);
        simMat.SetFloat("radius", radius);
        Graphics.Blit(null, vel_1, simMat, 2);//render
        SwapTexture(ref vel_0, ref vel_1);//->0

        //>>>>>>>>>>>>>>>>>>>>>>>>>>>divergence
        simMat.SetTexture("_Tex0", vel_0);
        Graphics.Blit(null, div_0, simMat, 3);//render
        //>>>>>>>>>>>>>>>>>>>>>>>>>>>presure
        RenderTexture pres_0 = new RenderTexture(texRes, texRes, 0, RenderTextureFormat.RHalf);
        RenderTexture pres_1 = new RenderTexture(texRes, texRes, 0, RenderTextureFormat.RHalf);
        for (int i = 0; i < 55; i++)
        {
            simMat.SetTexture("_Tex0", pres_0);
            simMat.SetTexture("_Tex1", div_0);
            Graphics.Blit(null, pres_1, simMat, 4);//render
            SwapTexture(ref pres_0, ref pres_1);//->0
        }
        //>>>>>>>>>>>>>>>>>>>>>>>>>>>gradient
        simMat.SetTexture("_Tex0", pres_0);
        simMat.SetTexture("_Tex1", vel_0);
        Graphics.Blit(null, vel_1, simMat, 5);//render
        SwapTexture(ref vel_0, ref vel_1);//->0

        //>>>>>>>>>>>>>>>>>>>>>>>>>>>final color 
        simMat.SetTexture("_Tex0", vel_0);
        simMat.SetTexture("_Tex1", col_0);
        simMat.SetFloat("advectSpeed", advectSpeed);
        Graphics.Blit(null, col_1, simMat, 0);//render
        targetMat.SetTexture("_FinalCTex", col_1);//show final target        
        SwapTexture(ref col_0, ref col_1);//->0

        pres_0.Release();
        pres_1.Release();
    }
    #endregion
}
