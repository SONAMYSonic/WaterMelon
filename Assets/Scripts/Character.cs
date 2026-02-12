using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class Character : MonoBehaviour
{
    public int Level { get; private set; }
    public bool HasMerged { get; set; }
    public bool IsDropped { get; set; }

    private SpriteRenderer spriteRenderer;
    private CircleCollider2D circleCollider;
    private Rigidbody2D rb;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(CharacterData data, int level, float radius)
    {
        Level = level;
        spriteRenderer.sprite = data.characterSprite;

        float diameter = radius * 2f;
        // 스프라이트 크기에 맞게 스케일 조정
        if (data.characterSprite != null)
        {
            float spriteSize = data.characterSprite.bounds.size.x;
            float scale = diameter / spriteSize;
            transform.localScale = Vector3.one * scale;
        }

        circleCollider.radius = data.characterSprite != null
            ? data.characterSprite.bounds.extents.x
            : 0.5f;

        gameObject.name = $"Character_Lv{level}_{data.characterName}";
    }

    public void SetKinematic(bool isKinematic)
    {
        rb.bodyType = isKinematic ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;
        if (isKinematic)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    public void Drop()
    {
        IsDropped = true;
        SetKinematic(false);
    }
}
