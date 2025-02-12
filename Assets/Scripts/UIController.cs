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
public class PlayerData
{
    public string name = "default";
    public int someInt = 10;
    public float someFloat = 10f;

    public PlayerData()
    {
        Debug.Log("Constructor: " + name);
    }

    [OnSerializing]
    void Serializing(StreamingContext context)
    {
        Debug.Log("OnSerialized");
        Debug.Log("NAME: " + name);
    }

    [OnDeserializing]
    void Deserializing(StreamingContext context)
    {
        Debug.Log("OnDerialized");
        Debug.Log("NAME: " + name);
    }

    [OnDeserialized]
    void Deserialized(StreamingContext context)
    {
        Debug.Log("OnDeserialized");
        Debug.Log("NAME: " + name);
    }

}

public class UIController : MonoBehaviour
{
    public PlayerData data;
    public Button createButton, safeButton, loadButton;
    public TextField fileNameInput, nameField, intField, floatField;
    public VisualElement dataEditor;

    private XmlSerializer xmlSerializer;

    private void Awake()
    {
        var document = GetComponent<UIDocument>().rootVisualElement;

        createButton = document.Q("Create") as Button;
        safeButton = document.Q("Safe") as Button;
        loadButton = document.Q("Load") as Button;

        dataEditor = document.Q<IMGUIContainer>("data-editor");

        fileNameInput = document.Q<TextField>("FileName");
        nameField = document.Q<TextField>("Name");
        intField = document.Q<TextField>("IntField");
        floatField = document.Q<TextField>("FloatField");

        createButton.RegisterCallback<ClickEvent>(OnCreate);
        safeButton.RegisterCallback<ClickEvent>(OnSafe);
        loadButton.RegisterCallback<ClickEvent>(OnLoad);

        StartCoroutine(ChangeChecker());

        xmlSerializer = new XmlSerializer(typeof(PlayerData));
    }

    private void OnDisable()
    {
        createButton.UnregisterCallback<ClickEvent>(OnCreate);
        safeButton.UnregisterCallback<ClickEvent>(OnSafe);
        loadButton.UnregisterCallback<ClickEvent>(OnLoad);
    }

    IEnumerator ChangeChecker()
    {
        while(Application.isPlaying)
        {
            ApplyChanges();
            yield return new WaitForSeconds(1f);
        }
    }

    private void OnCreate(ClickEvent evt)
    {
        string url = Path.Combine(Application.persistentDataPath, fileNameInput.text);
        FileStream fileStream = null;

        try
        {
            fileStream = File.Create(url);
            xmlSerializer.Serialize(fileStream, data);
            fileStream.Flush();
        }
        catch(System.Exception e)
        {
            Debug.LogError(e.Message, gameObject);
        }
        finally
        {
            if (fileStream != null) fileStream.Close();
        }

    }

    private void OnSafe(ClickEvent evt)
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
            xmlSerializer.Serialize(fileStream, data);
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

    private void OnLoad(ClickEvent evt)
    {
        //string[] options = StandaloneFileBrowser.OpenFilePanel("Open File", Application.persistentDataPath, "txt", false); // use this for better file selection
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
            data = (PlayerData)xmlSerializer.Deserialize(fileStream);
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
        Debug.Log(data.name);
    }

    #region UI stuff
    private void ApplyChanges()
    {
        if (data == null) data = new PlayerData();
        data.name = nameField.text;
        data.someInt = SanitizeInt(intField);
        data.someFloat = SanitizeFloat(floatField);
    }
    private int SanitizeInt(TextField field)
    {
        string sanitized;
        int retVal = 0;
        try
        {
            sanitized = Regex.Replace(field.text, @"[^-+0-9]", "");//"[^0-9]"
            retVal = int.Parse(sanitized);
            sanitized = retVal.ToString();
            field.SetValueWithoutNotify(sanitized);
        }
        catch (System.OverflowException e)
        {
            retVal = int.MaxValue;
            sanitized = retVal.ToString();
            field.SetValueWithoutNotify(sanitized);
        }
        catch (System.FormatException e)
        {
            if (field.panel.focusController.focusedElement == field)
                sanitized = field.text;
            else
            {
                Debug.LogWarning("Format exception: " + e.Message);
                sanitized = "0";
                field.SetValueWithoutNotify(sanitized);
            }
        }
        return retVal;
    }
    private float SanitizeFloat(TextField field)
    {
        string sanitized;
        float retVal = 0;
        try
        {
            sanitized = Regex.Replace(field.text, @"[^-+0-9\.eE]", ""); //"[^-0-9.]"
            retVal = float.Parse(sanitized);
            sanitized = retVal.ToString();
            field.SetValueWithoutNotify(sanitized);
        }
        catch (System.FormatException e)
        {
            if (field.panel.focusController.focusedElement == field)
                sanitized = field.text;
            else
            {
                Debug.LogWarning("Format exception: " + e.Message);
                sanitized = "0";
                field.SetValueWithoutNotify(sanitized);
            }
        }
        return retVal;
    }
    private void UpdateEditorDisplay()
    {
        intField.SetValueWithoutNotify(data.someInt.ToString());
        floatField.SetValueWithoutNotify(data.someFloat.ToString());
    }
    #endregion

}
