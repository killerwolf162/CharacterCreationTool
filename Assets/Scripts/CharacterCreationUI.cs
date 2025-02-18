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
    [SerializeField] private List<Sprite> headImages = new List<Sprite>();
    [SerializeField] private List<Sprite> chestImages = new List<Sprite>();
    [SerializeField] private List<Sprite> legImages = new List<Sprite>();
    [SerializeField] private List<Sprite> feetImages = new List<Sprite>();
    [SerializeField] public int headIndex = 1, chestIndex = 1, legIndex = 1, feetIndex = 1;

    private Button headForward, headBackward,
        chestForward, chestBackward,
        legForward, legBackward,
        feetForward, feetBackward,
        exportButton, loadButton, saveButton;

    public TextField fileName;

    public VisualElement headDisplay, chestDisplay, legDisplay, feetDisplay;

    private List<List<Sprite>> imageCategoryList = new List<List<Sprite>>();
    private ImageLoader loader;
    private ImageExporter exporter;
    private PresetSaver presetSaver;
    private PresetLoader presetLoader;


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
        #endregion

        #region UI Init

        var root = GetComponent<UIDocument>().rootVisualElement;

        headForward = root.Q<Button>("headforward");
        headBackward = root.Q<Button>("headbackward");
        chestForward = root.Q<Button>("chestforward");
        chestBackward = root.Q<Button>("chestbackward");
        legForward = root.Q<Button>("legsforward");
        legBackward = root.Q<Button>("legsbackward");
        feetForward = root.Q<Button>("feetforward");
        feetBackward = root.Q<Button>("feetbackward");
        exportButton = root.Q<Button>("exportbutton");
        loadButton = root.Q<Button>("loadbutton");
        saveButton = root.Q<Button>("savebutton");

        headDisplay = root.Q<VisualElement>("headdisplay");
        chestDisplay = root.Q<VisualElement>("chestdisplay");
        legDisplay = root.Q<VisualElement>("legdisplay");
        feetDisplay = root.Q<VisualElement>("feetdisplay");

        headDisplay.AddManipulator(new DragManipulator(headDisplay));
        chestDisplay.AddManipulator(new DragManipulator(chestDisplay));
        legDisplay.AddManipulator(new DragManipulator(legDisplay));
        feetDisplay.AddManipulator(new DragManipulator(feetDisplay));

        fileName = root.Q<TextField>("filename");

        headForward.clicked += headForwardClicked;
        headBackward.clicked += headBackwardClicked;
        chestForward.clicked += chestForwardClicked;
        chestBackward.clicked += chestBackwardClicked;
        legForward.clicked += legForwardClicked;
        legBackward.clicked += legBackwardClicked;
        feetForward.clicked += feetForwardClicked;
        feetBackward.clicked += feetBackwardClicked;
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

    private void SetImage(VisualElement visElement, List<Sprite> spriteList, int index)
    {
        var texture = spriteList[index];
        visElement.style.backgroundImage = new StyleBackground(texture);
    }

    private void CycleThroughImage(VisualElement visElement, List<Sprite> spriteList, ref int index, bool forward)
    {
        if (forward == true) index++;
        else if (forward == false) index--;
        if (index < 0)
            index = spriteList.Count - 1;
        else if (index >= spriteList.Count)
            index = 0;
        SetImage(visElement, spriteList, index);
    }

    #region ButtonActions
    private void headForwardClicked()
    {
        CycleThroughImage(headDisplay, headImages, ref headIndex, true);
    }

    private void headBackwardClicked()
    {
        CycleThroughImage(headDisplay, headImages, ref headIndex, false);
    }

    private void chestForwardClicked()
    {
        CycleThroughImage(chestDisplay, chestImages, ref chestIndex, true);
    }

    private void chestBackwardClicked()
    {
        CycleThroughImage(chestDisplay, chestImages, ref chestIndex, false);
    }

    private void legForwardClicked()
    {
        CycleThroughImage(legDisplay, legImages, ref legIndex, true);
    }

    private void legBackwardClicked()
    {
        CycleThroughImage(legDisplay, legImages, ref legIndex, false);
    }

    private void feetForwardClicked()
    {
        CycleThroughImage(feetDisplay, feetImages, ref feetIndex, true);
    }

    private void feetBackwardClicked()
    {
        CycleThroughImage(feetDisplay, feetImages, ref feetIndex, false);
    }

    private void exportButtonClicked()
    {
        exporter.ExportImage(fileName.value, headImages[1].texture); // change to actual image when figured out how to fuse images
    }

    private void loadButtonClicked()
    {
        presetLoader.LoadImagePreset(this);
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
