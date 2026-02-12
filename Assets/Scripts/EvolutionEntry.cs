using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 진화 차트의 각 항목 UI
/// </summary>
public class EvolutionEntry : MonoBehaviour
{
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI levelText;

    public void Setup(CharacterData data, int level)
    {
        if (characterImage != null)
        {
            characterImage.sprite = data.characterSprite;
            characterImage.SetNativeSize();
            RectTransform rt = characterImage.rectTransform;
            float maxSize = 40f;
            if (rt.sizeDelta.x > maxSize || rt.sizeDelta.y > maxSize)
            {
                float ratio = maxSize / Mathf.Max(rt.sizeDelta.x, rt.sizeDelta.y);
                rt.sizeDelta *= ratio;
            }
        }

        if (levelText != null)
            levelText.text = $"Lv.{level}";
    }
}
