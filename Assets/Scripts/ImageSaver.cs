using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;

public class ImageSaver : MonoBehaviour
{

    private XmlSerializer xmlSerializer;
    private ImagePreset preset;

    public void SaveImagePreset(string fileName, int headIndex, int chestIndex, int legIndex, int feetIndex)
    {
        preset = new ImagePreset(fileName, headIndex, chestIndex, legIndex, feetIndex);
        FileStream fileStream = null;
        
        try
        {
            string directoryPath = Path.Combine(Application.persistentDataPath, "Presets");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string filePath = Path.Combine(directoryPath, fileName);
            fileStream = File.Create(filePath);
            xmlSerializer.Serialize(fileStream, preset);
            fileStream.Flush();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message, gameObject);
        }
        finally
        {
            if (fileStream != null) fileStream.Close();
        }
    }
}
