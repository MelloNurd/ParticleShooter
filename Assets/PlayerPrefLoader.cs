using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PlayerPrefLoader : MonoBehaviour
{
    [SerializeField] Toggle PostProcess;
    [SerializeField] Toggle HideSideMenu;
    [SerializeField] Toggle StopInPause;
    [SerializeField] Toggle RunInBackground;
    [SerializeField] Camera mainCamera;
    [SerializeField] KeyInputs keyInputs;

    [SerializeField] MenuButton SideMenuButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(PlayerPrefs.GetInt("PostProcessing", 1) == 1)
        {
            PostProcess.isOn = true;
            if (mainCamera.TryGetComponent(out UniversalAdditionalCameraData cameraData))
            {
                // Enable post-processing
                cameraData.renderPostProcessing = true;
            }
        }
        else
        {
            PostProcess.isOn = false;
            if (mainCamera.TryGetComponent(out UniversalAdditionalCameraData cameraData))
            {
                // Disable post-processing
                cameraData.renderPostProcessing = false;
            }
        }

        if (PlayerPrefs.GetInt("HideSideMenu", 0) == 1)
        {
            HideSideMenu.isOn = true;
            SideMenuButton.ToggleSideMenu();
            SideMenuButton.transform.gameObject.SetActive(false);
        }
        else
        {
            HideSideMenu.isOn = false;
            SideMenuButton.transform.gameObject.SetActive(true);
        }

        if (PlayerPrefs.GetInt("StopInPause", 1) == 1)
        {
            StopInPause.isOn = true;
            keyInputs.stopOnPause = true; 
        }
        else
        {
            StopInPause.isOn = false;
            keyInputs.stopOnPause = false;
        }

        if (PlayerPrefs.GetInt("RunInBackground", 0) == 1)
        {
            RunInBackground.isOn = true;
            Application.runInBackground = true;
        }
        else
        {
            RunInBackground.isOn = false;
            Application.runInBackground = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
