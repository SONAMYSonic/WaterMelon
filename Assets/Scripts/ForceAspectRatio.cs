using UnityEngine;

/// <summary>
/// 카메라 viewport rect를 조정하여 16:9 비율을 강제합니다.
/// BackgroundCamera의 검은 배경이 레터박스 역할을 합니다.
/// </summary>
public class ForceAspectRatio : MonoBehaviour
{
    [SerializeField] private float targetAspect = 16f / 9f;

    private Camera cam;
    private int lastScreenWidth;
    private int lastScreenHeight;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Start()
    {
        UpdateViewport();
    }

    private void Update()
    {
        // 해상도 변경 시에만 재계산
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            UpdateViewport();
        }
    }

    private void UpdateViewport()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;

        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        Rect rect;

        if (scaleHeight < 1.0f)
        {
            // 화면이 타겟보다 좁음 (세로로 긴 화면) → 상하 검은 바
            rect = new Rect(0f, (1f - scaleHeight) / 2f, 1f, scaleHeight);
        }
        else
        {
            // 화면이 타겟보다 넓음 → 좌우 검은 바
            float scaleWidth = 1f / scaleHeight;
            rect = new Rect((1f - scaleWidth) / 2f, 0f, scaleWidth, 1f);
        }

        cam.rect = rect;
    }
}
