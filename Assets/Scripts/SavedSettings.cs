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

        SimSettings settings = SaveSystem.LoadFromFile(filePath);
        SettingsLoader.Instance.SetSettings(settings);
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
    }
}
