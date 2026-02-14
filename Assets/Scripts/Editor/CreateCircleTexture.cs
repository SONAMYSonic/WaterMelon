using UnityEngine;
using UnityEditor;
using System.IO;

public static class CreateCircleTexture
{
    [MenuItem("Tools/Create Circle Particle Texture")]
    public static void Create()
    {
        int size = 64;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        float center = size / 2f;
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(center, center));
                float t = Mathf.Clamp01(dist / radius);
                // 부드러운 원: 중심은 불투명, 바깥은 투명
                float alpha = 1f - Mathf.SmoothStep(0f, 1f, t);
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();
        byte[] pngData = tex.EncodeToPNG();
        string path = "Assets/Textures/CircleParticle.png";

        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllBytes(path, pngData);
        AssetDatabase.Refresh();

        // 텍스처 임포트 설정
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.filterMode = FilterMode.Bilinear;
            importer.SaveAndReimport();
        }

        // 파티클 머티리얼 생성
        string matPath = "Assets/Material/ParticleCircle.mat";
        Shader particleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (particleShader == null)
            particleShader = Shader.Find("Particles/Standard Unlit");

        if (particleShader != null)
        {
            Material mat = new Material(particleShader);
            Texture2D circTex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            mat.SetTexture("_BaseMap", circTex);
            mat.SetTexture("_MainTex", circTex);

            // Additive 블렌딩 설정
            mat.SetFloat("_Surface", 0); // Transparent
            mat.SetFloat("_Blend", 0);
            mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.One);
            mat.SetFloat("_ZWrite", 0);
            mat.renderQueue = 3000;
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.EnableKeyword("_ALPHAPREMULTIPLY_ON");

            AssetDatabase.CreateAsset(mat, matPath);
            AssetDatabase.SaveAssets();
        }
    }
}
