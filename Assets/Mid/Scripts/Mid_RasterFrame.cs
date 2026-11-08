using UnityEngine;

// 래스터화 액자 (선택 ②)
// 수업의 소프트웨어 래스터라이저(barycentric 좌표로 삼각형 안팎 판정)를 확장해 그림을 그린다.
//   - 정점 색을 barycentric 가중치로 보간 (그라데이션)
//   - 삼각형 부채로 원(달)을 그림
public class Mid_RasterFrame : MonoBehaviour
{
    public int width = 512;
    public int height = 352;
    public Material pictureMaterial;   // 그림이 붙을 재질 (Lit, Emission 사용)
    public Material woodMaterial;      // 액자 테두리

    Color32[] pixels;

    void Start()
    {
        pixels = new Color32[width * height];
        PaintScene();

        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixels32(pixels);
        tex.Apply();

        BuildFrame(tex);
    }

    // 그림
    void PaintScene()
    {
        float W = width, H = height;

        // 하늘: 화면 전체를 삼각형 2개로 덮고, 위/아래 정점 색을 달리해 보간
        Color top = new Color(0.03f, 0.05f, 0.18f), low = new Color(0.55f, 0.28f, 0.42f);
        Tri(new Vector2(0, 0), new Vector2(W, 0), new Vector2(W, H), low, low, top);
        Tri(new Vector2(0, 0), new Vector2(W, H), new Vector2(0, H), low, top, top);

        // 별: 아주 작은 삼각형
        for (int i = 0; i < 70; i++)
        {
            float x = Hash(i, 1f) * W, y = H * 0.35f + Hash(i, 2f) * H * 0.65f;
            float s = 1.2f + Hash(i, 3f) * 1.8f;
            Color c = Color.Lerp(new Color(0.7f, 0.75f, 1f), Color.white, Hash(i, 4f));
            Tri(new Vector2(x, y + s), new Vector2(x - s, y - s), new Vector2(x + s, y - s), c, c, c);
        }

        // 달: 삼각형 부채 (중심색은 밝고 가장자리는 살짝 어둡게)
        Vector2 mc = new Vector2(W * 0.74f, H * 0.72f);
        float mr = H * 0.13f;
        int seg = 28;
        Color moonIn = new Color(1f, 0.97f, 0.82f), moonOut = new Color(0.92f, 0.86f, 0.66f);
        for (int i = 0; i < seg; i++)
        {
            float a0 = 2f * Mathf.PI * i / seg, a1 = 2f * Mathf.PI * (i + 1) / seg;
            Tri(mc, mc + new Vector2(Mathf.Cos(a0), Mathf.Sin(a0)) * mr,
                    mc + new Vector2(Mathf.Cos(a1), Mathf.Sin(a1)) * mr, moonIn, moonOut, moonOut);
        }

        // 먼 산 / 가까운 산: 삼각형 지그재그
        Mountains(H * 0.30f, H * 0.18f, new Color(0.17f, 0.17f, 0.36f), new Color(0.10f, 0.11f, 0.26f), 7, 11f);
        Mountains(H * 0.20f, H * 0.14f, new Color(0.08f, 0.10f, 0.22f), new Color(0.04f, 0.06f, 0.15f), 5, 23f);

        // 땅
        Color g0 = new Color(0.05f, 0.09f, 0.10f), g1 = new Color(0.02f, 0.04f, 0.06f);
        Tri(new Vector2(0, 0), new Vector2(W, 0), new Vector2(W, H * 0.2f), g1, g1, g0);
        Tri(new Vector2(0, 0), new Vector2(W, H * 0.2f), new Vector2(0, H * 0.2f), g1, g0, g0);

        // 정자 실루엣: 지붕(삼각형) + 기둥 + 창(따뜻한 불빛)
        float px = W * 0.30f, pb = H * 0.14f;
        Color roof = new Color(0.03f, 0.03f, 0.07f), body = new Color(0.04f, 0.04f, 0.08f);
        Tri(new Vector2(px - 70, pb + 62), new Vector2(px + 70, pb + 62), new Vector2(px, pb + 118), roof, roof, roof);
        Quad(new Vector2(px - 52, pb), new Vector2(px + 52, pb), new Vector2(px + 52, pb + 62), new Vector2(px - 52, pb + 62), body);
        Color glowA = new Color(1f, 0.72f, 0.3f), glowB = new Color(0.9f, 0.4f, 0.1f);
        Quad(new Vector2(px - 28, pb + 8), new Vector2(px + 28, pb + 8), new Vector2(px + 28, pb + 48), new Vector2(px - 28, pb + 48), glowA, glowA, glowB, glowB);

        // 불티: 정자 위로 오르는 작은 주황 삼각형
        for (int i = 0; i < 14; i++)
        {
            float x = px + (Hash(i, 7f) - 0.5f) * 70f + Mathf.Sin(i) * 10f;
            float y = pb + 70f + Hash(i, 8f) * 140f;
            float s = 1.5f + Hash(i, 9f) * 2.5f;
            Color c = Color.Lerp(new Color(1f, 0.8f, 0.3f), new Color(1f, 0.35f, 0.1f), Hash(i, 10f));
            Tri(new Vector2(x, y + s * 1.6f), new Vector2(x - s, y), new Vector2(x + s, y), c, c, c);
        }
    }

