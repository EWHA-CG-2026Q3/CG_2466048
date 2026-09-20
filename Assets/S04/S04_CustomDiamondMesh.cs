using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class S04_CustomDiamondMesh : MonoBehaviour
{
    void Start()
    {
        Vector3[] vertices = new Vector3[]
        {
            new Vector3(0f, 0f, 0f),     // 0 (정육면체 0번, 허리띠)
            new Vector3(1f, 0f, 0f),     // 1 (정육면체 1번, 허리띠)
            new Vector3(1f, 0f, 1f),     // 2 (정육면체 5번, 허리띠)
            new Vector3(0f, 0f, 1f),     // 3 (정육면체 4번, 허리띠)
            new Vector3(0.5f, 1f, 0.5f),  // 4 (새 정점, 윗꼭짓점)
            new Vector3(0.5f, -1f, 0.5f), // 5 (새 정점, 아랫꼭짓점)
        };

        int[] triangles = new int[]
        {
            // 위쪽 4면 (정점 4 사용)
            0, 4, 1,
            1, 4, 2,
            2, 4, 3,
            3, 4, 0,

            // 아래쪽 4면 (정점 5 사용)
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    }
}