using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

public class GameSetupWizard : EditorWindow
{
    [MenuItem("WatermelonGame/Setup Scene")]
    public static void SetupScene()
    {
        // 바로 실행 (MCP 호환)

        SetupCamera();
        CreateContainer();
        CreateBackground();
        CreateDropPoint();
        CreateGameOverLine();
        CreateCharacterPrefab();
        CreateMergeEffectPrefab();
        CreateAudioManager();
        CreateGameManager();
        CreateCanvas();

        Debug.Log("[수박게임] 씬 세팅 완료!  다음 단계:");
        Debug.Log("  1) WatermelonGame > Create Sample Character Data");
        Debug.Log("  2) WatermelonGame > Generate Placeholder Sprites");
        Debug.Log("  3) WatermelonGame > Auto-Link References");
        Debug.Log("  (또는 직접 캐릭터 스프라이트를 할당)");
    }

    private static void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        cam.orthographic = true;
        cam.orthographicSize = 6f;
        cam.transform.position = new Vector3(0f, 0f, -10f);
        cam.backgroundColor = new Color(0.95f, 0.92f, 0.85f);
    }

    private static void CreateContainer()
    {
        if (GameObject.Find("Container") != null) return;

        GameObject container = new GameObject("Container");
        ContainerSetup setup = container.AddComponent<ContainerSetup>();
        setup.width = 5.0f;
        setup.height = 7.5f;
        container.transform.position = Vector3.zero;

        Undo.RegisterCreatedObjectUndo(container, "Create Container");
    }

    private static void CreateBackground()
    {
        GameObject container = GameObject.Find("Container");
        if (container == null) return;
        if (container.transform.Find("Background") != null) return;

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(container.transform);
        bg.transform.localPosition = Vector3.zero;
        bg.AddComponent<BackgroundSetup>();

        Undo.RegisterCreatedObjectUndo(bg, "Create Background");
    }

    private static void CreateDropPoint()
    {
        if (GameObject.Find("DropPoint") != null) return;

        GameObject dropPoint = new GameObject("DropPoint");
        dropPoint.transform.position = new Vector3(0, 4.5f, 0);

        // 가이드 라인
        LineRenderer lr = dropPoint.AddComponent<LineRenderer>();
        lr.startWidth = 0.03f;
        lr.endWidth = 0.03f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = new Color(1f, 0f, 0f, 0.4f);
        lr.endColor = new Color(1f, 0f, 0f, 0.1f);
        lr.positionCount = 2;
        lr.sortingOrder = 10;

        dropPoint.AddComponent<DropGuide>();

        Undo.RegisterCreatedObjectUndo(dropPoint, "Create DropPoint");
    }

    private static void CreateGameOverLine()
    {
        if (GameObject.Find("GameOverLine") != null) return;

        GameObject line = new GameObject("GameOverLine");
        line.transform.position = new Vector3(0, 3.5f, 0);
        line.AddComponent<GameOverLine>();

        // 시각적 라인
        SpriteRenderer sr = line.AddComponent<SpriteRenderer>();
        Texture2D tex = new Texture2D(4, 4);
        Color[] colors = new Color[16];
        for (int i = 0; i < 16; i++) colors[i] = Color.white;
        tex.SetPixels(colors);
        tex.Apply();
        sr.sprite = Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4f);
        sr.color = new Color(1f, 0f, 0f, 0.3f);
        line.transform.localScale = new Vector3(5.4f, 0.05f, 1f);
        sr.sortingOrder = 5;

        Undo.RegisterCreatedObjectUndo(line, "Create GameOverLine");
    }

    private static void CreateCharacterPrefab()
    {
        string prefabPath = "Assets/Prefabs/Character.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) return;

        GameObject charObj = new GameObject("Character");
        SpriteRenderer sr = charObj.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 1;

        Rigidbody2D rb = charObj.AddComponent<Rigidbody2D>();
        rb.gravityScale = 3f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.sharedMaterial = CreateBounceMaterial();

        CircleCollider2D col = charObj.AddComponent<CircleCollider2D>();

        charObj.AddComponent<Character>();
        charObj.AddComponent<MergeHandler>();

        PrefabUtility.SaveAsPrefabAsset(charObj, prefabPath);
        Object.DestroyImmediate(charObj);
        Debug.Log("[수박게임] Character 프리팹 생성 완료");
    }

    private static PhysicsMaterial2D CreateBounceMaterial()
    {
        string matPath = "Assets/Prefabs/CharacterPhysics.physicsMaterial2D";
        PhysicsMaterial2D mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(matPath);
        if (mat != null) return mat;

        mat = new PhysicsMaterial2D("CharacterPhysics");
        mat.bounciness = 0.2f;
        mat.friction = 0.4f;
        AssetDatabase.CreateAsset(mat, matPath);
        return mat;
    }

    private static void CreateMergeEffectPrefab()
    {
        string prefabPath = "Assets/Prefabs/Effects/MergeEffect.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) return;

        GameObject fxObj = new GameObject("MergeEffect");
        fxObj.AddComponent<MergeEffect>();

        // 파티클 시스템
        GameObject particleObj = new GameObject("Particles");
        particleObj.transform.SetParent(fxObj.transform);
        ParticleSystem ps = particleObj.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.duration = 0.5f;
        main.startLifetime = 0.5f;
        main.startSpeed = 3f;
        main.startSize = 0.15f;
        main.maxParticles = 20;
        main.loop = false;
        main.playOnAwake = false;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 15) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius = 0.3f;

        // 플래시 스프라이트
        GameObject flashObj = new GameObject("Flash");
        flashObj.transform.SetParent(fxObj.transform);
        SpriteRenderer flashSR = flashObj.AddComponent<SpriteRenderer>();
        Texture2D tex = new Texture2D(32, 32);
        Color[] colors = new Color[32 * 32];
        // 원형 그라데이션
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                float dx = (x - 16f) / 16f;
                float dy = (y - 16f) / 16f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float alpha = Mathf.Clamp01(1f - dist);
                colors[y * 32 + x] = new Color(1, 1, 1, alpha);
            }
        }
        tex.SetPixels(colors);
        tex.Apply();
        flashSR.sprite = Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f), 32f);
        flashSR.sortingOrder = 5;

        // MergeEffect 연결 (Serialized Field는 프리팹 저장 후 수동 연결 필요)
        PrefabUtility.SaveAsPrefabAsset(fxObj, prefabPath);
        Object.DestroyImmediate(fxObj);
        Debug.Log("[수박게임] MergeEffect 프리팹 생성 완료");
    }

    private static void CreateAudioManager()
    {
        if (GameObject.Find("AudioManager") != null) return;

        GameObject audioObj = new GameObject("AudioManager");
        AudioManager am = audioObj.AddComponent<AudioManager>();

        // SFX Source
        GameObject sfxObj = new GameObject("SFX");
        sfxObj.transform.SetParent(audioObj.transform);
        sfxObj.AddComponent<AudioSource>();

        // Voice Source
        GameObject voiceObj = new GameObject("Voice");
        voiceObj.transform.SetParent(audioObj.transform);
        voiceObj.AddComponent<AudioSource>();

        // BGM Source
        GameObject bgmObj = new GameObject("BGM");
        bgmObj.transform.SetParent(audioObj.transform);
        AudioSource bgmSource = bgmObj.AddComponent<AudioSource>();
        bgmSource.loop = true;
        bgmSource.volume = 0.5f;

        Undo.RegisterCreatedObjectUndo(audioObj, "Create AudioManager");
    }

    private static void CreateGameManager()
    {
        if (GameObject.Find("GameManager") != null) return;

        GameObject gmObj = new GameObject("GameManager");
        GameManager gm = gmObj.AddComponent<GameManager>();

        Undo.RegisterCreatedObjectUndo(gmObj, "Create GameManager");
    }

    private static void CreateCanvas()
    {
        if (GameObject.Find("GameCanvas") != null) return;

        // Canvas
        GameObject canvasObj = new GameObject("GameCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        canvasObj.AddComponent<GraphicRaycaster>();

        // ===== 좌측: 점수 영역 =====
        GameObject leftPanel = CreatePanel(canvasObj.transform, "LeftScorePanel",
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(20, -20), new Vector2(220, 180));

        // 현재 점수 라벨
        CreateText(leftPanel.transform, "ScoreLabel", "SCORE",
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(10, -10), new Vector2(180, 30), 20, TextAlignmentOptions.Left);

        // 현재 점수 값
        CreateText(leftPanel.transform, "ScoreValue", "0",
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(10, -45), new Vector2(180, 50), 36, TextAlignmentOptions.Left);

        // 최고 점수 라벨
        CreateText(leftPanel.transform, "HighScoreLabel", "BEST",
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(10, -100), new Vector2(180, 30), 18, TextAlignmentOptions.Left);

        // 최고 점수 값
        CreateText(leftPanel.transform, "HighScoreValue", "0",
            new Vector2(0, 1), new Vector2(0, 1),
            new Vector2(10, -130), new Vector2(180, 40), 28, TextAlignmentOptions.Left);

        // ===== 우측 상단: 다음 캐릭터 =====
        GameObject rightTopPanel = CreatePanel(canvasObj.transform, "NextCharPanel",
            new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-20, -20), new Vector2(180, 160));

        CreateText(rightTopPanel.transform, "NextLabel", "NEXT",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -10), new Vector2(160, 30), 20, TextAlignmentOptions.Center);

        // 다음 캐릭터 이미지
        GameObject nextImgObj = new GameObject("NextCharImage");
        nextImgObj.transform.SetParent(rightTopPanel.transform, false);
        RectTransform nextImgRT = nextImgObj.AddComponent<RectTransform>();
        nextImgRT.anchorMin = new Vector2(0.5f, 0.5f);
        nextImgRT.anchorMax = new Vector2(0.5f, 0.5f);
        nextImgRT.anchoredPosition = new Vector2(0, -10);
        nextImgRT.sizeDelta = new Vector2(80, 80);
        Image nextImg = nextImgObj.AddComponent<Image>();
        nextImg.color = Color.white;

        // 다음 캐릭터 이름
        CreateText(rightTopPanel.transform, "NextCharName", "",
            new Vector2(0.5f, 0), new Vector2(0.5f, 0),
            new Vector2(0, 10), new Vector2(160, 25), 16, TextAlignmentOptions.Center);

        // ===== 우측: 진화 차트 =====
        GameObject rightPanel = CreatePanel(canvasObj.transform, "EvolutionPanel",
            new Vector2(1, 1), new Vector2(1, 1),
            new Vector2(-20, -190), new Vector2(180, 600));

        CreateText(rightPanel.transform, "EvolutionLabel", "EVOLUTION",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -5), new Vector2(160, 25), 16, TextAlignmentOptions.Center);

        // 진화 차트 스크롤 영역
        GameObject chartContent = new GameObject("ChartContent");
        chartContent.transform.SetParent(rightPanel.transform, false);
        RectTransform chartRT = chartContent.AddComponent<RectTransform>();
        chartRT.anchorMin = new Vector2(0, 0);
        chartRT.anchorMax = new Vector2(1, 1);
        chartRT.offsetMin = new Vector2(10, 10);
        chartRT.offsetMax = new Vector2(-10, -35);
        VerticalLayoutGroup vlg = chartContent.AddComponent<VerticalLayoutGroup>();
        vlg.spacing = 5;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childControlWidth = true;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        ContentSizeFitter csf = chartContent.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        // ===== 게임오버 패널 =====
        GameObject goPanel = CreatePanel(canvasObj.transform, "GameOverPanel",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            Vector2.zero, new Vector2(500, 300));
        Image goPanelImg = goPanel.GetComponent<Image>();
        if (goPanelImg != null)
            goPanelImg.color = new Color(0, 0, 0, 0.7f);

        CreateText(goPanel.transform, "GameOverText", "GAME OVER",
            new Vector2(0.5f, 1), new Vector2(0.5f, 1),
            new Vector2(0, -30), new Vector2(400, 60), 48, TextAlignmentOptions.Center);

        CreateText(goPanel.transform, "FinalScoreText", "FINAL SCORE: 0",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
            new Vector2(0, 10), new Vector2(400, 50), 30, TextAlignmentOptions.Center);

        // 재시작 버튼
        GameObject btnObj = new GameObject("RestartButton");
        btnObj.transform.SetParent(goPanel.transform, false);
        RectTransform btnRT = btnObj.AddComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(0.5f, 0);
        btnRT.anchorMax = new Vector2(0.5f, 0);
        btnRT.anchoredPosition = new Vector2(0, 50);
        btnRT.sizeDelta = new Vector2(200, 50);
        Image btnImg = btnObj.AddComponent<Image>();
        btnImg.color = new Color(0.2f, 0.7f, 0.3f, 1f);
        Button btn = btnObj.AddComponent<Button>();

        CreateText(btnObj.transform, "BtnText", "RESTART",
            new Vector2(0, 0), new Vector2(1, 1),
            Vector2.zero, Vector2.zero, 24, TextAlignmentOptions.Center, true);

        goPanel.SetActive(false);

        // UIManager 추가
        UIManager uiMgr = canvasObj.AddComponent<UIManager>();

        // 진화 엔트리 프리팹 생성
        CreateEvolutionEntryPrefab();

        Undo.RegisterCreatedObjectUndo(canvasObj, "Create GameCanvas");
        Debug.Log("[수박게임] UI Canvas 생성 완료. Inspector에서 UIManager 필드를 연결하세요.");
    }

    private static void CreateEvolutionEntryPrefab()
    {
        string prefabPath = "Assets/Prefabs/EvolutionEntry.prefab";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath) != null) return;

        GameObject entry = new GameObject("EvolutionEntry");
        RectTransform rt = entry.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(160, 45);
        HorizontalLayoutGroup hlg = entry.AddComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.padding = new RectOffset(5, 5, 2, 2);
        hlg.childControlWidth = false;
        hlg.childControlHeight = false;

        // 캐릭터 이미지
        GameObject imgObj = new GameObject("CharImage");
        imgObj.transform.SetParent(entry.transform);
        RectTransform imgRT = imgObj.AddComponent<RectTransform>();
        imgRT.sizeDelta = new Vector2(40, 40);
        Image img = imgObj.AddComponent<Image>();
        img.preserveAspect = true;

        // 레벨 텍스트
        GameObject txtObj = new GameObject("LevelText");
        txtObj.transform.SetParent(entry.transform);
        RectTransform txtRT = txtObj.AddComponent<RectTransform>();
        txtRT.sizeDelta = new Vector2(100, 40);
        TextMeshProUGUI tmp = txtObj.AddComponent<TextMeshProUGUI>();
        tmp.text = "Lv.0";
        tmp.fontSize = 16;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.color = Color.white;

        entry.AddComponent<EvolutionEntry>();

        PrefabUtility.SaveAsPrefabAsset(entry, prefabPath);
        Object.DestroyImmediate(entry);
        Debug.Log("[수박게임] EvolutionEntry 프리팹 생성 완료");
    }

    private static GameObject CreatePanel(Transform parent, string name,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = anchorMin; // pivot matches anchor corner
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Image img = panel.AddComponent<Image>();
        img.color = new Color(0.15f, 0.15f, 0.2f, 0.85f);
        return panel;
    }

    private static TextMeshProUGUI CreateText(Transform parent, string name, string text,
        Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size,
        float fontSize, TextAlignmentOptions alignment, bool stretch = false)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.AddComponent<RectTransform>();

        if (stretch)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
        else
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.anchoredPosition = position;
            rt.sizeDelta = size;
        }

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = alignment;
        tmp.color = Color.white;
        return tmp;
    }

    // ===== 캐릭터 데이터 자동 생성 =====
    [MenuItem("WatermelonGame/Create Sample Character Data")]
    public static void CreateSampleCharacterData()
    {
        string folder = "Assets/ScriptableObjects";

        // 기본 캐릭터 데이터 (레벨 0~10)
        string[] names = {
            "슬라임", "고블린", "스켈레톤", "오크", "미노타우로스",
            "드래곤", "피닉스", "유니콘", "타이탄", "발키리", "신"
        };
        float[] radii = {
            0.25f, 0.32f, 0.40f, 0.48f, 0.56f,
            0.65f, 0.75f, 0.85f, 0.95f, 1.05f, 1.20f
        };
        int[] scores = {
            1, 3, 6, 10, 15, 21, 28, 36, 45, 55, 66
        };

        Color[] effectColors = {
            Color.green, Color.cyan, Color.blue, Color.magenta,
            Color.red, new Color(1f, 0.5f, 0f), Color.yellow,
            new Color(0.5f, 0f, 1f), Color.white,
            new Color(1f, 0.84f, 0f), new Color(1f, 1f, 0.8f)
        };

        CharacterData[] allData = new CharacterData[names.Length];

        for (int i = 0; i < names.Length; i++)
        {
            string path = $"{folder}/Character_Lv{i}_{names[i]}.asset";
            CharacterData existing = AssetDatabase.LoadAssetAtPath<CharacterData>(path);
            if (existing != null)
            {
                allData[i] = existing;
                continue;
            }

            CharacterData data = ScriptableObject.CreateInstance<CharacterData>();
            data.characterName = names[i];
            data.mergeEffectColor = effectColors[i];
            // sprite, mergeSound, voiceClip은 나중에 수동 할당

            AssetDatabase.CreateAsset(data, path);
            allData[i] = data;
        }

        // CharacterDatabase 생성
        string dbPath = $"{folder}/CharacterDatabase.asset";
        CharacterDatabase db = AssetDatabase.LoadAssetAtPath<CharacterDatabase>(dbPath);
        if (db == null)
        {
            db = ScriptableObject.CreateInstance<CharacterDatabase>();
            AssetDatabase.CreateAsset(db, dbPath);
        }
        db.allCharacters = allData;
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();

        Debug.Log($"[수박게임] {names.Length}개 캐릭터 데이터 + Database 생성 완료!");
        Debug.Log("[수박게임] 각 CharacterData 에셋에 스프라이트, 효과음, 보이스를 할당하세요.");
    }

    // ===== 자동 연결 유틸 =====
    [MenuItem("WatermelonGame/Auto-Link References")]
    public static void AutoLinkReferences()
    {
        // GameManager 연결
        GameManager gm = Object.FindFirstObjectByType<GameManager>();
        if (gm != null)
        {
            var so = new SerializedObject(gm);

            // CharacterDatabase
            CharacterDatabase db = AssetDatabase.LoadAssetAtPath<CharacterDatabase>(
                "Assets/ScriptableObjects/CharacterDatabase.asset");
            if (db != null) so.FindProperty("characterDB").objectReferenceValue = db;

            // DropPoint
            GameObject dp = GameObject.Find("DropPoint");
            if (dp != null) so.FindProperty("dropPoint").objectReferenceValue = dp.transform;

            // ContainerParent
            GameObject container = GameObject.Find("Container");
            if (container != null) so.FindProperty("containerParent").objectReferenceValue = container.transform;

            // Prefabs
            GameObject charPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Character.prefab");
            if (charPrefab != null) so.FindProperty("characterPrefab").objectReferenceValue = charPrefab;

            GameObject fxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Effects/MergeEffect.prefab");
            if (fxPrefab != null) so.FindProperty("mergeEffectPrefab").objectReferenceValue = fxPrefab;

            so.ApplyModifiedProperties();
            Debug.Log("[수박게임] GameManager 레퍼런스 연결 완료");
        }

        // UIManager 연결
        UIManager ui = Object.FindFirstObjectByType<UIManager>();
        if (ui != null)
        {
            var so = new SerializedObject(ui);

            CharacterDatabase db = AssetDatabase.LoadAssetAtPath<CharacterDatabase>(
                "Assets/ScriptableObjects/CharacterDatabase.asset");
            if (db != null) so.FindProperty("characterDB").objectReferenceValue = db;

            // 점수 텍스트
            var scoreVal = GameObject.Find("ScoreValue");
            if (scoreVal != null)
                so.FindProperty("scoreText").objectReferenceValue =
                    scoreVal.GetComponent<TextMeshProUGUI>();

            var highVal = GameObject.Find("HighScoreValue");
            if (highVal != null)
                so.FindProperty("highScoreText").objectReferenceValue =
                    highVal.GetComponent<TextMeshProUGUI>();

            // 다음 캐릭터
            var nextImg = GameObject.Find("NextCharImage");
            if (nextImg != null)
                so.FindProperty("nextCharacterImage").objectReferenceValue =
                    nextImg.GetComponent<Image>();

            var nextName = GameObject.Find("NextCharName");
            if (nextName != null)
                so.FindProperty("nextCharacterName").objectReferenceValue =
                    nextName.GetComponent<TextMeshProUGUI>();

            // 진화 차트
            var chartContent = GameObject.Find("ChartContent");
            if (chartContent != null)
                so.FindProperty("evolutionChartParent").objectReferenceValue =
                    chartContent.transform;

            // 진화 엔트리 프리팹
            GameObject entryPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(
                "Assets/Prefabs/EvolutionEntry.prefab");
            if (entryPrefab != null)
                so.FindProperty("evolutionEntryPrefab").objectReferenceValue = entryPrefab;

            // 게임오버
            var goPanel = GameObject.Find("GameOverPanel");
            if (goPanel != null)
                so.FindProperty("gameOverPanel").objectReferenceValue = goPanel;

            var finalScore = GameObject.Find("FinalScoreText");
            if (finalScore != null)
                so.FindProperty("finalScoreText").objectReferenceValue =
                    finalScore.GetComponent<TextMeshProUGUI>();

            var restartBtn = GameObject.Find("RestartButton");
            if (restartBtn != null)
                so.FindProperty("restartButton").objectReferenceValue =
                    restartBtn.GetComponent<Button>();

            so.ApplyModifiedProperties();
            Debug.Log("[수박게임] UIManager 레퍼런스 연결 완료");
        }

        // AudioManager 연결
        AudioManager am = Object.FindFirstObjectByType<AudioManager>();
        if (am != null)
        {
            var so = new SerializedObject(am);

            var sfx = GameObject.Find("SFX");
            if (sfx != null)
                so.FindProperty("sfxSource").objectReferenceValue =
                    sfx.GetComponent<AudioSource>();

            var voice = GameObject.Find("Voice");
            if (voice != null)
                so.FindProperty("voiceSource").objectReferenceValue =
                    voice.GetComponent<AudioSource>();

            var bgm = GameObject.Find("BGM");
            if (bgm != null)
                so.FindProperty("bgmSource").objectReferenceValue =
                    bgm.GetComponent<AudioSource>();

            so.ApplyModifiedProperties();
            Debug.Log("[수박게임] AudioManager 레퍼런스 연결 완료");
        }

        // MergeEffect 프리팹 내부 연결
        string fxPath = "Assets/Prefabs/Effects/MergeEffect.prefab";
        GameObject fxPrefabObj = AssetDatabase.LoadAssetAtPath<GameObject>(fxPath);
        if (fxPrefabObj != null)
        {
            MergeEffect me = fxPrefabObj.GetComponent<MergeEffect>();
            if (me != null)
            {
                var meSO = new SerializedObject(me);

                Transform particles = fxPrefabObj.transform.Find("Particles");
                if (particles != null)
                    meSO.FindProperty("mergeParticles").objectReferenceValue =
                        particles.GetComponent<ParticleSystem>();

                Transform flash = fxPrefabObj.transform.Find("Flash");
                if (flash != null)
                    meSO.FindProperty("flashRenderer").objectReferenceValue =
                        flash.GetComponent<SpriteRenderer>();

                meSO.ApplyModifiedProperties();
                EditorUtility.SetDirty(fxPrefabObj);
                AssetDatabase.SaveAssets();
                Debug.Log("[수박게임] MergeEffect 프리팹 내부 연결 완료");
            }
        }

        Debug.Log("[수박게임] 전체 Auto-Link 완료!");
    }
}
