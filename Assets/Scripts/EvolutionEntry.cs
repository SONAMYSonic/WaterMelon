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

    private const float ImageSize = 110f;

    public void Setup(CharacterData data, int level, int maxLevel)
    {
        if (characterImage != null)
        {
            characterImage.sprite = data.characterSprite;
            characterImage.preserveAspect = true;

            RectTransform imgRT = characterImage.rectTransform;
            imgRT.sizeDelta = new Vector2(ImageSize, ImageSize);
        }

        if (levelText != null)
            levelText.text = $"Lv.{level}";
    }
}
