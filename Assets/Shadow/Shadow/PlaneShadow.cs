using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaneShadow : MonoBehaviour
{
    [Header("光的方向")]
    public Vector3 mLightDir;
    [Header("平面方向（相对于中心点）")]
    public Vector3 mPlaneDir;
    [Header("平面距离")]
    public float mDistance;
    [Header("光的颜色")]
    public Color mColor;

    float mDistanceChange;

    Renderer[] renderers;
    MaterialPropertyBlock materialPropertyBlock;
    string shaderName = "Study/PlaneShadow";

    public static readonly int _DistancePropertyID = Shader.PropertyToID("_PlaneDistance");
    public static readonly int _CosLPPropertyID = Shader.PropertyToID("_CosLP");
    public static readonly int _LightDirNormalPropertyID = Shader.PropertyToID("_LightDirNormal");
    public static readonly int _PlaneDirNormalPropertyID = Shader.PropertyToID("_PlaneDirNormal");
    public static readonly int _ShadowColorPropertyID = Shader.PropertyToID("_ShadowColor");

    // Start is called before the first frame update
    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        materialPropertyBlock = new MaterialPropertyBlock();
        Material shadowMat = new Material(Shader.Find(shaderName));
        if (shadowMat == null)
        {
            return;
        }
        foreach (var render in renderers)
        {
            int length = render.materials.Length;
            Material[] materials = new Material[length + 1];
            for (int i = 0; i < length; i++)
            {
                materials[i] = render.materials[i];
            }
            materials[length] = shadowMat;
            render.materials = materials;
        }

        mDistanceChange = mDistance;
        calculate();
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    void calculate()
    {
        Debug.Log("计算");
        Vector3 mLightDirNormal = mLightDir.normalized;
        Vector3 mPlaneDirNormal = mPlaneDir.normalized;
        materialPropertyBlock.SetVector(_LightDirNormalPropertyID, mLightDirNormal);
        materialPropertyBlock.SetVector(_PlaneDirNormalPropertyID, mPlaneDirNormal);
        //光线和平面方向余弦
        float cosLP = Vector3.Dot(mLightDirNormal, mPlaneDirNormal);
        materialPropertyBlock.SetFloat(PlaneShadow._DistancePropertyID, mDistance);
        materialPropertyBlock.SetFloat(PlaneShadow._CosLPPropertyID, cosLP);
        materialPropertyBlock.SetColor(PlaneShadow._ShadowColorPropertyID, mColor);


        foreach (Renderer render in renderers)
        {
            render.SetPropertyBlock(materialPropertyBlock);
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        calculate();
    }
}
