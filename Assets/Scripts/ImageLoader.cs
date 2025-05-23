using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using UnityEngine.UIElements;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using System;
using System.Linq;
using UnityEngine.Events;

public class ImageLoader : MonoBehaviour
{
    [System.Serializable]
    public class ImageCategory
    {
        public string folderName;
        public List<Sprite> imageList = new List<Sprite>();
        public bool autoReload = false;
    }

    public List<ImageCategory> imageCategories = new List<ImageCategory>();
    private Dictionary<string, System.DateTime> lastCheckTimes = new Dictionary<string, System.DateTime>();
    private float autoReloadInterval = 1f;
    private readonly string[] supportedFormats = { "*.png", "*.jpg", "*.jpeg" };


    public UnityEvent onImageLoaded = new UnityEvent();

    private void Awake()
    {
        foreach (var category in imageCategories)
        {
            string fullPath = Path.Combine(Application.persistentDataPath, category.folderName);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            lastCheckTimes[category.folderName] = System.DateTime.Now;
        }

        LoadAllImages();
    }

    private void Update()
    {
        foreach (var category in imageCategories)
        {
            if (category.autoReload &&
                (System.DateTime.Now - lastCheckTimes[category.folderName]).TotalSeconds >= autoReloadInterval)
            {
                LoadImagesForCategory(category);
                lastCheckTimes[category.folderName] = System.DateTime.Now;
            }
        }
    }

    public void LoadAllImages()
    {
        foreach (var category in imageCategories)
        {
            LoadImagesForCategory(category);
        }
        onImageLoaded.Invoke();
    }

    private void LoadImagesForCategory(ImageCategory category)
    {
        string fullPath = Path.Combine(Application.persistentDataPath, category.folderName);

        if (!Directory.Exists(fullPath))
        {
            Debug.LogError($"Directory not found: {fullPath}");
            return;
        }

        List<string> imageFiles = new List<string>();
        foreach (string format in supportedFormats)
        {
            imageFiles.AddRange(Directory.GetFiles(fullPath, format));
        }

        if (category.imageList.Count > 0)
        {
            var currentFileNames = category.imageList.Select(s => s.name).ToList();
            var newFileNames = imageFiles.Select(f => Path.GetFileNameWithoutExtension(f)).ToList();

            if (currentFileNames.SequenceEqual(newFileNames))
            {
                return;
            }
        }

        category.imageList.Clear();

        foreach (string imagePath in imageFiles)
        {
            try
            {
                byte[] imageData = File.ReadAllBytes(imagePath);
                Texture2D texture = new Texture2D(2, 2);

                if (texture.LoadImage(imageData))
                {
                    Sprite newSprite = Sprite.Create(
                        texture,
                        new Rect(0, 0, texture.width, texture.height),
                        new Vector2(0.5f, 0.5f),
                        100.0f
                    );

                    newSprite.name = Path.GetFileNameWithoutExtension(imagePath);
                    category.imageList.Add(newSprite);
                }
                else
                {
                    Debug.LogError($"Failed to load image: {imagePath}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error loading image {imagePath}: {e.Message}");
            }
        }

        Debug.Log($"Loaded {category.imageList.Count} images from {category.folderName}");
        onImageLoaded.Invoke();
    }
}
