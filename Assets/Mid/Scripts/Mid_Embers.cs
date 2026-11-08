using UnityEngine;

// 화로 위로 피어오르는 불티 (필수 ④)
// 정점마다 M = T(화로) * Ry * T(반지름, 높이, 0) * Rx * Rz * S 를 곱해 움직인다.
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_Embers : MonoBehaviour
{
    public Transform center;          // 화로
    public int count = 14;
    public float riseHeight = 2.2f;
    public float riseSpeed = 0.35f;   // 한 번 오르는 데 걸리는 시간의 역수
    public float baseRadius = 0.35f;
    public float emberSize = 0.07f;

    // 불티 하나의 기본 형태: 8면체
    static readonly Vector3[] baseVerts =
    {
        new Vector3(-0.5f, 0, -0.5f), new Vector3(0.5f, 0, -0.5f),
        new Vector3(0.5f, 0, 0.5f),   new Vector3(-0.5f, 0, 0.5f),
        new Vector3(0, 1f, 0),        new Vector3(0, -1f, 0),
    };
    static readonly int[] baseTris =
    {
        0,4,1, 1,4,2, 2,4,3, 3,4,0,
        0,1,5, 1,2,5, 2,3,5, 3,0,5,
    };

    Mesh mesh;
    Vector3[] verts;
    float[] startAngle, spin, phase, life, size, tumble;

    // k번째 불티마다 고정된 0~1 난수 (실행할 때마다 같은 모양)
    static float Rand(int k, float salt)
    {
        float x = Mathf.Sin(k * 12.9898f + salt * 78.233f) * 43758.5453f;
        return x - Mathf.Floor(x);
    }

    void Start()
    {
        int vc = baseVerts.Length;
        verts = new Vector3[vc * count];
        var tris = new int[baseTris.Length * count];
        startAngle = new float[count]; spin = new float[count]; phase = new float[count];
        life = new float[count];       size = new float[count]; tumble = new float[count];

        for (int k = 0; k < count; k++)
        {
            for (int t = 0; t < baseTris.Length; t++) tris[k * baseTris.Length + t] = baseTris[t] + k * vc;
            startAngle[k] = 360f * k / count + Rand(k, 1f) * 20f;
            spin[k]  = (70f + Rand(k, 2f) * 90f) * (k % 5 == 0 ? -1f : 1f); // 가끔 반대로 도는 불티
            phase[k] = Rand(k, 3f);                                           // 오르기 시작하는 시점
            life[k]  = 0.8f + Rand(k, 4f) * 0.5f;                             // 불티마다 수명 배율
            size[k]  = 0.6f + Rand(k, 5f) * 0.9f;
            tumble[k] = 1.5f + Rand(k, 6f) * 3f;
        }

        mesh = new Mesh { name = "Mid_Embers" };
        mesh.vertices = verts;
        mesh.triangles = tris;
        GetComponent<MeshFilter>().mesh = mesh;
        // 메시를 월드 좌표로 직접 채우므로 오브젝트 자체 변환은 항등으로 둔다
        transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        transform.localScale = Vector3.one;
        Update();
    }

    void Update()
    {
        Vector3 c = center != null ? center.position : Vector3.zero;
        int vc = baseVerts.Length;
        float time = Time.time;

        for (int k = 0; k < count; k++)
        {
            // p: 0(생김) -> 1(사라짐)
            float p = Mathf.Repeat(time * riseSpeed / life[k] + phase[k], 1f);

            float y = riseHeight * (1f - Mathf.Pow(1f - p, 1.6f));              // 점점 느려지며 오름
            float r = baseRadius * (0.5f + 1.1f * p)                             // 올라갈수록 나선이 넓어짐
                      + 0.04f * Mathf.Sin(time * 3f + k * 2.1f);                 // 살짝 흔들림
            float ang = (startAngle[k] + spin[k] * time) * Mathf.Deg2Rad;

            // 크기: 0에서 커졌다가 다시 0으로 (부드러운 등장과 소멸)
            float env = Mathf.Pow(Mathf.Sin(p * Mathf.PI), 0.7f);
            float s = emberSize * size[k] * env;

            float tum = time * tumble[k] + k;
            Matrix4x4 M = Translate(c) * RotateY(ang) * Translate(new Vector3(r, y, 0))
                        * RotateX(tum) * RotateZ(tum * 0.7f)
                        * Scale(s * 0.7f, s * 1.5f, s * 0.7f);

            for (int i = 0; i < vc; i++)
                verts[k * vc + i] = M.MultiplyPoint3x4(baseVerts[i]);
        }
        mesh.vertices = verts;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    static Matrix4x4 Translate(Vector3 t)
    {
        var m = Matrix4x4.identity;
        m.m03 = t.x; m.m13 = t.y; m.m23 = t.z;
        return m;
    }

    static Matrix4x4 RotateY(float a)
    {
        var m = Matrix4x4.identity;
        float c = Mathf.Cos(a), s = Mathf.Sin(a);
        m.m00 = c;  m.m02 = s;
        m.m20 = -s; m.m22 = c;
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

    static Matrix4x4 RotateZ(float a)
    {
        var m = Matrix4x4.identity;
        float c = Mathf.Cos(a), s = Mathf.Sin(a);
        m.m00 = c; m.m01 = -s;
        m.m10 = s; m.m11 = c;
        return m;
    }

    static Matrix4x4 Scale(float x, float y, float z)
    {
        var m = Matrix4x4.identity;
        m.m00 = x; m.m11 = y; m.m22 = z;
        return m;
    }
}
