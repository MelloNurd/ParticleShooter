using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class SimSettings
{
    public string saveName;

    public Vector2 ScreenSpace;
    public int NumberOfParticles;
    public int NumberOfTypes;

    public Vector2 forces;
    public Vector2 minDistances;
    public Vector2 radii;

    public float Friction;
    public float Dampening;
    public float RepulsionEffector;

    public float TimeScale;

    public void PrintAll()
    {
        Debug.Log("saveName: " + saveName);
        Debug.Log("ScreenSpace: " + ScreenSpace);
        Debug.Log("NumberOfParticles: " + NumberOfParticles);
        Debug.Log("NumberOfTypes: " + NumberOfTypes);
        Debug.Log("forces: " + forces);
        Debug.Log("minDistances: " + minDistances);
        Debug.Log("radii: " + radii);
        Debug.Log("Friction: " + Friction);
        Debug.Log("Dampening: " + Dampening);
        Debug.Log("RepulsionEffector: " + RepulsionEffector);
        Debug.Log("TimeScale: " + TimeScale);
    }

    public SimSettings WithName(string name)
    {
        saveName = name;
        return this;
    }
}

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }
    
    public static string DataPath => Path.Combine(Application.persistentDataPath, "Settings");

    // Loading
    [Header("Loading")]
    [SerializeField] GameObject _savedItemPrefab;
    private CanvasGroup _loadingGroup;
    private Transform _contentHolder;
    private List<string> filePaths = new List<string>();

    // Saving
    private CanvasGroup _savingGroup;
    private TMP_InputField _nameInput;
    private TMP_InputField _directoryInput;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Loading
        _loadingGroup = transform.Find("LoadingMenu").GetComponent<CanvasGroup>();
        _contentHolder = _loadingGroup.transform.GetChild(0).GetChild(0).GetChild(0);

        Button closeButton = _loadingGroup.transform.GetChild(0).Find("Panel").GetChild(0).GetComponent<Button>();
        closeButton.onClick.AddListener(HideLoadMenu);

        // Saving
        _savingGroup = transform.Find("SavingMenu").GetComponent<CanvasGroup>();
        _nameInput = _savingGroup.transform.GetChild(0).Find("FileName").GetComponent<TMP_InputField>();
        _directoryInput = _savingGroup.transform.GetChild(0).Find("Directory").GetComponent<TMP_InputField>();

        Button cancelButton = _savingGroup.transform.GetChild(0).Find("Cancel").GetComponent<Button>();
        cancelButton.onClick.AddListener(HideSaveMenu);
        
        Button saveButton = _savingGroup.transform.GetChild(0).Find("Save").GetComponent<Button>();
        saveButton.onClick.AddListener(SaveNewSettings);

        HideSaveMenu();
        HideLoadMenu();
    }

    public void ShowSaveMenu()
    {
        HideLoadMenu();
        Utilities.ShowCanvasGroup(ref _savingGroup);
        _nameInput.text = string.Empty;
        _directoryInput.text = Application.persistentDataPath;
    }

    public void HideSaveMenu()
    {
        Utilities.HideCanvasGroup(ref _savingGroup);
        _nameInput.text = string.Empty;
        _directoryInput.text = Application.persistentDataPath;
    }

    public void ShowLoadMenu()
    {
        HideSaveMenu();
        Utilities.ShowCanvasGroup(ref _loadingGroup);
        LoadSavedSettings(_contentHolder);
    }

    public void HideLoadMenu()
    {
        Utilities.HideCanvasGroup(ref _loadingGroup);
        foreach (Transform child in _contentHolder)
        {
            Destroy(child.gameObject);
        }
        filePaths.Clear();
    }

    public void SaveNewSettings()
    {
        string fileName = _nameInput.text.FileNameFriendly();
        string directory = _directoryInput.text;
        if (string.IsNullOrEmpty(fileName) || string.IsNullOrEmpty(directory))
        {
            Debug.LogWarning("File name or directory is empty. Cannot save settings.");
            return;
        }
        
        SimSettings settings = SettingsLoader.Instance.GetSettings().WithName(fileName);

        SaveToFile(settings);

        Debug.Log("Settings saved to " + fileName + ".json in " + directory);

        HideSaveMenu();
    }

    public void LoadSavedSettings(Transform parent)
    {
        if(!Directory.Exists(DataPath)) Directory.CreateDirectory(DataPath);

        string[] files = Directory.GetFiles(DataPath, "*.json", SearchOption.TopDirectoryOnly);
        foreach (string file in files)
        {
            filePaths.Add(file);
            Debug.Log(file);
            Instantiate(_savedItemPrefab, parent).GetComponent<SavedSettings>().filePath = file;
        }
    }

    public static void SaveToFile(SimSettings settings)
    {
        string filePath = Path.Combine(DataPath, settings.saveName.FileNameFriendly() + ".json").AutoIncrementFileName();

        string jsonData = JsonUtility.ToJson(settings, prettyPrint: true);
        File.WriteAllText(filePath, jsonData);
    }

    public static SimSettings LoadFromFile(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<SimSettings>(json);
        }
        Debug.Log("Unable to find file path: " + filePath);
        return null;
    }

    public static void test() {
        //System.Diagnostics.Process.Start("explorer.exe" , "/ select," + path);
    }
}

// When importing, select a file outside normal save path. Load it, then create a copy inside normal save path.