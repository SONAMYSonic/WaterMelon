using UnityEngine;
using UnityEditor;

public class PlaceholderSpriteGenerator : EditorWindow
{
    [MenuItem("WatermelonGame/Generate Placeholder Sprites")]
    public static void Generate()
    {
        string folder = "Assets/Sprites/Characters";

        Color[] colors = {
            new Color(0.6f, 1f, 0.6f),   // Lv0 연두
            new Color(0.4f, 0.8f, 1f),   // Lv1 하늘
            new Color(0.4f, 0.4f, 1f),   // Lv2 파랑
            new Color(0.8f, 0.4f, 1f),   // Lv3 보라
            new Color(1f, 0.4f, 0.6f),   // Lv4 분홍
            new Color(1f, 0.3f, 0.3f),   // Lv5 빨강
            new Color(1f, 0.6f, 0.2f),   // Lv6 주황
            new Color(1f, 0.9f, 0.2f),   // Lv7 노랑
            new Color(1f, 1f, 1f),       // Lv8 흰색
            new Color(0.9f, 0.8f, 0.2f), // Lv9 금색
            new Color(1f, 0.95f, 0.7f),  // Lv10 빛
        };

        string[] names = {
            "슬라임", "고블린", "스켈레톤", "오크", "미노타우로스",
            "드래곤", "피닉스", "유니콘", "타이탄", "발키리", "신"
        };

        for (int i = 0; i < colors.Length; i++)
        {
            int size = 64 + i * 16; // 레벨이 높을수록 큰 텍스처
            Texture2D tex = CreateCircleTexture(size, colors[i]);

            byte[] png = tex.EncodeToPNG();
            string path = $"{folder}/Character_Lv{i}.png";
            System.IO.File.WriteAllBytes(
                System.IO.Path.Combine(Application.dataPath, "..", path), png);
        }

        AssetDatabase.Refresh();

        // ScriptableObject에 스프라이트 자동 할당
        for (int i = 0; i < names.Length; i++)
        {
            string spritePath = $"{folder}/Character_Lv{i}.png";
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

            string dataPath = $"Assets/ScriptableObjects/Character_Lv{i}_{names[i]}.asset";
            CharacterData data = AssetDatabase.LoadAssetAtPath<CharacterData>(dataPath);
            if (data != null && sprite != null)
            {
                data.characterSprite = sprite;
                EditorUtility.SetDirty(data);
            }
        }

        AssetDatabase.SaveAssets();
    }

    private static Texture2D CreateCircleTexture(int size, Color color)
    {
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size / 2f;
        float radius = size / 2f - 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist <= radius)
                {
                    // 간단한 셰이딩
                    float shade = 1f - (dist / radius) * 0.3f;
                    float highlight = Mathf.Max(0, 1f - Mathf.Abs(dist - radius * 0.3f) / (radius * 0.5f));
                    Color c = color * shade + Color.white * highlight * 0.15f;
                    c.a = 1f;
                    tex.SetPixel(x, y, c);
                }
                else if (dist <= radius + 1.5f)
                {
                    // 안티앨리어싱 테두리
                    float alpha = 1f - (dist - radius) / 1.5f;
                    Color c = color * 0.7f;
                    c.a = alpha;
                    tex.SetPixel(x, y, c);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }

        tex.Apply();
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }
}
