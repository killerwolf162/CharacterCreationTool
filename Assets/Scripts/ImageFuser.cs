using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ImageFuser : MonoBehaviour
{

    public Texture2D FuseImages(Texture2D headImage, Texture2D chestImage, Texture2D legImage, Texture2D feetImage)
    {
        int width = Mathf.Max(chestImage.width);
        int height = Mathf.Max(headImage.height + chestImage.height, legImage.height + feetImage.height);

        var fusedImage = new Texture2D(width, height, TextureFormat.RGBA32, false);

        Color[] clearPixels = new Color[width * height];
        for (int i = 0; i < clearPixels.Length; i++)
        {
            clearPixels[i] = Color.clear;
        }
        fusedImage.SetPixels(clearPixels);

        CopyTextureRegion(fusedImage, headImage, 0, 0);

        CopyTextureRegion(fusedImage, chestImage, 0, height-headImage.height);

        CopyTextureRegion(fusedImage, legImage, 0, height - headImage.height - chestImage.height);

        CopyTextureRegion(fusedImage, feetImage, 0, height - headImage.height -chestImage.height - legImage.height);



        return fusedImage;
    }

    private static void CopyTextureRegion(Texture2D target, Texture2D source, int startX, int startY)
    {
        Color[] sourcePixels = source.GetPixels();
        target.SetPixels(startX, startY, source.width, source.height, sourcePixels);
    }
}
