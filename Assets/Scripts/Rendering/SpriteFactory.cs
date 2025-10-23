using System.Collections.Generic;
using UnityEngine;





public static class SpriteFactory
{
    private static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public static Sprite CreateCircleSprite(Color color, int size = 64, float feather = 1f)
    {
        string key = $"circle_{size}_{color}_{feather}";
        if (cache.TryGetValue(key, out Sprite cached))
        {
            return cached;
        }

        Texture2D texture = new Texture2D(size, size, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        float radius = (size - feather) * 0.5f;
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= radius)
                {
                    texture.SetPixel(x, y, color);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        texture.Apply();

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, size, size),
            new Vector2(0.5f, 0.5f),
            size
        );
        sprite.name = key;

        cache[key] = sprite;
        return sprite;
    }

    public static Sprite CreateRoundedRectSprite(Color fillColor, Color borderColor, int width = 64, int height = 64, int borderSize = 4, int cornerRadius = 8)
    {
        string key = $"roundedRect_{width}_{height}_{fillColor}_{borderColor}_{borderSize}_{cornerRadius}";
        if (cache.TryGetValue(key, out Sprite cached))
        {
            return cached;
        }

        Texture2D texture = new Texture2D(width, height, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;

        Vector2 center = new Vector2(width / 2f, height / 2f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color pixelColor = fillColor;
                float distToEdgeX = Mathf.Min(x, width - 1 - x);
                float distToEdgeY = Mathf.Min(y, height - 1 - y);

                bool inBorder = distToEdgeX < borderSize || distToEdgeY < borderSize;

                if (cornerRadius > 0)
                {
                    Vector2 corner = new Vector2(
                        Mathf.Clamp(x, cornerRadius, width - 1 - cornerRadius),
                        Mathf.Clamp(y, cornerRadius, height - 1 - cornerRadius)
                    );
                    float distanceToCorner = Vector2.Distance(new Vector2(x, y), corner);
                    if (distanceToCorner > cornerRadius)
                    {
                        pixelColor = Color.clear;
                    }
                }

                if (inBorder)
                {
                    pixelColor = borderColor;
                }

                texture.SetPixel(x, y, pixelColor);
            }
        }

        texture.Apply();

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, width, height),
            new Vector2(0.5f, 0.5f),
            width
        );
        sprite.name = key;

        cache[key] = sprite;
        return sprite;
    }
}
