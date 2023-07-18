using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;

public class MultiLayerMesh : MonoBehaviour
{
    [Range(2, 100)] public int step = 10;//层数
    [Range(0, 1)] public float thick = 0.01f;//厚度

    public Mesh meshIN;//输入网格

    private void Start()
    {
        SetMesh();
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        SetMesh();
    }
#endif

    private void SetMesh()
    {
        if (meshIN)
        {
            if (meshIN.isReadable)
            {
                MeshFilter meshFilter;
                SkinnedMeshRenderer skinRenderer;
                if (this.TryGetComponent<MeshFilter>(out meshFilter))
                {
                    meshFilter.mesh = CreateMesh(meshIN, step, thick);
                }
                else if (this.TryGetComponent<SkinnedMeshRenderer>(out skinRenderer))
                {
                    skinRenderer.sharedMesh = CreateMesh(meshIN, step, thick);
                }
            }
            else
            {
                Debug.Log("网格设置为【不可读写】");
            }
        }
    }

    public static Mesh CreateMesh(Mesh baseMesh, int stepMax, float thick)
    {
        float deltaThick = thick / (stepMax - 1);//单步厚度变化量
        float deltaVCol = 1.0f / (stepMax - 1);//单步顶点色变化量,用来标识层数高低

        //输出网格数据准备
        Mesh mesh = new Mesh();
        List<Vector3> verts = new List<Vector3>();
        List<Color> colors = new List<Color>(); //顶点色记录层数归一化高度
        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector4> tangents = new List<Vector4>();
        List<int> indexs = new List<int>();
        List<BoneWeight> weights = new List<BoneWeight>();

        //输入的数据集
        Vector3[] vertsIN = baseMesh.vertices;
        Vector2[] uvsIN = baseMesh.uv;
        Vector3[] normalsIN = baseMesh.normals;
        Vector4[] tangentsIN = baseMesh.tangents;
        int[] indexsIN = baseMesh.GetIndices(0);
        BoneWeight[] weightsIN = baseMesh.boneWeights;

        //遍历，顶点沿法线方向偏移
        for (int stepIdx = 0; stepIdx < stepMax; stepIdx++)
        {
            //数据
            for (int vIdx = 0; vIdx < baseMesh.vertexCount; vIdx++)
            {
                verts.Add(vertsIN[vIdx] + deltaThick * stepIdx * normalsIN[vIdx]);
                colors.Add(Color.white * deltaVCol * stepIdx);
                uvs.Add(uvsIN[vIdx]);
                normals.Add(normalsIN[vIdx]);
                tangents.Add(tangentsIN[vIdx]);
                if (weightsIN.Length > 0)
                {
                    weights.Add(weightsIN[vIdx]);
                }
            }

            //顶点索引
            for (int i = 0; i < indexsIN.Length; i++)
            {
                indexs.Add(indexsIN[i] + baseMesh.vertexCount * stepIdx);
            }
        }

        //数据导入
        mesh.SetVertices(verts);
        mesh.SetColors(colors);
        mesh.SetUVs(0, uvs);
        mesh.SetNormals(normals);
        mesh.SetTangents(tangents);
        mesh.SetIndices(indexs, baseMesh.GetTopology(0), 0);
        if (weightsIN.Length > 0)
        {
            mesh.bounds = baseMesh.bounds;
            mesh.boneWeights = weights.ToArray();
            mesh.bindposes = baseMesh.bindposes;
        }

        return mesh;
    }
}