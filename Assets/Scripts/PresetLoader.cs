using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;

public class PresetLoader
{

    private XmlSerializer xmlSerializer;

    public PresetLoader()
    {
        xmlSerializer = new XmlSerializer(typeof(ImagePreset));
    }

    public void LoadImagePreset(CharacterCreationUI UI)
    {
        FileStream fileStream = null;

        try
        {
            string directoryPath = Path.Combine(Application.persistentDataPath, "Presets");
            if (!Directory.Exists(directoryPath))
                Debug.Log("No Preset Directory");

            string filePath = Path.Combine(directoryPath, UI.fileName.text + ".xml");
            fileStream = File.OpenRead(filePath);
            ImagePreset preset = (ImagePreset)xmlSerializer.Deserialize(fileStream);

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

    private void SetPreset(CharacterCreationUI UI, ImagePreset preset)
    {
        UI.headName = preset.headName;
        UI.chestName = preset.chestName;
        UI.legName = preset.legName;
        UI.feetName = preset.feetName;

        UI.fileName.SetValueWithoutNotify(preset.name);
    }
}
