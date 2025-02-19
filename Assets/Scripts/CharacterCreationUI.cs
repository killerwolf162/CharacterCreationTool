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
    

    [SerializeField] public int headIndex = 1, chestIndex = 1, legIndex = 1, feetIndex = 1;

    public TextField fileName { get; private set; }

    private VisualElement headDisplay, chestDisplay, legDisplay, feetDisplay;

    public ListView headListView, chestListView, legListView, feetListView;

    private Button exportButton, loadButton, saveButton;

    private Texture2D ImageToExport;

    private List<List<Sprite>> imageCategoryList = new List<List<Sprite>>();
    private List<Sprite> headImages = new List<Sprite>();
    private List<Sprite> chestImages = new List<Sprite>();
    private List<Sprite> legImages = new List<Sprite>();
    private List<Sprite> feetImages = new List<Sprite>();

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
        exporter = GetComponent<ImageExporter>();
        presetSaver = GetComponent<PresetSaver>();
        presetLoader = GetComponent<PresetLoader>();
        fuser = GetComponent<ImageFuser>();
        #endregion

        #region UI Init

        var root = GetComponent<UIDocument>().rootVisualElement;

        exportButton = root.Q<Button>("exportbutton");
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
        #endregion
    }

    private void Start()
    {
        #region SetImagesAtStart
        if (headImages.Count > 0)
            SetImage(headDisplay, headImages, headIndex);
        if (chestImages.Count > 0)
            SetImage(chestDisplay, chestImages, chestIndex);
        if (legImages.Count > 0)
            SetImage(legDisplay, legImages, legIndex);
        if (feetImages.Count > 0)
            SetImage(feetDisplay, feetImages, feetIndex);
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

    public void RebuildListView(ListView listView, List<Sprite> sprites)
    {
        sprites.Clear();
        listView.Rebuild();
    }

    public void UpdateList()
    {
        for (int i = 0; i < imageCategoryList.Count; i++)
        {
            foreach (var image in loader.imageCategories[i].imageList)
            {
                if (loader.imageCategories[i].imageList.Count == 0)
                {
                    Debug.Log("Image folder empty");
                    return;
                }

                List<Sprite> listToCopyTo = imageCategoryList[i];
                if (!listToCopyTo.Contains(image))
                {
                    listToCopyTo.Add(image);
                    Debug.Log($"Copied{image.name} to {listToCopyTo}");
                }
                Debug.Log("Lists updated");
            }
        }
    }

    private int GetSpriteIndex(ListView listView)
    {
        return listView.selectedIndex;
    }

    private void SetImage(VisualElement visElement, List<Sprite> spriteList, int index)
    {
        var texture = spriteList[index];
        visElement.style.backgroundImage = new StyleBackground(texture);
    }

    #region ButtonActions

    private void exportButtonClicked()
    {
        ImageToExport = fuser.FuseImages(headImages[headIndex].texture, chestImages[chestIndex].texture, legImages[legIndex].texture, feetImages[feetIndex].texture);
        exporter.ExportImage(fileName.value, ImageToExport);
    }

    private void loadButtonClicked()
    {
        presetLoader.LoadImagePreset(this);

        RebuildListView(headListView, headImages);
        RebuildListView(chestListView, chestImages);
        RebuildListView(legListView, legImages);
        RebuildListView(feetListView, feetImages);

        SetImage(headDisplay, headImages, headIndex);
        SetImage(chestDisplay, chestImages, chestIndex);
        SetImage(legDisplay, legImages, legIndex);
        SetImage(feetDisplay, feetImages, feetIndex);
    }

    private void saveButtonClicked()
    {
        presetSaver.SaveImagePreset(this);
    }
    #endregion
}
