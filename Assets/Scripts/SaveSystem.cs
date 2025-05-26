using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SimpleFileBrowser;
using System.Collections;

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

    [SerializeField] private GameObject _playerObject;

    // Loading
    [Header("Loading")]
    [SerializeField] private GameObject _savedItemPrefab;
    [SerializeField] private GameObject _emptyMenuText;
    private CanvasGroup _loadingGroup;
    private Transform _contentHolder;
    private List<string> filePaths = new List<string>();

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
        _loadingGroup = transform.Find("Loading Menu").GetComponent<CanvasGroup>();
        _contentHolder = _loadingGroup.transform.GetChild(0).GetChild(0).GetChild(0);

        Button closeButton = _loadingGroup.transform.GetChild(0).Find("Panel").GetChild(0).GetComponent<Button>();
        closeButton.onClick.AddListener(HideLoadMenu);

        _emptyMenuText = _loadingGroup.transform.GetChild(0).Find("Viewport").Find("EmptyMsg").gameObject;
        _emptyMenuText.SetActive(true);

        HideLoadMenu();
    }

    public void ShowLoadMenu()
    {
        Utilities.ShowCanvasGroup(ref _loadingGroup);
        int count = LoadSavedSettings(_contentHolder);
        Debug.Log("Loaded " + count + " settings.");
        _emptyMenuText.SetActive(count == 0);
    }

    public void UpdateEmptyMsg() => _emptyMenuText.SetActive(Directory.GetFiles(DataPath, "*.json", SearchOption.TopDirectoryOnly).Length == 0);

    public void HideLoadMenu()
    {
        Utilities.HideCanvasGroup(ref _loadingGroup);
        foreach (Transform child in _contentHolder)
        {
            Destroy(child.gameObject);
        }
        filePaths.Clear();
    }

    public int LoadSavedSettings(Transform parent)
    {
        if(!Directory.Exists(DataPath)) Directory.CreateDirectory(DataPath);

        string[] files = Directory.GetFiles(DataPath, "*.json", SearchOption.TopDirectoryOnly);
        foreach (string file in files)
        {
            filePaths.Add(file);
            Instantiate(_savedItemPrefab, parent).GetComponent<SavedSettings>().filePath = file;
        }

        return files.Length;
    }

    public void SaveSettingsToFile() => SaveSettingsToFile(SettingsLoader.Instance.GetSettings()); // No parameter saves current settings
    public void SaveSettingsToFile(SimSettings settings) => StartCoroutine(SaveDialog(settings)); // Option to import different settings
    private IEnumerator SaveDialog(SimSettings settings)
    {
        _playerObject.SetActive(false);

        FileBrowser.SetFilters(false, new FileBrowser.Filter("Settings Files", ".json"));
        FileBrowser.AddQuickLink("Settings", DataPath);
        FileBrowser.AddQuickLink("Downloads", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads");
        yield return FileBrowser.WaitForSaveDialog(FileBrowser.PickMode.Files, initialPath: DataPath, title: "Save Setting File", initialFilename: "new_settings_file.json");
        if (FileBrowser.Success)
        {

            string[] paths = FileBrowser.Result;


            foreach (string path in paths)
            {
                string name = Path.GetFileNameWithoutExtension(path);

                string jsonData = JsonUtility.ToJson(settings.WithName(name), prettyPrint: true);
                File.WriteAllText(path, jsonData);
            }
        }
        else
        {
            Debug.Log("Unable to save file.");
        }

        _playerObject.SetActive(true);
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

    public void ImportSettings() => StartCoroutine(LoadDialog());
    public IEnumerator LoadDialog()
    {
        _playerObject.SetActive(false);

        FileBrowser.SetFilters(false, new FileBrowser.Filter("Settings Files", ".json"));
        FileBrowser.AddQuickLink("Settings", DataPath);
        FileBrowser.AddQuickLink("Downloads", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads");

        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.Files, initialPath: Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\Downloads", title: "Import Setting File");
        if (FileBrowser.Success)
        {
            string[] paths = FileBrowser.Result;
            foreach (string path in paths)
            {
                SimSettings settings = LoadFromFile(path);
                if (settings != null)
                {
                    SettingsLoader.Instance.SetSettings(settings);

                    // Saving the imported file to the settings folder
                    string newPath = Path.Combine(DataPath, settings.saveName + ".json");

                    string jsonData = JsonUtility.ToJson(settings, prettyPrint: true);
                    File.WriteAllText(newPath, jsonData);

                }
            }
        }
        else
        {
            Debug.Log("No file selected.");
        }

        _playerObject.SetActive(true);
    }
}