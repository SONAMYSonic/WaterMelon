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

    public void PlayBig(Vector3 position, Color color, float size)
    {
        transform.position = position;

        float bigSize = size * 3f;

        if (mergeParticles != null)
        {
            var main = mergeParticles.main;
            main.startColor = color;
            main.startSize = bigSize * 0.5f;
            var emission = mergeParticles.emission;
            if (emission.burstCount > 0)
            {
                var burst = emission.GetBurst(0);
                burst.count = burst.count.constant * 3f;
                emission.SetBurst(0, burst);
            }
            mergeParticles.Play();
        }

        if (flashRenderer != null)
        {
            flashRenderer.color = new Color(1f, 0.95f, 0.5f, 0.9f); // 금색 플래시
            StartCoroutine(FlashRoutine(bigSize, 0.4f));
        }

        Destroy(gameObject, 3f);
    }

    private System.Collections.IEnumerator FlashRoutine(float size, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float scale = Mathf.Lerp(size * 0.5f, size * flashMaxScale * 1.5f, t);
            float alpha = Mathf.Lerp(0.9f, 0f, t);
            flashRenderer.transform.localScale = Vector3.one * scale;
            flashRenderer.color = new Color(
                flashRenderer.color.r, flashRenderer.color.g, flashRenderer.color.b, alpha);
            yield return null;
        }
        flashRenderer.gameObject.SetActive(false);
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
