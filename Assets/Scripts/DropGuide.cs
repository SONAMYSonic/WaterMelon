using UnityEngine;

/// <summary>
/// 드롭 위치를 표시하는 가이드 라인
/// </summary>
public class DropGuide : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private float lineLength = 8f;

    private void Awake()
    {
        if (lineRenderer == null)
            lineRenderer = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.IsGameOver)
        {
            lineRenderer.enabled = false;
            return;
        }

        lineRenderer.enabled = true;
        Vector3 top = transform.position;
        Vector3 bottom = top - Vector3.up * lineLength;
        lineRenderer.SetPosition(0, top);
        lineRenderer.SetPosition(1, bottom);
    }

    private void OnDrawGizmos()
    {
        // 드롭 포인트 위치 표시
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.15f);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * lineLength);
    }
}
