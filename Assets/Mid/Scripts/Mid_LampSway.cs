using UnityEngine;

// 바람에 흔들리는 등불 (선택 ④)
// 램프 메시의 정점에 M = Rx(b) * Rz(a) 를 곱해 흔들고, 안의 조명도 같은 행렬로 옮긴다.
// 회전축은 램프 메시의 원점(지붕에 매달린 윗부분)이다.
[RequireComponent(typeof(MeshFilter))]
public class Mid_LampSway : MonoBehaviour
{
    public float maxAngle = 22f;     // 최대 흔들림 각도(도)
    public float speed = 1.4f;       // 기본 흔들림 속도
    public float gustSpeed = 0.35f;  // 바람 세기가 약해졌다 강해지는 속도

    Mesh mesh;
    Vector3[] baseVerts, baseNormals;
    Vector3[] verts, normals;

    Transform[] lights;
    Vector3[] lightPos;    // 조명의 원래 로컬 위치
    Vector3[] lightFwd;    // 조명의 원래 로컬 앞 방향
    Vector3[] lightUp;

    void Start()
    {
        // 공유 메시를 건드리지 않도록 복사본을 쓴다
        mesh = Instantiate(GetComponent<MeshFilter>().sharedMesh);
        mesh.name = "Mid_Lamp_Runtime";
        GetComponent<MeshFilter>().mesh = mesh;

        baseVerts = mesh.vertices;
        baseNormals = mesh.normals;
        verts = new Vector3[baseVerts.Length];
        normals = new Vector3[baseNormals.Length];

        int n = transform.childCount;
        lights = new Transform[n];
        lightPos = new Vector3[n]; lightFwd = new Vector3[n]; lightUp = new Vector3[n];
        for (int i = 0; i < n; i++)
        {
            lights[i] = transform.GetChild(i);
            lightPos[i] = lights[i].localPosition;
            lightFwd[i] = lights[i].localRotation * Vector3.forward;
            lightUp[i]  = lights[i].localRotation * Vector3.up;
        }
    }

    void Update()
    {
        float t = Time.time;
        // 바람: 세기가 천천히 변해서 규칙적인 시계추처럼 보이지 않게 함
        float gust = 0.55f + 0.45f * Mathf.Sin(t * gustSpeed * Mathf.PI);
        float a = Mathf.Sin(t * speed) * maxAngle * gust * Mathf.Deg2Rad;                        // 좌우
        float b = Mathf.Sin(t * speed * 0.73f + 1.3f) * maxAngle * 0.5f * gust * Mathf.Deg2Rad;  // 앞뒤

        Matrix4x4 M = RotateX(b) * RotateZ(a);

        for (int i = 0; i < baseVerts.Length; i++)
        {
            verts[i] = M.MultiplyPoint3x4(baseVerts[i]);
            normals[i] = M.MultiplyVector(baseNormals[i]);   // 회전만 있으므로 법선도 같은 행렬
        }
        mesh.vertices = verts;
        mesh.normals = normals;
        mesh.RecalculateBounds();

        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].localPosition = M.MultiplyPoint3x4(lightPos[i]);
            lights[i].localRotation = Quaternion.LookRotation(M.MultiplyVector(lightFwd[i]),
                                                              M.MultiplyVector(lightUp[i]));
        }
    }

    static Matrix4x4 RotateZ(float a)
    {
        var m = Matrix4x4.identity;
        float c = Mathf.Cos(a), s = Mathf.Sin(a);
        m.m00 = c; m.m01 = -s;
        m.m10 = s; m.m11 = c;
        return m;
    }

    static Matrix4x4 RotateX(float a)
    {
        var m = Matrix4x4.identity;
        float c = Mathf.Cos(a), s = Mathf.Sin(a);
        m.m11 = c; m.m12 = -s;
        m.m21 = s; m.m22 = c;
        return m;
    }
}
