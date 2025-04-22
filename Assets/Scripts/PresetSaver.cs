using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;

public static class PresetSaver
{

    private static readonly XmlSerializer xmlSerializer = new XmlSerializer(typeof(CharacterPreset));

    public static void SaveImagePreset(CharacterCreationUI UI, string destinationPath)
    {
        CharacterPreset characterPreset = new CharacterPreset();

        for (int i = 0; i < UI.imageNames.Count; i++)
        {
            characterPreset.imageList.Add(new ImagePreset());
        }

        FileStream fileStream = null;
        ApplyChanges(UI, characterPreset);     
        try
        {
            fileStream = File.Create(destinationPath);
            xmlSerializer.Serialize(fileStream, characterPreset);
            fileStream.Flush();
            Debug.Log($"Image exported successfully to: {destinationPath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message);
        }
        finally
        {
            if (fileStream != null) fileStream.Close();
        }
    }

    private static void ApplyChanges(CharacterCreationUI UI, CharacterPreset characterPreset)
    {
        int imageCount = Mathf.Min(UI.imageNames.Count, characterPreset.imageList.Count);

        for (int i = 0; i < UI.imageNames.Count; i++)
        {
            characterPreset.imageList[i].imageName = UI.imageNames[i]; //Set image name

            var style = UI.displayList[i].resolvedStyle;
            characterPreset.imageList[i].xPos = (int)style.left; // set position
            characterPreset.imageList[i].yPos = (int)style.top;
            characterPreset.imageList[i].width = (int)style.width; // set size
            characterPreset.imageList[i].height = (int)style.height;
        }
    }
}


