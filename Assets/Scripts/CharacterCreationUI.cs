using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using UnityEngine.UIElements;
using System.Xml.Serialization;
using UnityEngine.Events;
using SimpleFileBrowser;

public class CharacterCreationUI : MonoBehaviour
{
    [SerializeField] public string headName, chestName, legName, feetName;

    public TextField fileName { get; private set; }

    private VisualElement headDisplay, chestDisplay, legDisplay, feetDisplay;

    public ListView headListView, chestListView, legListView, feetListView;

    private Button exportButton, importButton, loadButton, saveButton;

    private Texture2D ImageToExport;

    private List<List<Sprite>> imageCategoryList = new List<List<Sprite>>();
    [SerializeField] private List<Sprite> headImages = new List<Sprite>();
    [SerializeField] private List<Sprite> chestImages = new List<Sprite>();
    [SerializeField] private List<Sprite> legImages = new List<Sprite>();
    [SerializeField] private List<Sprite> feetImages = new List<Sprite>();

    private ImageLoader loader;
    private ImageExporter exporter;
    private PresetSaver presetSaver;
    private PresetLoader presetLoader;
    private ImageFuser fuser;

    private void Awake()
    {
        #region Setup SFB

        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".jpg", ".png"), new FileBrowser.Filter("Text Files", ".xml"));

        #endregion

        #region Fill list
        imageCategoryList.Add(headImages);
        imageCategoryList.Add(chestImages);
        imageCategoryList.Add(legImages);
        imageCategoryList.Add(feetImages);
        #endregion

        #region Load&Save&Export
        loader = GetComponent<ImageLoader>();
        loader.onImageLoaded.AddListener(UpdateList);
        exporter = new ImageExporter();
        presetSaver = new PresetSaver();
        presetLoader = new PresetLoader();
        fuser = new ImageFuser();
        #endregion

        #region UI Init

        var root = GetComponent<UIDocument>().rootVisualElement;

        exportButton = root.Q<Button>("exportbutton");
        importButton = root.Q<Button>("importbutton");
        loadButton = root.Q<Button>("loadbutton");
        saveButton = root.Q<Button>("savebutton");

        headDisplay = root.Q<VisualElement>("headdisplay");
        chestDisplay = root.Q<VisualElement>("chestdisplay");
        legDisplay = root.Q<VisualElement>("legdisplay");
        feetDisplay = root.Q<VisualElement>("feetdisplay");

        headListView = root.Q<ListView>("headlistview");
        chestListView = root.Q<ListView>("chestlistview");
        legListView = root.Q<ListView>("leglistview");
        feetListView = root.Q<ListView>("feetlistview");

        headDisplay.AddManipulator(new DragManipulator(headDisplay));
        chestDisplay.AddManipulator(new DragManipulator(chestDisplay));
        legDisplay.AddManipulator(new DragManipulator(legDisplay));
        feetDisplay.AddManipulator(new DragManipulator(feetDisplay));

        fileName = root.Q<TextField>("filename");

        exportButton.clicked += exportButtonClicked;
        loadButton.clicked += loadButtonClicked;
        saveButton.clicked += saveButtonClicked;
        importButton.clicked += importButtonClicked;

        headListView.selectionChanged += (items) => OnSelectionChanged(headListView, headImages, headDisplay, ref headName);
        chestListView.selectionChanged += (items) => OnSelectionChanged(chestListView, chestImages, chestDisplay, ref chestName);
        legListView.selectionChanged += (items) => OnSelectionChanged(legListView, legImages, legDisplay, ref legName);
        feetListView.selectionChanged += (items) => OnSelectionChanged(feetListView, feetImages, feetDisplay, ref feetName);

        #endregion

    }

    private void Start()
    {
        #region SetImagesAtStart
        UpdateList();

        if (headImages.Count > 0)
            headName = headImages[0].name;
        if (chestImages.Count > 0)
            chestName = chestImages[0].name;
        if (legImages.Count > 0)
            legName = legImages[0].name;
        if (feetImages.Count > 0)
            feetName = feetImages[0].name;

        if (headImages.Count > 0)
            SetImage(headDisplay, headImages, headName);
        if (chestImages.Count > 0)
            SetImage(chestDisplay, chestImages, chestName);
        if (legImages.Count > 0)
            SetImage(legDisplay, legImages, legName);
        if (feetImages.Count > 0)
            SetImage(feetDisplay, feetImages, feetName);
        #endregion

        #region InitializeListView
        InitializeListView(headListView, headImages, headDisplay);
        InitializeListView(chestListView, chestImages, chestDisplay);
        InitializeListView(legListView, legImages, legDisplay);
        InitializeListView(feetListView, feetImages, feetDisplay);
        #endregion
    }

    private void InitializeListView(ListView listView, List<Sprite> sprites, VisualElement visElem)
    {
        listView.makeItem = () =>
        {
            var container = new VisualElement();
            container.style.width = new Length(100, LengthUnit.Pixel);
            container.style.height = new Length(100, LengthUnit.Pixel);
            container.style.marginTop = 2;
            container.style.marginBottom = 2;
            container.style.alignItems = Align.Center;

            var image = new Image();
            image.style.width = new Length(100, LengthUnit.Percent);
            image.style.height = new Length(100, LengthUnit.Percent);
            image.scaleMode = ScaleMode.ScaleToFit;

            container.Add(image);
            container.AddManipulator(new DragManipulator(container));
            return container;
        };

        listView.bindItem = (element, index) =>
        {
            var container = element;
            var image = container.Q<Image>();

            if (index < sprites.Count)
            {
                image.sprite = sprites[index];
            }
        };

        listView.itemsSource = sprites;
        listView.fixedItemHeight = 100;
        listView.selectionType = SelectionType.Single;
    }

    private void OnSelectionChanged(ListView listview, List<Sprite> sprites, VisualElement visElem, ref string name)
    {
        var selectedItem = listview.selectedItem as Sprite;
        name = selectedItem.name;
        SetImage(visElem, sprites, name);
    }

    public void RebuildListView()
    {
        headListView.Rebuild();
        chestListView.Rebuild();
        legListView.Rebuild();
        feetListView.Rebuild();
    }

    public void UpdateList()
    {
        for (int i = 0; i < imageCategoryList.Count; i++)
        {
            imageCategoryList[i].Clear();

            foreach (var image in loader.imageCategories[i].imageList)
            {
                if (loader.imageCategories[i].imageList.Count == 0)
                {
                    Debug.Log("Image folder empty");
                    return;
                }

                imageCategoryList[i].Add(image);
                Debug.Log($"Copied{image.name} to {imageCategoryList[i]}");
                Debug.Log("Lists updated");
            }
        }

    }

    private void SetImage(VisualElement visElement, List<Sprite> spriteList, string imageName)
    {
        var index = spriteList.FindIndex(0, spriteList.Count, s => s.name == imageName);
        if (spriteList[index] == null)
            return;

        var texture = spriteList[index];
        visElement.style.backgroundImage = new StyleBackground(texture);
    }

    #region ButtonActions
    private void exportButtonClicked()
    {
        var headIndex = headImages.FindIndex(0, headImages.Count, s => s.name == headName);
        var chestIndex = chestImages.FindIndex(0, chestImages.Count, s => s.name == chestName);
        var legIndex = legImages.FindIndex(0, legImages.Count, s => s.name == legName);
        var feetIndex = feetImages.FindIndex(0, feetImages.Count, s => s.name == feetName);

        ImageToExport = fuser.FuseImages(headDisplay, chestDisplay, legDisplay, feetDisplay);
        exporter.ExportImage(fileName.value, ImageToExport);
    }

    private void importButtonClicked()
    {
        StartCoroutine(ShowImportDialogCoroutine());
    }

    private void loadButtonClicked()
    {
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    private void saveButtonClicked()
    {
        string fullPath = Path.Combine(Application.persistentDataPath, "Presets");
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
        StartCoroutine(ShowSaveDialogCoroutine());
    }
    #endregion

    #region SFB functions

    IEnumerator ShowSelectFolderDialogCoroutine(string[] filePaths)
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, false, Application.persistentDataPath, null, "Select folder", "Import");
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
            ImportSelectedImages(FileBrowser.Result[0], filePaths);
    }

    IEnumerator ShowImportDialogCoroutine()
    {
        FileBrowser.SetDefaultFilter(".png");
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, true, "C:\\", null, "Select Files", "Select");

        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
            OnFilesSelected(FileBrowser.Result, 3);
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        FileBrowser.SetDefaultFilter(".xml");
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, Application.persistentDataPath, null, "Select File to load", "Load");

        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
            OnFilesSelected(FileBrowser.Result, 1);
    }

    IEnumerator ShowSaveDialogCoroutine()
    {
        FileBrowser.SetDefaultFilter(".xml");
        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Files, false, Application.persistentDataPath, null, "Select File to save", "Save");
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
            OnFilesSelected(FileBrowser.Result, 2);
    }

    private void ImportSelectedImages(string selectedFolder, string[] selectedFiles)
    {
        for (int i = 0; i < selectedFiles.Length; i++)
        {
            string filePath = selectedFiles[i];
            Debug.Log(filePath);
            string destinationPath = Path.Combine(selectedFolder, FileBrowserHelpers.GetFilename(filePath));
            Debug.Log(destinationPath);
            FileBrowserHelpers.CopyFile(filePath, destinationPath);
        }
        loader.LoadAllImages();
    }

    void OnFilesSelected(string[] filePaths, int state)
    {
        for (int i = 0; i < filePaths.Length; i++)
            Debug.Log(filePaths[i]);

        string filePath = filePaths[0];

        if (state == 1) //loading preset
        {
            presetLoader.LoadImagePreset(this, filePath);
            SetImage(headDisplay, headImages, headName);
            SetImage(chestDisplay, chestImages, chestName);
            SetImage(legDisplay, legImages, legName);
            SetImage(feetDisplay, feetImages, feetName);
        }

        if (state == 2) //saving preset
        {
            presetSaver.SaveImagePreset(this, filePath);
        }

        if (state == 3) //importing images
        {
            StartCoroutine(ShowSelectFolderDialogCoroutine(filePaths));
        }
    }
    #endregion

}