    void Mountains(float baseY, float amp, Color cTop, Color cBase, int peaks, float salt)
    {
        float W = width;
        float step = W / peaks;
        for (int i = 0; i < peaks; i++)
        {
            float x0 = i * step - step * 0.25f, x1 = (i + 1) * step + step * 0.25f;
            float peakX = x0 + (x1 - x0) * (0.35f + 0.3f * Hash(i, salt));
            float peakY = baseY + amp * (0.5f + 0.5f * Hash(i, salt + 1f));
            Tri(new Vector2(x0, baseY - 10f), new Vector2(x1, baseY - 10f), new Vector2(peakX, peakY), cBase, cBase, cTop);
        }
    }

    // 0~1 난수 (항상 같은 그림)
    static float Hash(int k, float salt)
    {
        float x = Mathf.Sin(k * 12.9898f + salt * 78.233f) * 43758.5453f;
        return x - Mathf.Floor(x);
    }

    // 래스터라이저
    void Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color col) { Quad(a, b, c, d, col, col, col, col); }
    void Quad(Vector2 a, Vector2 b, Vector2 c, Vector2 d, Color ca, Color cb, Color cc, Color cd)
    {
        Tri(a, b, c, ca, cb, cc);
        Tri(a, c, d, ca, cc, cd);
    }

    // 삼각형 하나를 칠한다: 경계 상자 안의 픽셀마다 barycentric 좌표를 구해 안쪽이면 보간 색으로 칠함
    void Tri(Vector2 a, Vector2 b, Vector2 c, Color ca, Color cb, Color cc)
    {
        float denom = (b.y - c.y) * (a.x - c.x) + (c.x - b.x) * (a.y - c.y);
        if (Mathf.Abs(denom) < 1e-6f) return;

        int minX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x))));
        int maxX = Mathf.Min(width - 1, Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x))));
        int minY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y))));
        int maxY = Mathf.Min(height - 1, Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y))));

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float px = x + 0.5f, py = y + 0.5f;
                float w1 = ((b.y - c.y) * (px - c.x) + (c.x - b.x) * (py - c.y)) / denom;
                float w2 = ((c.y - a.y) * (px - c.x) + (a.x - c.x) * (py - c.y)) / denom;
                float w3 = 1f - w1 - w2;
                if (w1 < 0f || w2 < 0f || w3 < 0f) continue;

                Color col = ca * w1 + cb * w2 + cc * w3;
                pixels[y * width + x] = col;
            }
        }
    }

    // 액자
    // 이 오브젝트의 로컬 -Z 쪽이 그림 앞면. 크기: 가로 1.0 x 세로 0.69
    void BuildFrame(Texture2D tex)
    {
        float w = 1.0f, h = w * height / width, border = 0.06f, depth = 0.04f;

        // 그림: 직접 만든 사각형 메시 (정점 4개, UV 0~1)
        var pic = new GameObject("Picture", typeof(MeshFilter), typeof(MeshRenderer));
        pic.transform.SetParent(transform, false);
        var m = new Mesh { name = "Mid_PictureQuad" };
        m.vertices = new[]
        {
            new Vector3(-w / 2, -h / 2, 0), new Vector3(w / 2, -h / 2, 0),
            new Vector3(w / 2, h / 2, 0),   new Vector3(-w / 2, h / 2, 0),
        };
        m.uv = new[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
        m.triangles = new[] { 0, 2, 1, 0, 3, 2 };   // 앞면이 -Z 쪽
        m.RecalculateNormals();
        pic.GetComponent<MeshFilter>().mesh = m;

        var mat = new Material(pictureMaterial);
        mat.SetTexture("_BaseMap", tex);
        mat.SetTexture("_EmissionMap", tex);
        mat.SetColor("_EmissionColor", Color.white * 0.55f);
        mat.EnableKeyword("_EMISSION");
        pic.GetComponent<MeshRenderer>().material = mat;

        // 테두리 4개 + 뒷판 (큐브를 늘려서 사용)
        Bar("Top",    new Vector3(0,  h / 2 + border / 2, -depth / 2 + 0.001f), new Vector3(w + border * 2, border, depth));
        Bar("Bottom", new Vector3(0, -h / 2 - border / 2, -depth / 2 + 0.001f), new Vector3(w + border * 2, border, depth));
        Bar("Left",   new Vector3(-w / 2 - border / 2, 0, -depth / 2 + 0.001f), new Vector3(border, h, depth));
        Bar("Right",  new Vector3( w / 2 + border / 2, 0, -depth / 2 + 0.001f), new Vector3(border, h, depth));
        Bar("Back",   new Vector3(0, 0, depth / 2 + 0.002f), new Vector3(w + border * 2, h + border * 2, 0.02f));
    }

    void Bar(string name, Vector3 localPos, Vector3 size)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        Destroy(g.GetComponent<Collider>());
        g.transform.SetParent(transform, false);
        g.transform.localPosition = localPos;
        g.transform.localScale = size;
        if (woodMaterial != null) g.GetComponent<MeshRenderer>().sharedMaterial = woodMaterial;
    }
}
