using UnityEngine;

/// <summary>
/// 수박게임 컨테이너(바닥 + 좌우 벽)를 자동 생성한다.
/// FHD 기준 중앙 배치.
/// </summary>
public class ContainerSetup : MonoBehaviour
{
    [Header("Container Dimensions (World Units)")]
    public float width = 5.0f;
    public float height = 7.5f;
    public float wallThickness = 0.2f;

    [Header("Appearance")]
    public Color wallColor = new Color(0.3f, 0.2f, 0.1f, 1f);
    public int sortingOrder = -1;

    private void Awake()
    {
        CreateWall("Floor", new Vector3(0, -height / 2f, 0),
            new Vector2(width + wallThickness * 2, wallThickness));
        CreateWall("LeftWall", new Vector3(-width / 2f - wallThickness / 2f, 0, 0),
            new Vector2(wallThickness, height + wallThickness));
        CreateWall("RightWall", new Vector3(width / 2f + wallThickness / 2f, 0, 0),
            new Vector2(wallThickness, height + wallThickness));
    }

    private void CreateWall(string wallName, Vector3 localPos, Vector2 size)
    {
        GameObject wall = new GameObject(wallName);
        wall.transform.SetParent(transform);
        wall.transform.localPosition = localPos;

        BoxCollider2D col = wall.AddComponent<BoxCollider2D>();
        col.size = size;

        SpriteRenderer sr = wall.AddComponent<SpriteRenderer>();
        sr.sprite = CreateSquareSprite();
        sr.color = wallColor;
        sr.sortingOrder = sortingOrder;
        wall.transform.localScale = new Vector3(size.x, size.y, 1f);
    }

    private Sprite CreateSquareSprite()
    {
        Texture2D tex = new Texture2D(4, 4);
        Color[] colors = new Color[16];
        for (int i = 0; i < 16; i++) colors[i] = Color.white;
        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
    }

    private void OnDrawGizmos()
    {
        Vector3 pos = transform.position;
        float hw = width / 2f;
        float hh = height / 2f;
        float wt = wallThickness;

        // 배경 영역
        Gizmos.color = new Color(0.98f, 0.95f, 0.88f, 0.3f);
        Gizmos.DrawCube(pos, new Vector3(width, height, 0));

        // 벽
        Gizmos.color = wallColor;
        // Floor
        Gizmos.DrawCube(pos + new Vector3(0, -hh, 0), new Vector3(width + wt * 2, wt, 0.1f));
        // Left
        Gizmos.DrawCube(pos + new Vector3(-hw - wt / 2f, 0, 0), new Vector3(wt, height + wt, 0.1f));
        // Right
        Gizmos.DrawCube(pos + new Vector3(hw + wt / 2f, 0, 0), new Vector3(wt, height + wt, 0.1f));

        // 윤곽선
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(pos, new Vector3(width, height, 0));
    }
}
