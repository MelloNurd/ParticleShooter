using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerPrefLoader : MonoBehaviour
{
    [SerializeField] private Toggle PostProcess;
    [SerializeField] private Toggle SliderLimit;
    [SerializeField] private Toggle HideSideMenu;
    [SerializeField] private Toggle StopInPause;
    [SerializeField] private Toggle RunInBackground;
    [SerializeField] private Toggle FullScreen;
    [SerializeField] private Toggle FPSToggle;
    [SerializeField] private Camera UICamera;
    [SerializeField] private KeyInputs keyInputs;

    [SerializeField] private MenuButton SideMenuButton;

    void Start()
    {
        SetAllToggles();  
    }

    private void SetAllToggles()
    {
        SetPostProcessingToggle();
        SetSliderLimitToggle();
        SetHideMenuToggle();
        SetStopInPauseToggle();
        SetRunInBackgroundToggle();
        SetFullscreenToggle();
        SetShowFPS();
    }
    
    private void SetPostProcessingToggle()
    {
        if (PlayerPrefs.GetInt("PostProcessing", 1) == 1)
        {
            PostProcess.isOn = true;
            if (UICamera.TryGetComponent(out UniversalAdditionalCameraData cameraData))
            {
                cameraData.renderPostProcessing = true;
            }
        }
        else
        {
            PostProcess.isOn = false;
            if (UICamera.TryGetComponent(out UniversalAdditionalCameraData cameraData))
            {
                cameraData.renderPostProcessing = false;
            }
        }
    }

    private void SetSliderLimitToggle()
    {
        SliderLimit.isOn = (PlayerPrefs.GetInt("SliderLimit", 0) == 1);

        // Manually run each one for initializing, then can just Toggle after
        if (SliderLimit.isOn)
        {
            SlidersController.Instance.IncreaseSlidersLimit();
        }
        else
        {
            SlidersController.Instance.DecreaseSlidersLimit();
        }
    }

    private void SetHideMenuToggle()
    {
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
    }

    private void SetStopInPauseToggle()
    {
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
    }

    private void SetRunInBackgroundToggle()
    {
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

    private void SetFullscreenToggle()
    {
        bool isFullScreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        FullScreen.isOn = isFullScreen;
        if (isFullScreen)
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.SetResolution(Display.displays[Camera.main.targetDisplay].systemWidth, Display.displays[Camera.main.targetDisplay].systemHeight, Screen.fullScreenMode);
        }
    }

    private void SetShowFPS()
    {
        bool state = PlayerPrefs.GetInt("ShowFPS", 0) == 1;
        FPSToggle.isOn = state;
        if (FPSToggle.isOn)
        {
            SlidersController.Instance.EnableFPS();
        }
        else
        {
            SlidersController.Instance.DisableFPS();
        }
    }
}
