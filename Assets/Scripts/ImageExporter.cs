using UnityEngine;
using System.IO;

public class ImageExporter : MonoBehaviour
{
    public Texture2D textureToExport;

    public void ExportImage(string fileName = "testExport.png")
    {
        if (textureToExport == null)
        {
            Debug.LogError("No texture assigned to export!");
            return;
        }

        try
        {
            byte[] bytes = textureToExport.EncodeToPNG();
            string directoryPath = Path.Combine(Application.persistentDataPath, "Exports");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            string filePath = Path.Combine(directoryPath, fileName);
            File.WriteAllBytes(filePath, bytes);

            Debug.Log($"Image exported successfully to: {filePath}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to export image: {e.Message}");
        }
    }
}
