using UnityEngine;

public class MergeEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem mergeParticles;
    [SerializeField] private SpriteRenderer flashRenderer;
    [SerializeField] private float flashDuration = 0.15f;
    [SerializeField] private float flashMaxScale = 2f;

    public void Play(Vector3 position, Color color, float size)
    {
        transform.position = position;

        // 파티클 이펙트
        if (mergeParticles != null)
        {
            var main = mergeParticles.main;
            main.startColor = color;
            main.startSize = size * 0.3f;
            mergeParticles.Play();
        }

        // 플래시 이펙트
        if (flashRenderer != null)
        {
            flashRenderer.color = new Color(color.r, color.g, color.b, 0.8f);
            StartCoroutine(FlashRoutine(size));
        }

        Destroy(gameObject, 2f);
    }

    private System.Collections.IEnumerator FlashRoutine(float size)
    {
        float elapsed = 0f;
        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;
            float scale = Mathf.Lerp(size * 0.5f, size * flashMaxScale, t);
            float alpha = Mathf.Lerp(0.8f, 0f, t);
            flashRenderer.transform.localScale = Vector3.one * scale;
            flashRenderer.color = new Color(
                flashRenderer.color.r,
                flashRenderer.color.g,
                flashRenderer.color.b,
                alpha);
            yield return null;
        }
        flashRenderer.gameObject.SetActive(false);
    }
}
