using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ImageFuser
{

    public Texture2D FuseImages(Texture2D[] textures, VisualElement headImage, VisualElement chestImage, VisualElement legImage, VisualElement feetImage)
    {

        List<(VisualElement element, Texture2D texture)> visElements = new List<(VisualElement element, Texture2D texture)>()
        {
            (headImage, textures[0]),
            (chestImage, textures[1]),
            (legImage, textures[2]),
            (feetImage, textures[3])
        };

        List<(VisualElement element, Texture2D resizedTexture, Vector2 position)> resizedElements =
            new List<(VisualElement, Texture2D, Vector2)>();

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (var (element, texture) in visElements)
        {
            Vector2 position = element.worldBound.position;

            Texture2D resized = ResizeImage(texture, (int)element.resolvedStyle.width, (int)element.resolvedStyle.height);

            resizedElements.Add((element, resized, position));

            minX = Mathf.Min(minX, position.x);
            minY = Mathf.Min(minY, position.y);
            maxX = Mathf.Max(maxX, position.x + element.resolvedStyle.width);
            maxY = Mathf.Max(maxY, position.y + element.resolvedStyle.height);
        }

        int width = (int)(maxX - minX);
        int height = (int)(maxY - minY);

        Texture2D combinedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Color[] clearPixels = new Color[width * height];
        for (int i = 0; i < clearPixels.Length; i++)
        {
            clearPixels[i] = Color.clear;
        }
        combinedTexture.SetPixels(clearPixels);

        resizedElements.Reverse();
        foreach (var (element, resizedTexture, position) in resizedElements)
        {
            int relX = (int)(position.x - minX);
            int relY = (int)(position.y - minY);

            int yPos = height - relY - resizedTexture.height;

            CopyTextureRegion(combinedTexture, resizedTexture, relX, yPos);
        }

        combinedTexture.Apply();
        return combinedTexture;
    }

    private static void CopyTextureRegion(Texture2D target, Texture2D source, int startX, int startY)
    {
        Color[] sourcePixels = source.GetPixels();

        for (int y = 0; y < source.height; y++)
        {
            for (int x = 0; x < source.width; x++)
            {
                int sourceIndex = y * source.width + x;
                Color pixel = sourcePixels[sourceIndex];
                if (pixel.a > 0.01f)
                {
                    target.SetPixel(startX + x, startY + y, pixel);
                }
            }
        }
    }

    private static Texture2D ResizeImage(Texture2D source, int targetWidth, int targetHeight)
    {
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
        rt.filterMode = FilterMode.Bilinear;

        RenderTexture.active = rt;
        Graphics.Blit(source, rt);

        Texture2D resized = new Texture2D(targetWidth, targetHeight, TextureFormat.RGBA32, false);
        resized.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        resized.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return resized;
    }

}
