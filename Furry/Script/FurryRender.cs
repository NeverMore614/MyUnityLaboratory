using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FurryRenderManager
{
    public HashSet<FurryRender> furryRenders;
    public MaterialPropertyBlock materialPropertyBlock;
    public void InitResource()
    {
        //shader数据
        furryRenders = new HashSet<FurryRender>();
        materialPropertyBlock = new MaterialPropertyBlock();
    }
}
[ExecuteInEditMode]
public class FurryRender : MonoBehaviour
{
    static FurryRenderManager instance;
    public static FurryRenderManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new FurryRenderManager();
                instance.InitResource();
            }
            return instance;
        }
    }
    [HideInInspector]
    public Mesh m_Mesh;

    private Texture m_Texture;
    private Vector3 m_WorldPosition;
    private Vector3 m_offset;

    [Header("pass数量（越多越精细）")]
    public int m_PassCount = 1;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start");
        var mf = gameObject.GetComponent<MeshFilter>();
        if (mf != null)
        {
            m_Mesh = mf.sharedMesh;
        }
        var renderer = gameObject.GetComponent<Renderer>();
        if (renderer != null)
        { 
            Material material = renderer.sharedMaterial;
            if (material != null)
            {
                m_Texture = material.mainTexture;
            }
        }
        m_WorldPosition = transform.position;
    }

    public void SetMaterialProperties()
    {
        Instance.materialPropertyBlock.SetMatrix(GrassShaderProperties._LocalToWorldPropertiesId, transform.localToWorldMatrix);
        Instance.materialPropertyBlock.SetFloat(GrassShaderProperties._PassCount, m_PassCount);
        if (Application.isPlaying)
        {
            Instance.materialPropertyBlock.SetVector(GrassShaderProperties._UVOffest, m_offset);
        }
 
        if (m_Texture != null)
        {
            Instance.materialPropertyBlock.SetTexture(GrassShaderProperties._ColorTex, m_Texture);
        }
    }



    private void OnEnable()
    {
        if (Instance.furryRenders.Contains(this))
        {
            return;
        }
        Instance.furryRenders.Add(this);
    }

    private void OnDisable()
    {
        if (Instance.furryRenders.Contains(this))
        {
            Instance.furryRenders.Remove(this);
        }
    }

    private void OnDestroy()
    {
        if (Instance.furryRenders.Contains(this))
        {
            Instance.furryRenders.Remove(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        m_offset = Vector3.Lerp(m_offset, (transform.position - m_WorldPosition) * 30, 0.05f) ;
        m_WorldPosition = transform.position;
        Debug.Log(m_offset);
    }
}
