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
        headDisplay.AddManipulator(new ResizeHandler(headDisplay));
        chestDisplay.AddManipulator(new DragManipulator(chestDisplay));
        chestDisplay.AddManipulator(new ResizeHandler(chestDisplay));
        legDisplay.AddManipulator(new DragManipulator(legDisplay));
        legDisplay.AddManipulator(new ResizeHandler(legDisplay));
        feetDisplay.AddManipulator(new DragManipulator(feetDisplay));
        feetDisplay.AddManipulator(new ResizeHandler(feetDisplay));

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
        InitializeListView(headListView, headImages);
        InitializeListView(chestListView, chestImages);
        InitializeListView(legListView, legImages);
        InitializeListView(feetListView, feetImages);
        #endregion
    }

    private void InitializeListView(ListView listView, List<Sprite> sprites)
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
                    return;
                }

                imageCategoryList[i].Add(image);
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
        string fullPath = Path.Combine(Application.persistentDataPath, "Exports");
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
        StartCoroutine(ShowExportDialogCoroutine());
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

    IEnumerator ShowExportDialogCoroutine()
    {
        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Files, false, Application.persistentDataPath, null, "Select folder", "Export");
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            fileName.value = FileBrowser.Result[0];
            var headIndex = headImages.FindIndex(0, headImages.Count, s => s.name == headName);
            var chestIndex = chestImages.FindIndex(0, chestImages.Count, s => s.name == chestName);
            var legIndex = legImages.FindIndex(0, legImages.Count, s => s.name == legName);
            var feetIndex = feetImages.FindIndex(0, feetImages.Count, s => s.name == feetName);

            Texture2D[] textures = { headImages[headIndex].texture, chestImages[chestIndex].texture, legImages[legIndex].texture, feetImages[feetIndex].texture };
            ImageToExport = fuser.FuseImages(textures, headDisplay, chestDisplay, legDisplay, feetDisplay);
            exporter.ExportImage(fileName.value, ImageToExport);
        }
    }

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
            StartCoroutine(ShowSelectFolderDialogCoroutine(FileBrowser.Result));
    }

    IEnumerator ShowLoadDialogCoroutine()
    {
        FileBrowser.SetDefaultFilter(".xml");
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, Application.persistentDataPath, null, "Select File to load", "Load");

        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            presetLoader.LoadImagePreset(this, FileBrowser.Result[0]);
            SetImage(headDisplay, headImages, headName);
            SetImage(chestDisplay, chestImages, chestName);
            SetImage(legDisplay, legImages, legName);
            SetImage(feetDisplay, feetImages, feetName);
        }
    }

    IEnumerator ShowSaveDialogCoroutine()
    {
        FileBrowser.SetDefaultFilter(".xml");
        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Files, false, Application.persistentDataPath, null, "Select File to save", "Save");
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
            presetSaver.SaveImagePreset(this, FileBrowser.Result[0]);
    }

    private void ImportSelectedImages(string selectedFolder, string[] selectedFiles)
    {
        for (int i = 0; i < selectedFiles.Length; i++)
        {
            string filePath = selectedFiles[i];
            string destinationPath = Path.Combine(selectedFolder, FileBrowserHelpers.GetFilename(filePath));
            FileBrowserHelpers.CopyFile(filePath, destinationPath);
        }
        loader.LoadAllImages();
    }
    #endregion

}
