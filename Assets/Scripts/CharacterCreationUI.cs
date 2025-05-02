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
    public TextField fileName { get; private set; }
    public ListView headListView, chestListView, legListView, feetListView;

    public List<string> imageNames = new List<string>();
    public List<VisualElement> displayList = new List<VisualElement>();

    [SerializeField] private List<Sprite> headImages = new List<Sprite>();
    [SerializeField] private List<Sprite> chestImages = new List<Sprite>();
    [SerializeField] private List<Sprite> legImages = new List<Sprite>();
    [SerializeField] private List<Sprite> feetImages = new List<Sprite>();
    private List<List<Sprite>> imageCategoryList = new List<List<Sprite>>();

    private List<DragHandler> dragManList = new List<DragHandler>();
    private List<ResizeHandler> resHandList = new List<ResizeHandler>();

    private VisualElement headDisplay, chestDisplay, legDisplay, feetDisplay, setImageSizeDisplay;
    private Button exportButton, importHeadButton, importChestButton, importLegsButton, importFeetButton, loadButton, saveButton, methodButton, scaleModeButton, setImageSizeButton;
    private Foldout importFoldout, headSizeFoldout, chestSizeFoldout, legSizeFoldout, feetSizeFoldout;
    private IntegerField headWidthField, headHeightField, chestWidthField, chestHeightField, legWidthField, legHeightField, feetWidthField, feetHeightField;
    private Texture2D ImageToExport;

    private int currHeadWidth, currHeadHeight, currChestWidth, currChestHeight, currLegWidth, currLegHeight, currFeetWidth, currFeetHeight;

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
        importHeadButton = root.Q<Button>("importheadbutton");
        importChestButton = root.Q<Button>("importchestbutton");
        importLegsButton = root.Q<Button>("importlegsbutton");
        importFeetButton = root.Q<Button>("importfeetbutton");
        loadButton = root.Q<Button>("loadbutton");
        saveButton = root.Q<Button>("savebutton");
        methodButton = root.Q<Button>("methodselectionbutton");
        scaleModeButton = root.Q<Button>("scalemodeselectionbutton");
        setImageSizeButton = root.Q<Button>("setsizebutton");

        importFoldout = root.Q<Foldout>("importfoldout");
        importFoldout.value = false;
        headSizeFoldout = root.Q<Foldout>("headsizefoldout");
        headSizeFoldout.value = false;
        chestSizeFoldout = root.Q<Foldout>("chestsizefoldout");
        chestSizeFoldout.value = false;
        legSizeFoldout = root.Q<Foldout>("legsizefoldout");
        legSizeFoldout.value = false;
        feetSizeFoldout = root.Q<Foldout>("feetsizefoldout");
        feetSizeFoldout.value = false;

        headDisplay = root.Q<VisualElement>("headdisplay");
        chestDisplay = root.Q<VisualElement>("chestdisplay");
        legDisplay = root.Q<VisualElement>("legdisplay");
        feetDisplay = root.Q<VisualElement>("feetdisplay");
        setImageSizeDisplay = root.Q<VisualElement>("setimagesizedisplay");
        

        headListView = root.Q<ListView>("headlistview");
        chestListView = root.Q<ListView>("chestlistview");
        legListView = root.Q<ListView>("leglistview");
        feetListView = root.Q<ListView>("feetlistview");

        headWidthField = root.Q<IntegerField>("headsizewidthfield");
        headHeightField = root.Q<IntegerField>("headsizeheightfield");
        chestWidthField = root.Q<IntegerField>("chestsizewidthfield");
        chestHeightField = root.Q<IntegerField>("chestsizeheightfield");
        legWidthField = root.Q<IntegerField>("legsizewidthfield");
        legHeightField = root.Q<IntegerField>("legsizeheightfield");
        feetWidthField = root.Q<IntegerField>("feetsizewidthfield");
        feetHeightField = root.Q<IntegerField>("feetsizeheightfield");

        setImageSizeDisplay.style.visibility = Visibility.Hidden;

        dragManList.Add(new DragHandler(headDisplay));
        dragManList.Add(new DragHandler(chestDisplay));
        dragManList.Add(new DragHandler(legDisplay));
        dragManList.Add(new DragHandler(feetDisplay));
        dragManList.Add(new DragHandler(setImageSizeDisplay));

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

        setImageSizeDisplay.AddManipulator(new DragHandler(setImageSizeDisplay));

        fileName = root.Q<TextField>("filename");

        exportButton.clicked += exportButtonClicked;
        loadButton.clicked += loadButtonClicked;
        saveButton.clicked += saveButtonClicked;
        importHeadButton.clicked += importHeadButtonClicked;
        importChestButton.clicked += importChestButtonClicked;
        importLegsButton.clicked += importLegsButtonClicked;
        importFeetButton.clicked += importFeetButtonClicked;
        methodButton.clicked += methodButtonClicked;
        scaleModeButton.clicked += scaleModeButtonClicked;
        setImageSizeButton.clicked += setImageSizeButtonClicked;

        scaleModeButton.parent.style.display = DisplayStyle.None;

        headListView.selectionChanged += (items) => OnSelectionChanged(headListView, headImages, headDisplay, ref headName);
        chestListView.selectionChanged += (items) => OnSelectionChanged(chestListView, chestImages, chestDisplay, ref chestName);
        legListView.selectionChanged += (items) => OnSelectionChanged(legListView, legImages, legDisplay, ref legName);
        feetListView.selectionChanged += (items) => OnSelectionChanged(feetListView, feetImages, feetDisplay, ref feetName);

        headWidthField.RegisterValueChangedCallback(OnHeadsizeChanged);
        headHeightField.RegisterValueChangedCallback(OnHeadsizeChanged);

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

        imageNames.AddRange(new[] { headName, chestName, legName, feetName });
        displayList.AddRange(new[] { headDisplay, chestDisplay, legDisplay, feetDisplay });
        #endregion

        headWidthField.value = Mathf.RoundToInt(headDisplay.resolvedStyle.width);
        headHeightField.value = Mathf.RoundToInt(headDisplay.resolvedStyle.height);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            methodButtonClicked();
        }
        if (Input.GetKeyDown(KeyCode.LeftAlt) && currMethod == Method.scale)
        {
            scaleModeButtonClicked();
        }
        if (Input.GetKeyUp(KeyCode.LeftAlt) && currMethod == Method.scale)
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

    public void SetImage(VisualElement visElement, List<Sprite> spriteList, string imageName)
    {
        var index = spriteList.FindIndex(0, spriteList.Count, s => s.name == imageName);
        if (spriteList[index] == null)
            return;

        var texture = spriteList[index];
        visElement.style.backgroundImage = new StyleBackground(texture);
    }

    #region SizeChangeCallbacks

    private void OnHeadsizeChanged(ChangeEvent<int> evt)
    {

        if(evt.target == headWidthField)
        {
            currHeadWidth = evt.newValue;
        }
        if(evt.target == headHeightField)
        {
            currHeadHeight = evt.newValue;
        }

        UpdateHeadSize(currHeadWidth, currHeadHeight);
    }

    private void UpdateHeadSize(int width, int height)
    {
        headDisplay.style.width = width;
        headDisplay.style.height = height;
    }

    #endregion


    #region ButtonActions

    private void methodButtonClicked() // swap functionality of mouse 0 -> drag to scale and reverse
    {
        if (currMethod == Method.drag)
        {
            foreach (var dragMan in dragManList)
                dragMan.selected = false;
            foreach (var reszHand in resHandList)
                reszHand.selected = true;

            scaleModeButton.parent.style.display = DisplayStyle.Flex;
            methodButton.text = "Method: Scale";
            currMethod = Method.scale;

            return;
        }
        if (currMethod == Method.scale)
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

    private void setImageSizeButtonClicked()
    {
        if (setImageSizeDisplay.style.visibility == Visibility.Hidden)
            setImageSizeDisplay.style.visibility = Visibility.Visible;
        else if (setImageSizeDisplay.style.visibility == Visibility.Visible)
            setImageSizeDisplay.style.visibility = Visibility.Hidden;
    }

    private void scaleModeButtonClicked()
    {
        if (currMode == ResizeMode.topLeft)
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

    private void importHeadButtonClicked()
    {
        StartCoroutine(ShowImportDialogCoroutine("Heads"));
    }

    private void importChestButtonClicked()
    {
        StartCoroutine(ShowImportDialogCoroutine("Chest"));
    }

    private void importLegsButtonClicked()
    {
        StartCoroutine(ShowImportDialogCoroutine("Legs"));
    }

    private void importFeetButtonClicked()
    {
        StartCoroutine(ShowImportDialogCoroutine("Feet"));
    }

    private void loadButtonClicked()
    {
        StartCoroutine(ShowLoadDialogCoroutine());
    }

    private void saveButtonClicked()
    {
        imageNames[0] = headName;
        imageNames[1] = chestName;
        imageNames[2] = legName;
        imageNames[3] = feetName;

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

    private IEnumerator ShowSelectFolderDialogCoroutine(string[] filePaths, string folderName)
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Folders, false, Application.persistentDataPath, null, "Select folder", "Import");
        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
            ImportSelectedImages(FileBrowser.Result[0], filePaths);
    }

    private IEnumerator ShowImportDialogCoroutine(string folderName)
    {
        FileBrowser.SetDefaultFilter(".png");
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, true, "C:\\", null, "Select Files", "Select");

        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            Debug.Log($"Foldername: " + folderName);
            foreach (var item in FileBrowser.Result)
            {
                Debug.Log($"item name: " + item);
            }
            ImportSelectedImages(folderName, FileBrowser.Result);
        }
    }

    private IEnumerator ShowLoadDialogCoroutine()
    {
        FileBrowser.SetDefaultFilter(".xml");
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, false, Application.persistentDataPath, null, "Select File to load", "Load");

        Debug.Log(FileBrowser.Success);

        if (FileBrowser.Success)
        {
            PresetLoader.LoadImagePreset(this, FileBrowser.Result[0]);
            headName = imageNames[0]; // reset images to be loaded correctly
            chestName = imageNames[1];
            legName = imageNames[2];
            feetName = imageNames[3];
            SetImage(headDisplay, headImages, headName);
            SetImage(chestDisplay, chestImages, chestName);
            SetImage(legDisplay, legImages, legName);
            SetImage(feetDisplay, feetImages, feetName);

            // update curr variables so UI gets updated
            // currHeadWidth = headDisplay.resolvedstyle.width; etc.
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
            string root = Path.Combine(Application.persistentDataPath, selectedFolder);
            string destinationPath = Path.Combine(root, FileBrowserHelpers.GetFilename(filePath));
            Debug.Log($"destinationPath: " + destinationPath);

            FileBrowserHelpers.CopyFile(filePath, destinationPath);
        }
        loader.LoadAllImages();
    }
    #endregion

}
