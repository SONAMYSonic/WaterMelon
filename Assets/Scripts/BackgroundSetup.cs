using UnityEngine;

/// <summary>
/// 게임 배경 (컨테이너 내부)
/// Inspector에서 backgroundSprite에 원하는 이미지를 넣으면 해당 이미지가 배경으로 사용됩니다.
/// 비워두면 기존처럼 단색 배경이 적용됩니다.
/// </summary>
[ExecuteInEditMode]
public class BackgroundSetup : MonoBehaviour
{
    [Header("배경 설정")]
    [Tooltip("배경에 사용할 스프라이트 (비워두면 단색 배경)")]
    [SerializeField] private Sprite backgroundSprite;

    [Tooltip("배경 색상 (스프라이트가 없을 때 사용, 스프라이트가 있으면 틴트 색상)")]
    public Color backgroundColor = new Color(0.98f, 0.95f, 0.88f);

    [Tooltip("스프라이트 사용 시 원본 색상 유지 (true = 틴트 없음)")]
    [SerializeField] private bool keepOriginalColor = true;

    private void Awake()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();

        if (backgroundSprite != null)
        {
            sr.sprite = backgroundSprite;
            sr.color = keepOriginalColor ? Color.white : backgroundColor;
        }
        else
        {
            Texture2D tex = new Texture2D(4, 4);
            Color[] colors = new Color[16];
            for (int i = 0; i < 16; i++) colors[i] = Color.white;
            tex.SetPixels(colors);
            tex.Apply();
            sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
            sr.color = backgroundColor;
        }

        sr.sortingOrder = -10;

        ContainerSetup container = GetComponentInParent<ContainerSetup>();
        if (container != null)
        {
            float containerWidth = container.width;
            float containerHeight = container.height;

            if (backgroundSprite != null)
            {
                // 스프라이트 크기를 컨테이너에 맞추기
                float spriteW = backgroundSprite.bounds.size.x;
                float spriteH = backgroundSprite.bounds.size.y;
                transform.localScale = new Vector3(
                    containerWidth / spriteW,
                    containerHeight / spriteH,
                    1f);
            }
            else
            {
                transform.localScale = new Vector3(containerWidth, containerHeight, 1f);
            }
            // 배경 중심 = 컨테이너 중심 (local 0,0)
            transform.localPosition = Vector3.zero;
        }
    }
}
