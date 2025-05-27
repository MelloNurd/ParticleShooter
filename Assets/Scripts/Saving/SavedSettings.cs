using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;

public class SavedSettings : MonoBehaviour
{
    public string filePath;

    private void Start()
    {
        if (filePath.IsBlank() || !File.Exists(filePath))
        {
            Debug.LogWarning("Unable to find settings for item " + gameObject.name);
            return;
        }

        TMP_Text name = transform.Find("Name").GetComponent<TMP_Text>();
        name.text = SaveSystem.LoadFromFile(filePath).saveName;

        Button loadButton = transform.Find("Load").GetComponent<Button>();
        Button deleteButton = transform.Find("Delete").GetComponent<Button>();

        loadButton.onClick.AddListener(LoadSetting);
        deleteButton.onClick.AddListener(DeleteSetting);
    }

    public void LoadSetting()
    {
        if(filePath.IsBlank() || !File.Exists(filePath))
        {
            Debug.LogWarning("Unable to find settings for item " + gameObject.name);
            return;
        }

        if(SettingsLoader.Instance == null)
        {
            Debug.LogWarning("SettingsLoader instance is null. Cannot load settings.");
            return;
        }

        // Store current screen space value before loading new settings
        Vector2 currentScreenSpace = SettingsLoader.Instance.screenSpace;
        float currentScale = CameraScaler.Instance.GetScaleRatio(currentScreenSpace);

        // Load settings from file
        SimSettings settings = SaveSystem.LoadFromFile(filePath);

        // Keep the current screen space instead of the loaded one
        Vector2 originalScreenSpace = settings.ScreenSpace;
        settings.ScreenSpace = currentScreenSpace;

        // Apply settings
        SettingsLoader.Instance.SetSettings(settings);

        SaveSystem.Instance.HideLoadMenu();
    }

    public void DeleteSetting()
    {
        if(filePath.IsBlank() || !File.Exists(filePath))
        {
            Debug.LogWarning("Unable to find settings for item " + gameObject.name);
            return;
        }

        File.Delete(filePath);

        Destroy(gameObject);

        SaveSystem.Instance.UpdateEmptyMsg();
    }
}
