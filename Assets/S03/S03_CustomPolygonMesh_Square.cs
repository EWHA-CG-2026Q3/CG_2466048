  using UnityEngine;

  [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
  public class S03_CustomPolygonMesh_Square : MonoBehaviour
  {
    void Start()
    {
        Vector3[] vertices = new Vector3[]
        {
            new Vector3( 0.00f,  3.00f, 0f), // 0 위
            new Vector3( 1.56f,  2.25f, 0f), // 1 오른쪽 위
            new Vector3( 1.95f,  0.55f, 0f), // 2 오른쪽
            new Vector3( 0.87f, -0.80f, 0f), // 3 오른쪽 아래
            new Vector3(-0.87f, -0.80f, 0f), // 4 왼쪽 아래
            new Vector3(-1.95f,  0.55f, 0f), // 5 왼쪽
            new Vector3(-1.56f,  2.25f, 0f), // 6 왼쪽 위
        };

        int[] triangles = new int[]
        {
            0, 1, 2,
            0, 2, 3,
            0, 3, 4,
            0, 4, 5,
            0, 5, 6,
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshRenderer>().sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    }
}