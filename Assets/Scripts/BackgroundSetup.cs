using UnityEngine;

/// <summary>
/// 게임 배경 (컨테이너 내부 색상)
/// </summary>
public class BackgroundSetup : MonoBehaviour
{
    public Color backgroundColor = new Color(0.98f, 0.95f, 0.88f);

    private void Awake()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

        Texture2D tex = new Texture2D(4, 4);
        Color[] colors = new Color[16];
        for (int i = 0; i < 16; i++) colors[i] = Color.white;
        tex.SetPixels(colors);
        tex.Apply();

        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        sr.color = backgroundColor;
        sr.sortingOrder = -10;

        // Container 크기에 맞춤
        ContainerSetup container = GetComponentInParent<ContainerSetup>();
        if (container != null)
        {
            transform.localScale = new Vector3(container.width, container.height, 1f);
            transform.localPosition = Vector3.zero;
        }
    }
}
