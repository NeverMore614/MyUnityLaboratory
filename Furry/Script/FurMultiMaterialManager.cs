using UnityEngine;

public class FurMultiMaterialManager : MonoBehaviour
{
    [Range(2, 100)] public int step = 10;//层数

    //shader参数
    [Header("【贴图】")]
    [Space(20)]
    public Texture2D _MainTex;
    public Vector4 _MainTex_ST = new Vector4(1, 1, 0, 0);
    public Texture2D _FurMask;
    public float _FurScale = 10;
    [Range(-5, 5)] public float _NormalInt = 1;

    
    [Header("【颜色】")]
    [Space(20)]
    public Color _BrightCol = Color.white;
    public Color _DarkCol = Color.black;
    [Range(0, 1)] public float _AOInt = 0.5f;
    [Range(0, 4)] public float _OpacityPow = 1;

    
    [Header("【形状】")]
    [Space(20)]
    [Range(0, 0.5f)] public float _Thick = 0.1f;
    [Range(0, 4)] public float _ConePow = 1;

    //
    private void Start()
    {
        SetMaterials();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        SetMaterials();
    }
#endif

    private void SetMaterials()
    {
        Renderer renderer;
        if (this.TryGetComponent<Renderer>(out renderer)) 
        {
            //初始化传参
            Material[] tempMats = new Material[step];
            tempMats[0] = new Material(Shader.Find("Effect/Fur_MultiMaterial"));

            tempMats[0].SetTexture("_MainTex", _MainTex);
            tempMats[0].SetVector("_MainTex_ST", _MainTex_ST);
            tempMats[0].SetTexture("_FurMask", _FurMask);
            tempMats[0].SetFloat("_FurScale", _FurScale);
            tempMats[0].SetFloat("_NormalInt", _NormalInt);

            tempMats[0].SetColor("_BrightCol", _BrightCol);
            tempMats[0].SetColor("_DarkCol", _DarkCol);
            tempMats[0].SetFloat("_AOInt", _AOInt);
            tempMats[0].SetFloat("_OpacityPow", _OpacityPow);

            tempMats[0].SetFloat("_ConePow", _ConePow);
            tempMats[0].SetInt("_UseVColHeight", 0);
            tempMats[0].SetFloat("_Thick", _Thick);
            tempMats[0].SetFloat("_Height01", 0);

            //遍历计算高度偏移
            for (int i = 1; i < tempMats.Length; i++)
            {
                tempMats[i] = new Material(tempMats[0]);
                tempMats[i].SetFloat("_Height01", i / (step - 1.0f));
            }

            //传递renderer
            renderer.materials = tempMats;
        }
    }
}
