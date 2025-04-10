using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UIElements;
using SimpleFileBrowser;

public class CharacterCreationUI : MonoBehaviour
{
    [SerializeField] public string headName, chestName, legName, feetName;

    private List<List<Sprite>> imageCategoryList = new List<List<Sprite>>();
    [SerializeField] private List<Sprite> headImages = new List<Sprite>();
    [SerializeField] private List<Sprite> chestImages = new List<Sprite>();
    [SerializeField] private List<Sprite> legImages = new List<Sprite>();
    [SerializeField] private List<Sprite> feetImages = new List<Sprite>();
    private List<DragManipulator> dragManList = new List<DragManipulator>();
    private List<ResizeHandler> resHandList = new List<ResizeHandler>();

    public TextField fileName { get; private set; }
    public ListView headListView, chestListView, legListView, feetListView;

    private VisualElement headDisplay, chestDisplay, legDisplay, feetDisplay;
    private Button exportButton, importButton, loadButton, saveButton, methodButton, scaleModeButton;
    private Texture2D ImageToExport;

    private ImageLoader loader;

    private Method currMethod = Method.drag;
    private ResizeMode currMode = ResizeMode.topLeft;

    private enum Method
    {
        drag,
        scale
    }

    private enum ResizeMode
    {
        topLeft,
        center
    }

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
        #endregion

        #region UI Init

        var root = GetComponent<UIDocument>().rootVisualElement;

        exportButton = root.Q<Button>("exportbutton");
        importButton = root.Q<Button>("importbutton");
        loadButton = root.Q<Button>("loadbutton");
        saveButton = root.Q<Button>("savebutton");
        methodButton = root.Q<Button>("methodselectionbutton");
        scaleModeButton = root.Q<Button>("scalemodeselectionbutton");

        headDisplay = root.Q<VisualElement>("headdisplay");
        chestDisplay = root.Q<VisualElement>("chestdisplay");
        legDisplay = root.Q<VisualElement>("legdisplay");
        feetDisplay = root.Q<VisualElement>("feetdisplay");

        headListView = root.Q<ListView>("headlistview");
        chestListView = root.Q<ListView>("chestlistview");
        legListView = root.Q<ListView>("leglistview");
        feetListView = root.Q<ListView>("feetlistview");

        dragManList.Add(new DragManipulator(headDisplay));
        dragManList.Add(new DragManipulator(chestDisplay));
        dragManList.Add(new DragManipulator(legDisplay));
        dragManList.Add(new DragManipulator(feetDisplay));

        resHandList.Add(new ResizeHandler(headDisplay));
        resHandList.Add(new ResizeHandler(chestDisplay));
        resHandList.Add(new ResizeHandler(legDisplay));
        resHandList.Add(new ResizeHandler(feetDisplay));

        headDisplay.AddManipulator(dragManList[0]);
        headDisplay.AddManipulator(resHandList[0]);
        chestDisplay.AddManipulator(dragManList[1]);
        chestDisplay.AddManipulator(resHandList[1]);
        legDisplay.AddManipulator(dragManList[2]);
        legDisplay.AddManipulator(resHandList[2]);
        feetDisplay.AddManipulator(dragManList[3]);
        feetDisplay.AddManipulator(resHandList[3]);

        fileName = root.Q<TextField>("filename");

        exportButton.clicked += exportButtonClicked;
        loadButton.clicked += loadButtonClicked;
        saveButton.clicked += saveButtonClicked;
        importButton.clicked += importButtonClicked;
        methodButton.clicked += methodButtonClicked;
        scaleModeButton.clicked += scaleModeButtonClicked;

        scaleModeButton.parent.style.display = DisplayStyle.None;

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

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            methodButtonClicked();
        }
        if(Input.GetKeyDown(KeyCode.LeftAlt) && currMethod == Method.scale)
        {
            scaleModeButtonClicked();
        }
        if(Input.GetKeyUp(KeyCode.LeftAlt) && currMethod == Method.scale)
        {
            scaleModeButtonClicked();
        }
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

    private void methodButtonClicked() // swap functionality of mouse 0 -> drag to scale and reverse
    {
        if (currMethod == Method.drag)
        {
            foreach(var dragMan in dragManList)
                dragMan.selected = false;
            foreach (var reszHand in resHandList)
                reszHand.selected = true;

            scaleModeButton.parent.style.display = DisplayStyle.Flex;
            methodButton.text = "Method: Scale";
            currMethod = Method.scale;

            return;
        }
        if(currMethod == Method.scale)
        {
            foreach (var dragMan in dragManList)
                dragMan.selected = true;
            foreach (var reszHand in resHandList)
                reszHand.selected = false;
            scaleModeButton.parent.style.display = DisplayStyle.None;
            methodButton.text = "Method: Move";
            currMethod = Method.drag;
            return;
        }
    }

    private void scaleModeButtonClicked()
    {
        if(currMode == ResizeMode.topLeft)
        {
            foreach (var reszHand in resHandList)
                reszHand.altPressed = true;
            scaleModeButton.text = "Mode: Center";

            currMode = ResizeMode.center;
            return;
        }
        if (currMode == ResizeMode.center)
        {
            foreach (var reszHand in resHandList)
                reszHand.altPressed = false;
            scaleModeButton.text = "Mode: Default";

            currMode = ResizeMode.topLeft;
            return;
        }
    }

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

    private IEnumerator ShowExportDialogCoroutine()
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
            VisualElement[] elements = { headDisplay, chestDisplay, legDisplay, feetDisplay };

            ImageToExport = ImageFuser.FuseImages(textures, elements);
            ImageExporter.ExportImage(fileName.value, ImageToExport);
        }
    }

    private IEnumerator ShowSelectFolderDialogCoroutine(string[] filePaths)
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, false, Application.persistentDataPath, null, "Select folder", "Import");
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
            ImportSelectedImages(FileBrowser.Result[0], filePaths);
    }

    private IEnumerator ShowImportDialogCoroutine()
    {
        FileBrowser.SetDefaultFilter(".png");
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, true, "C:\\", null, "Select Files", "Select");

        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
            StartCoroutine(ShowSelectFolderDialogCoroutine(FileBrowser.Result));
    }

    private IEnumerator ShowLoadDialogCoroutine()
    {
        FileBrowser.SetDefaultFilter(".xml");
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, Application.persistentDataPath, null, "Select File to load", "Load");

        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            PresetLoader.LoadImagePreset(this, FileBrowser.Result[0]);
            SetImage(headDisplay, headImages, headName);
            SetImage(chestDisplay, chestImages, chestName);
            SetImage(legDisplay, legImages, legName);
            SetImage(feetDisplay, feetImages, feetName);
        }
    }

    private IEnumerator ShowSaveDialogCoroutine()
    {
        FileBrowser.SetDefaultFilter(".xml");
        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Files, false, Application.persistentDataPath, null, "Select File to save", "Save");
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
            PresetSaver.SaveImagePreset(this, FileBrowser.Result[0]);
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
