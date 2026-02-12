using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 진화 차트의 각 항목 UI
/// 이미지(위) + 레벨 텍스트(아래) 세로 배치
/// 레벨에 따라 캐릭터 이미지 크기가 달라짐
/// </summary>
public class EvolutionEntry : MonoBehaviour
{
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI levelText;

    // 레벨별 이미지 크기 범위
    private const float MinImageSize = 50f;
    private const float MaxImageSize = 110f;

    public void Setup(CharacterData data, int level, int maxLevel)
    {
        if (characterImage != null)
        {
            characterImage.sprite = data.characterSprite;
            characterImage.preserveAspect = true;

            // 레벨에 비례하여 이미지 크기 결정
            float t = maxLevel > 0 ? (float)level / maxLevel : 0f;
            float imageSize = Mathf.Lerp(MinImageSize, MaxImageSize, t);

            RectTransform imgRT = characterImage.rectTransform;
            imgRT.sizeDelta = new Vector2(imageSize, imageSize);
        }

        if (levelText != null)
            levelText.text = $"Lv.{level}";
    }
}
