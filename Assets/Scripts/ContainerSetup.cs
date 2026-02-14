using UnityEngine;

/// <summary>
/// 수박게임 컨테이너 경계 정보.
/// 벽(Floor, LeftWall, RightWall)은 씬에 직접 배치되어 있으며,
/// 이 스크립트는 GameManager 등에서 경계를 읽을 수 있도록 프로퍼티만 제공한다.
/// </summary>
public class ContainerSetup : MonoBehaviour
{
    [Header("Container Size")]
    [Tooltip("컨테이너 내부 폭")]
    public float width = 5f;
    [Tooltip("컨테이너 내부 높이")]
    public float height = 7.5f;

    // 외부에서 컨테이너 경계를 읽을 수 있게 프로퍼티 제공
    public float FloorY => -height / 2f;
    public float LeftX => -width / 2f;
    public float RightX => width / 2f;

    private void OnDrawGizmos()
    {
        Vector3 pos = transform.position;
        float halfW = width / 2f;
        float halfH = height / 2f;

        // 윤곽선
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(pos, new Vector3(width, height, 0));
    }
}
