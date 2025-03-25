using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;

public static class PresetSaver
{

    private static readonly XmlSerializer xmlSerializer = new XmlSerializer(typeof(ImagePreset));
    private static ImagePreset preset = new ImagePreset();

    public static void SaveImagePreset(CharacterCreationUI UI, string destinationPath)
    {
        FileStream fileStream = null;
        ApplyChanges(UI);

        try
        {
            fileStream = File.Create(destinationPath);
            xmlSerializer.Serialize(fileStream, preset);
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

    private static void ApplyChanges(CharacterCreationUI UI)
    {
        if (preset == null) preset = new ImagePreset();
        preset.headName = UI.headName;
        preset.chestName = UI.chestName;
        preset.legName = UI.legName;
        preset.feetName = UI.feetName;
    }
}


