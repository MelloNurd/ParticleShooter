using UnityEngine;
using UnityEngine.UI;

public class KeyInputs : MonoBehaviour
{
    [SerializeField] private GameObject PauseMenu;
    [SerializeField] private GameObject SettingsMenu;
    [SerializeField] private GameObject ControlsMenu;
    [SerializeField] private Button SideMenuButton;
    private MenuButton sideMenuButtonScript;
    [SerializeField] private GameObject SideMenu;
    RectTransform sideMenuRectTransform;
    public bool stopOnPause = true;
    [SerializeField] Slider screenScale;
    [SerializeField] Slider timeScaleSlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        stopOnPause = PlayerPrefs.GetInt("StopOnPause", 1) == 1; // Default to true if not set
        sideMenuRectTransform = SideMenu.GetComponent<RectTransform>();
        sideMenuButtonScript = SideMenuButton.GetComponent<MenuButton>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            SaveSystem.Instance.HideLoadMenu();
            if (PauseMenu.activeSelf || SettingsMenu.activeSelf || ControlsMenu.activeSelf)
            {
                if(PauseMenu.activeSelf)
                {
                    PauseMenu.SetActive(false);
                }
                else if (SettingsMenu.activeSelf)
                {
                    SettingsMenu.SetActive(false);
                }
                else if (ControlsMenu.activeSelf)
                {
                    ControlsMenu.SetActive(false);
                }
                SideMenu.transform.localScale = new Vector3(1, 1, 1);
                SettingsLoader.Instance.timeScale = 1f; 
            }
            else if(!PauseMenu.activeSelf && !SettingsMenu.activeSelf && !ControlsMenu.activeSelf)
            {
                PauseMenu.SetActive(true);
                if(sideMenuButtonScript.isMenuOpen)
                {
                    // If the side menu is open, close it
                    SideMenuButton.onClick.Invoke();
                }
                SideMenu.transform.localScale = new Vector3(0, 0, 0);
                if(stopOnPause)
                    SettingsLoader.Instance.timeScale = 0f;
            }
        }
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            if (SideMenu.activeSelf && PauseMenu.activeSelf == false)
            {
                SideMenuButton.onClick.Invoke();
            }
        }
        if(Input.GetKeyDown(KeyCode.P))
        {
            if (SettingsLoader.Instance.timeScale == 0)
            {
                SettingsLoader.Instance.timeScale = 1f;
            }
            else
            {
                SettingsLoader.Instance.timeScale = 0f;
            }
        }
        if(Input.GetKeyDown(KeyCode.S))
        {
            SaveSystem.Instance.SaveSettingsToFile();
        }
        if(Input.GetKeyDown(KeyCode.L))
        {
            SaveSystem.Instance.ShowLoadMenu();
        }
        if(Input.GetKeyDown(KeyCode.I))
        {
            SaveSystem.Instance.ImportSettings();
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0f && !Input.GetKey(KeyCode.T))
        {
            screenScale.value -= 0.1f;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f && !Input.GetKey(KeyCode.T))
        {
            screenScale.value += 0.1f;
        }
        if(Input.GetAxis("Mouse ScrollWheel") > 0f && Input.GetKey(KeyCode.T))
        {
            timeScaleSlider.value += 0.1f;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f && Input.GetKey(KeyCode.T))
        {
            timeScaleSlider.value -= 0.1f;
        }
    }
    public void ToggleStop()
    {
        stopOnPause = !stopOnPause;
        PlayerPrefs.SetInt("StopOnPause", stopOnPause ? 1 : 0);
    }
}


