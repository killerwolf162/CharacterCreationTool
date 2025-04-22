using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;
using UnityEngine.UIElements;

public static class PresetLoader
{

    private static readonly XmlSerializer xmlSerializer = new XmlSerializer(typeof(CharacterPreset));

    public static void LoadImagePreset(CharacterCreationUI UI, string destinationPath)
    {
        FileStream fileStream = null;

        try
        {
            fileStream = File.OpenRead(destinationPath);
            CharacterPreset preset = (CharacterPreset)xmlSerializer.Deserialize(fileStream);
            SetPreset(UI, preset);

        }
        catch (System.Exception e)
        {
            Debug.LogError(e.GetType() + " " + e.Message);
        }
        finally
        {
            if (fileStream != null) fileStream.Close();
        }

    }

    private static void SetPreset(CharacterCreationUI UI, CharacterPreset loadedPreset)
    {
        int imageCount = Mathf.Min(UI.imageNames.Count, loadedPreset.imageList.Count);

        for (int i = 0; i < imageCount; i++)
        {
            ImagePreset imagePreset = loadedPreset.imageList[i];

            UI.imageNames[i] = imagePreset.imageName; // set image name
            
            VisualElement element = UI.displayList[i];

            element.style.left = imagePreset.xPos; // set position
            element.style.top = imagePreset.yPos;
            element.style.width = imagePreset.width; //set size
            element.style.height = imagePreset.height;
        }
    }
}
