using UnityEngine;

/// <summary>
/// Character에 부착. 같은 레벨의 캐릭터와 충돌 시 합체를 요청한다.
/// instanceID가 작은 쪽에서만 호출하여 중복 방지.
/// </summary>
[RequireComponent(typeof(Character))]
public class MergeHandler : MonoBehaviour
{
    private Character self;

    private void Awake()
    {
        self = GetComponent<Character>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!self.IsDropped || self.HasMerged) return;

        Character other = collision.gameObject.GetComponent<Character>();
        if (other == null) return;
        if (!other.IsDropped || other.HasMerged) return;
        if (self.Level != other.Level) return;

        // instanceID가 작은 쪽에서만 처리 (중복 방지)
        if (self.GetInstanceID() > other.GetInstanceID()) return;

        GameManager.Instance.MergeCharacters(self, other);
    }
}
