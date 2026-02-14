using UnityEngine;
using UnityEditor;

public static class Create28Characters
{
    [MenuItem("Watermelon/Create 28 Character Data")]
    public static void CreateAll()
    {
        string folder = "Assets/ScriptableObjects";

        string[] names = {
            "Slime", "Goblin", "Skeleton", "Orc", "Minotaur",
            "Dragon", "Phoenix", "Unicorn", "Titan", "Valkyrie",
            "God", "Fairy", "Vampire", "Werewolf", "Golem",
            "Elf", "Dwarf", "Mermaid", "Demon", "Angel",
            "Sphinx", "Griffin", "Hydra", "Kraken", "Yeti",
            "Chimera", "Basilisk", "Leviathan"
        };

        Color[] colors = {
            Color.green, Color.cyan, Color.blue, Color.magenta,
            Color.red, new Color(1f, 0.5f, 0f), Color.yellow,
            new Color(0.5f, 0f, 1f), Color.white,
            new Color(1f, 0.84f, 0f), new Color(1f, 1f, 0.8f),
            new Color(0.8f, 0.5f, 1f), new Color(0.6f, 0f, 0f), new Color(0.4f, 0.3f, 0.2f),
            new Color(0.5f, 0.5f, 0.5f), new Color(0.3f, 0.8f, 0.3f), new Color(0.7f, 0.5f, 0.3f),
            new Color(0.2f, 0.6f, 0.9f), new Color(0.5f, 0f, 0.2f), new Color(1f, 1f, 0.6f),
            new Color(0.9f, 0.8f, 0.2f), new Color(0.8f, 0.6f, 0.2f), new Color(0.2f, 0.5f, 0.2f),
            new Color(0.1f, 0.3f, 0.6f), new Color(0.9f, 0.9f, 1f),
            new Color(0.7f, 0.2f, 0.3f), new Color(0.4f, 0.6f, 0.1f), new Color(0.1f, 0.2f, 0.4f)
        };

        CharacterData[] allData = new CharacterData[28];

        for (int i = 0; i < 28; i++)
        {
            string assetPath = $"{folder}/Char_{names[i]}.asset";
            CharacterData existing = AssetDatabase.LoadAssetAtPath<CharacterData>(assetPath);
            if (existing != null)
            {
                allData[i] = existing;
                // 이름이 다르면 업데이트
                if (existing.characterName != names[i])
                {
                    existing.characterName = names[i];
                    EditorUtility.SetDirty(existing);
                }
                continue;
            }

            // 기존 에셋 이름이 다른 패턴인지 확인 (레거시: Character_LvX_이름.asset)
            CharacterData data = ScriptableObject.CreateInstance<CharacterData>();
            data.characterName = names[i];
            data.mergeEffectColor = colors[i];

            AssetDatabase.CreateAsset(data, assetPath);
            allData[i] = data;
        }

        // CharacterDatabase 업데이트
        string dbPath = $"{folder}/CharacterDatabase.asset";
        CharacterDatabase db = AssetDatabase.LoadAssetAtPath<CharacterDatabase>(dbPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<CharacterDatabase>();
            AssetDatabase.CreateAsset(db, dbPath);
        }
        db.allCharacters = allData;
        db.levelCount = 11;
        db.minRadius = 0.2f;
        db.maxRadius = 0.8f;
        db.baseScore = 10;
        db.scorePerLevel = 10;
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();
    }
}
