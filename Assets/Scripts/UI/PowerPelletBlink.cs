using UnityEngine;

public class PowerPelletBlink : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private float timer = 0f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogWarning("PowerPelletBlink requires a SpriteRenderer component.");
            enabled = false;
        }
    }

    void OnEnable()
    {
        timer = 0f;
    }

    void Update()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        timer += Time.deltaTime * 3f;
        float alpha = (Mathf.Sin(timer) + 1f) * 0.5f;
        alpha = Mathf.Lerp(0.5f, 1f, alpha);

        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}
