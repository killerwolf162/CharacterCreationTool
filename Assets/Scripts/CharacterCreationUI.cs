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
    [SerializeField] private int headIndex = 1, chestIndex = 1, legIndex = 1, feetIndex = 1;
    [SerializeField] private List<Sprite> headImages = new List<Sprite>();
    [SerializeField] private List<Sprite> chestImages = new List<Sprite>();
    [SerializeField] private List<Sprite> legImages = new List<Sprite>();
    [SerializeField] private List<Sprite> feetImages = new List<Sprite>();

    public Button headForward, headBackward, 
        chestForward, chestBackward, 
        legForward, legBackward, 
        feetForward, feetBackward;
    public VisualElement headDisplay, chestDisplay, legDisplay, feetDisplay;

    private List<List<Sprite>> imageListList = new List<List<Sprite>>();
    private ImageLoader loader;


    private void Awake()
    {
        #region Fill list
        imageListList.Add(headImages);
        imageListList.Add(chestImages);
        imageListList.Add(legImages);
        imageListList.Add(feetImages);
        #endregion

        loader = GetComponent<ImageLoader>();
        loader.onImageLoaded.AddListener(UpdateList);

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

        headDisplay = root.Q<VisualElement>("headdisplay");
        chestDisplay = root.Q<VisualElement>("chestdisplay");
        legDisplay = root.Q<VisualElement>("legdisplay");
        feetDisplay = root.Q<VisualElement>("feetdisplay");

        headForward.clicked += headForwardClicked;
        headBackward.clicked += headBackwardClicked;
        chestForward.clicked += chestForwardClicked;
        chestBackward.clicked += chestBackwardClicked;
        legForward.clicked += legForwardClicked;
        legBackward.clicked += legBackwardClicked;
        feetForward.clicked += feetForwardClicked;
        feetBackward.clicked += feetBackwardClicked;
        #endregion
    }

    public void UpdateList()
    {
        for (int i = 0; i < imageListList.Count; i++)
        {
            foreach (var image in loader.imageCategories[i].imageList)
            {
                imageListList[i].Add(image);
            }
        }

        Debug.Log("Lists updated");
    }

    private void SetImage(VisualElement visElement, List<Sprite> spriteList, ref int index, bool forward)
    {
        if (forward == true) index++;           // spritelist count higher than actual amount of images inside the list(?!) 
        else if (forward == false) index--;     // for now it cycles like I want it to, so no priority to figure out why

        if (index < 0)
            index = spriteList.Count -1;
        else if (index >= spriteList.Count) 
            index = 0;

        var texture = spriteList[index];
        visElement.style.backgroundImage = new StyleBackground(texture);    
    }

    private void headForwardClicked()
    {
        SetImage(headDisplay, headImages, ref headIndex, true); 
    }

    private void headBackwardClicked()
    {
        SetImage(headDisplay, headImages, ref headIndex, false);     
    }

    private void chestForwardClicked()
    {
        SetImage(chestDisplay, chestImages, ref chestIndex, true);
    }

    private void chestBackwardClicked()
    {
        SetImage(chestDisplay, chestImages, ref chestIndex, false);
    }

    private void legForwardClicked()
    {
        SetImage(legDisplay, legImages, ref legIndex, true);
    }

    private void legBackwardClicked()
    {
        SetImage(legDisplay, legImages, ref legIndex, false);
    }

    private void feetForwardClicked()
    {
        SetImage(feetDisplay, feetImages, ref feetIndex, true);
    }

    private void feetBackwardClicked()
    {
        SetImage(feetDisplay, feetImages, ref feetIndex, false);
    }
}
