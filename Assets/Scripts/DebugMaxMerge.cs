#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;

/// <summary>
/// 디버그용: F9 키를 누르면 MAX 합체 축하 이벤트 발동
/// 에디터 및 Development Build에서만 동작, 릴리즈 빌드에서 자동 제외
/// </summary>
public class DebugMaxMerge : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F9))
        {
            if (GameManager.Instance != null)
                GameManager.Instance.TriggerMaxMergeCelebration();
        }
    }
}
#endif
