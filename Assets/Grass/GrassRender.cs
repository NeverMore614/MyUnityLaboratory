using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GrassRenderManager
{
    public Mesh grassMesh;
    public MaterialPropertyBlock materialPropertyBlock;
    public HashSet<GrassRender> grassRenders;
    public void InitResource()
    {
        Debug.Log(SystemInfo.supportsInstancing);
        //网格
        grassMesh = new Mesh();
        grassMesh.SetVertices(new List<Vector3>()
        {
            new Vector3(-0.5f, 0, 0),
            new Vector3(-0.5f, 1, 0),
            new Vector3(0.5f, 0, 0),
            new Vector3(0.5f, 1, 0),
        }
            );

        grassMesh.SetUVs(0, new List<Vector2>
            {
                new Vector2(0, 0),
                new Vector2(0, 1),
                new Vector2(1, 0),
                new Vector2(1, 1),
            });
        grassMesh.triangles = new int[6] { 0, 1, 2, 1, 2, 3 };
        //草地缓存
        grassRenders = new HashSet<GrassRender>();
        //shader数据
        materialPropertyBlock = new MaterialPropertyBlock();
    }
}

public class GrassRender : MonoBehaviour
{

    static GrassRenderManager instance;
    public static GrassRenderManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GrassRenderManager();
                instance.InitResource();
            }
            return instance;
        }
    }





    struct GrassInfo
    {
        //顶点到gameobject本地坐标
        public Matrix4x4 vertexToLocal;
    }
    private Mesh m_Mesh;
    private ComputeBuffer m_ComputeBuffer;
    [HideInInspector]
    public int m_GrassCount;
    public Vector2 m_GrassGenerateCount;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Start");
        var mf = gameObject.GetComponent<MeshFilter>();
        if (mf != null)
        { 
            m_Mesh = mf.sharedMesh;
        }

    }

    public void SetMaterialProperties()
    {
        Instance.materialPropertyBlock.SetMatrix(GrassShaderProperties._LocalToWorldPropertiesId, transform.localToWorldMatrix);
        Instance.materialPropertyBlock.SetBuffer(GrassShaderProperties._GrassInfos, GetGrassInfos());
    }

    private void OnEnable()
    {
        if (Instance.grassRenders.Contains(this))
        {
            return;
        }
        Instance.grassRenders.Add(this);
    }

    private void OnDisable()
    {
        if (Instance.grassRenders.Contains(this))
        {
            Instance.grassRenders.Remove(this);
        }
    }

    private void OnDestroy()
    {
        if (Instance.grassRenders.Contains(this))
        {
            Instance.grassRenders.Remove(this);
        }
        if (m_ComputeBuffer != null)
        {
            m_ComputeBuffer.Release();
        }
    }

    ComputeBuffer GetGrassInfos()
    {
        if (m_ComputeBuffer != null)
        {
            return m_ComputeBuffer;
        }
        m_GrassCount = 0;
        List<Matrix4x4> grassInfos = new List<Matrix4x4>();

        var indices = m_Mesh.triangles;
        var vertices = m_Mesh.vertices;

        for (int i = 0; i < indices.Length / 3; i++)
        {
            var index1 = indices[i * 3];
            var index2 = indices[i * 3 + 1];
            var index3 = indices[i * 3 + 2];
            var v1 = vertices[index1];
            var v2 = vertices[index2];
            var v3 = vertices[index3];
            var generateCount = Random.Range(m_GrassGenerateCount.x, m_GrassGenerateCount.y);
            for (int j = 0; j < generateCount; j++)
            {
                var vertex = RandomPosInTriangles(v1, v2, v3);
                var upNormal = GetNormalInTriangles(v1, v2, v3);
                //GrassInfo grassInfo = new GrassInfo();
                Matrix4x4 vertexToLocal = Matrix4x4.TRS(vertex, Quaternion.FromToRotation(Vector3.up, upNormal) * Quaternion.Euler(0, Random.Range(0, 180), 0), Vector3.one);
                grassInfos.Add(vertexToLocal);
                m_GrassCount++;
            }
        }
        Debug.Log(m_GrassCount);
        m_ComputeBuffer = new ComputeBuffer(m_GrassCount, 64);
        m_ComputeBuffer.SetData(grassInfos);
        return m_ComputeBuffer;
    }

    private void OnDrawGizmos()
    {
        
    }

    Vector3 RandomPosInTriangles(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        var x = Random.Range(0, 1f);
        var y = Random.Range(0, 1f);
        if (y > 1 - x)
        {
            //如果随机到了右上区域，那么反转到左下
            var temp = y;
            y = 1 - x;
            x = 1 - temp;
        }
        var vx = v2 - v1;
        var vy = v3 - v1;
        return v1 + x * vx + y * vy;
    }

    Vector3 GetNormalInTriangles(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        var vx = v2 - v1;
        var vy = v3 - v1;
        return Vector3.Cross(vx, vy);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
