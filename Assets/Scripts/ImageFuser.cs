using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ImageFuser
{

    public Texture2D FuseImages(Texture2D[] textures, VisualElement headImage, VisualElement chestImage, VisualElement legImage, VisualElement feetImage)
    {

        Texture2D resizedHead = ResizeImage(textures[0], ((int)headImage.resolvedStyle.width), (int)headImage.resolvedStyle.height);
        headImage.style.backgroundImage = resizedHead;
        Texture2D resizedChest = ResizeImage(textures[1], (int)chestImage.resolvedStyle.width, (int)chestImage.resolvedStyle.height);
        Texture2D resizedLeg = ResizeImage(textures[2], (int)legImage.resolvedStyle.width, (int)legImage.resolvedStyle.height);
        Texture2D resizedFeet = ResizeImage(textures[3], (int)feetImage.resolvedStyle.width, (int)feetImage.resolvedStyle.height);

        int width = resizedChest.width;
        int height = resizedHead.height + resizedChest.height + resizedLeg.height + resizedFeet.height;
        Texture2D combinedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false);

        // Fill with transparent pixels first
        Color[] clearPixels = new Color[width * height];
        for (int i = 0; i < clearPixels.Length; i++)
        {
            clearPixels[i] = Color.clear;
        }
        combinedTexture.SetPixels(clearPixels);

        int currentY = height;

        currentY -= resizedHead.height;
        CopyTextureRegion(combinedTexture, resizedHead, 0, currentY);

        currentY -= resizedChest.height;
        CopyTextureRegion(combinedTexture, resizedChest, 0, currentY);

        currentY -= resizedLeg.height;
        CopyTextureRegion(combinedTexture, resizedLeg, 0, currentY);

        currentY -= resizedFeet.height;
        CopyTextureRegion(combinedTexture, resizedFeet, 0, currentY);

        combinedTexture.Apply();

        //resizedHead = null;
        //resizedChest = null;
        //resizedLeg = null;
        //resizedFeet = null;

        return combinedTexture;
    }

    private static void CopyTextureRegion(Texture2D target, Texture2D source, int startX, int startY)
    {
        Color[] sourcePixels = source.GetPixels();
        target.SetPixels(startX, startY, source.width, source.height, sourcePixels);
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
