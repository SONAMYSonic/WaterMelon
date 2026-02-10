using UnityEngine;

/// <summary>
/// 게임오버 라인. 캐릭터가 일정 시간 이상 이 라인 위에 있으면 게임오버.
/// </summary>
public class GameOverLine : MonoBehaviour
{
    [SerializeField] private float gameOverDelay = 2.0f;
    private float overLineTimer;
    private bool characterAboveLine;

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver) return;

        characterAboveLine = false;

        // containerParent 아래의 모든 캐릭터 확인
        Transform container = GameManager.Instance.containerParent;
        if (container == null) return;

        foreach (Transform child in container)
        {
            Character ch = child.GetComponent<Character>();
            if (ch != null && ch.IsDropped && !ch.HasMerged)
            {
                if (child.position.y > transform.position.y)
                {
                    characterAboveLine = true;
                    break;
                }
            }
        }

        if (characterAboveLine)
        {
            overLineTimer += Time.deltaTime;
            if (overLineTimer >= gameOverDelay)
            {
                GameManager.Instance.TriggerGameOver();
            }
        }
        else
        {
            overLineTimer = 0f;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawLine(
            transform.position + Vector3.left * 3f,
            transform.position + Vector3.right * 3f);
        Gizmos.DrawIcon(transform.position, "", true);
        // 라벨 대신 작은 큐브로 위치 표시
        Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
        Gizmos.DrawCube(transform.position, new Vector3(6f, 0.05f, 0));
    }
}
