using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;

public class PresetSaver
{

    private XmlSerializer xmlSerializer;
    private ImagePreset preset;

    public PresetSaver()
    {
        xmlSerializer = new XmlSerializer(typeof(ImagePreset));
    }

    public void SaveImagePreset(CharacterCreationUI UI)
    {
        FileStream fileStream = null;
        ApplyChanges(UI);

        try
        {
            string directoryPath = Path.Combine(Application.persistentDataPath, "Presets");

            if (!Directory.Exists(directoryPath))
                Directory.CreateDirectory(directoryPath);

            string filePath = Path.Combine(directoryPath, preset.name + ".xml");
            fileStream = File.Create(filePath);
            xmlSerializer.Serialize(fileStream, preset);
            fileStream.Flush();
            Debug.Log($"Image exported successfully to: {filePath}");
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

    private void ApplyChanges(CharacterCreationUI UI)
    {
        if (preset == null) preset = new ImagePreset();
        preset.name = UI.fileName.text;
        preset.headIndex = UI.headIndex;
        preset.chestIndex = UI.chestIndex;
        preset.legIndex = UI.legIndex;
        preset.feetIndex = UI.feetIndex;
    }
}


