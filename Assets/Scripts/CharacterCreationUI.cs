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

        headName = headImages[0].name;
        chestName = chestImages[0].name;
        legName = legImages[0].name;
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

        ImageToExport = fuser.FuseImages(headImages[headIndex].texture, chestImages[chestIndex].texture, legImages[legIndex].texture, feetImages[feetIndex].texture);
        exporter.ExportImage(fileName.value, ImageToExport);
    }

    private void importButtonClicked()
    {

    }

    private void loadButtonClicked()
    {
        presetLoader.LoadImagePreset(this);

        SetImage(headDisplay, headImages, headName);
        SetImage(chestDisplay, chestImages, chestName);
        SetImage(legDisplay, legImages, legName);
        SetImage(feetDisplay, feetImages, feetName);
    }

    private void saveButtonClicked()
    {
        presetSaver.SaveImagePreset(this);
    }
    #endregion
}
