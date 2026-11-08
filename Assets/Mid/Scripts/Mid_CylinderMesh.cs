using System.Collections.Generic;
using UnityEngine;

// 반복문으로 만드는 n각 원기둥 (필수 ①)
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_CylinderMesh : MonoBehaviour
{
    [Range(3, 64)] public int n = 24;
    public float radius = 0.5f;
    public float height = 2f;

    int lastN;

    void Start() { Build(); }

    void Update()
    {
        // 인스펙터에서 n을 바꾸면 메시를 다시 만든다
        if (n != lastN) Build();
    }

    void Build()
    {
        lastN = n;
        var v = new List<Vector3>();
        var uv = new List<Vector2>();
        var tri = new List<int>();

        // 1) 옆면: 아래/위 링을 번갈아 추가 (마지막 점은 첫 점과 같은 위치, UV 이음새용)
        for (int i = 0; i <= n; i++)
        {
            float a = 2f * Mathf.PI * i / n;
            float x = Mathf.Cos(a) * radius, z = Mathf.Sin(a) * radius;
            v.Add(new Vector3(x, 0, z));      uv.Add(new Vector2((float)i / n, 0));
            v.Add(new Vector3(x, height, z)); uv.Add(new Vector2((float)i / n, 1));
        }
        for (int i = 0; i < n; i++)
        {
            int b = 2 * i;
            tri.Add(b);     tri.Add(b + 1); tri.Add(b + 2);
            tri.Add(b + 1); tri.Add(b + 3); tri.Add(b + 2);
        }

        // 2) 윗면: 중심 + 링 (평평하게 보이도록 별도 정점)
        int topCenter = v.Count;
        v.Add(new Vector3(0, height, 0)); uv.Add(new Vector2(0.5f, 0.5f));
        int topRing = v.Count;
        for (int i = 0; i <= n; i++)
        {
            float a = 2f * Mathf.PI * i / n;
            v.Add(new Vector3(Mathf.Cos(a) * radius, height, Mathf.Sin(a) * radius));
            uv.Add(new Vector2(Mathf.Cos(a) * 0.5f + 0.5f, Mathf.Sin(a) * 0.5f + 0.5f));
        }
        for (int i = 0; i < n; i++)
        {
            tri.Add(topCenter); tri.Add(topRing + i + 1); tri.Add(topRing + i);
        }

        // 3) 아랫면: 방향 반대
        int botCenter = v.Count;
        v.Add(new Vector3(0, 0, 0)); uv.Add(new Vector2(0.5f, 0.5f));
        int botRing = v.Count;
        for (int i = 0; i <= n; i++)
        {
            float a = 2f * Mathf.PI * i / n;
            v.Add(new Vector3(Mathf.Cos(a) * radius, 0, Mathf.Sin(a) * radius));
            uv.Add(new Vector2(Mathf.Cos(a) * 0.5f + 0.5f, Mathf.Sin(a) * 0.5f + 0.5f));
        }
        for (int i = 0; i < n; i++)
        {
            tri.Add(botCenter); tri.Add(botRing + i); tri.Add(botRing + i + 1);
        }

        var mesh = new Mesh { name = "Mid_Cylinder" };
        mesh.SetVertices(v);
        mesh.SetUVs(0, uv);
        mesh.SetTriangles(tri, 0);
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
