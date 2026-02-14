using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LeaderboardRow : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankText;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private Image backgroundImage;

    [Header("Front (순위 배경)")]
    [SerializeField] private Image frontImage;
    [SerializeField] private Sprite rank1Sprite;
    [SerializeField] private Sprite rank2Sprite;
    [SerializeField] private Sprite rank3Sprite;
    [SerializeField] private Sprite defaultFrontSprite;

    public void Setup(int rank, string playerName, string score)
    {
        if (nameText != null) nameText.text = playerName;
        if (scoreText != null) scoreText.text = score;

        // 1~3위: 전용 스프라이트, 순위 텍스트 숨김
        if (rank >= 1 && rank <= 3)
        {
            if (rankText != null) rankText.gameObject.SetActive(false);
            if (frontImage != null)
            {
                frontImage.gameObject.SetActive(true);
                frontImage.sprite = rank == 1 ? rank1Sprite
                                  : rank == 2 ? rank2Sprite
                                  : rank3Sprite;
            }
        }
        // 4위 이하: 공통 스프라이트, 순위 텍스트 표시
        else if (rank > 3)
        {
            if (rankText != null)
            {
                rankText.gameObject.SetActive(true);
                rankText.text = rank.ToString();
            }
            if (frontImage != null)
            {
                frontImage.gameObject.SetActive(true);
                frontImage.sprite = defaultFrontSprite;
            }
        }
        // rank 0 (데이터 없음)
        else
        {
            if (rankText != null)
            {
                rankText.gameObject.SetActive(true);
                rankText.text = "-";
            }
            if (frontImage != null)
                frontImage.sprite = defaultFrontSprite;
        }
    }
}
