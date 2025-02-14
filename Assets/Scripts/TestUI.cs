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

[System.Serializable]
public class TestPlayerData
{
    public string name = "default";
    [OptionalField(VersionAdded = 2)]
    public string lastName = "lastName";
    public float someFloat = 10f;
    public int someInt = 10;
    public List<int> someListOfData = new List<int>();
    public TestPlayerData()
    {
        //Debug.Log("Constructor: " + name);
    }
    [OnSerializing]
    void Serializing(StreamingContext context)
    {
        //Debug.Log("OnSerializing");
        //Debug.Log("NAME: " + name);
    }
    [OnSerialized]
    void Serialized(StreamingContext context)
    {
        //Debug.Log("OnSerialized");
        //Debug.Log("NAME: " + name);
    }
    [OnDeserializing]
    void Deserializing(StreamingContext context)
    {
        //Debug.Log("OnDeserializing");
        //Debug.Log("NAME: " + name);
    }
    [OnDeserialized]
    void Deserialized(StreamingContext context)
    {
        //Debug.Log("OnDeserialized");
        //Debug.Log("NAME: " + name);
    }
}

public class TestUI : MonoBehaviour
{
    public TextField fileNameInput;
    public Button createButton, saveButton, loadButton;
    public TestPlayerData myData;
    // TODO: Move this to a class of some sort, maybe with a generic / interface for applying/updating?
    public TextField nameField;
    public IntegerField intField;
    public FloatField floatField;
    XmlSerializer xmlSerializer;
    // Start is called before the first frame update
    void Start()
    {
        #region UI Init
        var root = GetComponent<UIDocument>().rootVisualElement;
        // file input field
        fileNameInput = root.Q<TextField>("filename");
        // get top level buttons
        createButton = root.Q<Button>("create");
        saveButton = root.Q<Button>("save");
        loadButton = root.Q<Button>("load");
        // get data editor & child name field
        nameField = root.Q<TextField>("name");
        intField = root.Q<IntegerField>("int");
        floatField = root.Q<FloatField>("float");
        // implement button reactions
        createButton.clicked += CreateButton_clicked;
        saveButton.clicked += SaveButton_clicked;
        loadButton.clicked += LoadButton_clicked;
        StartCoroutine(ChangeChecker());
        #endregion
        xmlSerializer = new XmlSerializer(typeof(TestPlayerData));
    }
    IEnumerator ChangeChecker()
    {
        // TODO: Focus Events...
        while (Application.isPlaying)
        {
            ApplyChanges();
            yield return new WaitForSeconds(1f);
        }
        Debug.Log("EXIT");
    }
    private void CreateButton_clicked()
    {
        string url = Path.Combine(Application.persistentDataPath, fileNameInput.text);
        FileStream fileStream = null;
        try
        {
            fileStream = File.Create(url);
            // write something
            xmlSerializer.Serialize(fileStream, myData);
            // flush
            fileStream.Flush();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message, gameObject);
        }
        finally
        {
            // close
            if (fileStream != null) fileStream.Close();
        }
    }
    public void LoadButton_clicked()
    {
        //string[] options = StandaloneFileBrowser.OpenFilePanel("Open File", Application.persistentDataPath, "txt", false);
        //if (options.Length == 0) return;
        //string url = options[0];
        string url = Path.Combine(Application.persistentDataPath, fileNameInput.text);
        FileStream fileStream = null;
        try
        {
            // Early return
            // if ( !File.Exists(url) )
            fileStream = File.OpenRead(url);
            // read something
            myData = (TestPlayerData)xmlSerializer.Deserialize(fileStream);
            UpdateEditorDisplay();
        }
        catch (System.Exception e)
        {
            // Woops achteraf
            Debug.LogError(e.GetType() + " " + e.Message, gameObject);
        }
        finally
        {
            // close
            if (fileStream != null) fileStream.Close();
        }
        Debug.Log(myData.name);
    }
    public void SaveButton_clicked()
    {
        string url = Path.Combine(Application.persistentDataPath, fileNameInput.text);
        FileStream fileStream = null;
        try
        {
            if (File.Exists(url))
            {
                // TODO: Warn the user
            }
            fileStream = File.Create(url);
            // write something
            xmlSerializer.Serialize(fileStream, myData);
            // flush
            fileStream.Flush();
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.Message, gameObject);
        }
        finally
        {
            // close
            if (fileStream != null) fileStream.Close();
        }
    }
    #region UI stuff
    private void ApplyChanges()
    {
        if (myData == null) myData = new TestPlayerData();
        myData.name = nameField.text;
        myData.someInt = intField.value;
        myData.someFloat = floatField.value;
    }
   
    private void UpdateEditorDisplay()
    {
        nameField.SetValueWithoutNotify(myData.name);
        intField.SetValueWithoutNotify(myData.someInt);
        floatField.SetValueWithoutNotify(myData.someFloat);
    }
    #endregion
}
