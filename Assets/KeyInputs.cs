using UnityEngine;
using UnityEngine.UI;

public class KeyInputs : MonoBehaviour
{
    [SerializeField] private GameObject PauseMenu;
    [SerializeField] private Button SideMenuButton;
    private MenuButton sideMenuButtonScript;
    [SerializeField] private GameObject SideMenu;
    RectTransform sideMenuRectTransform;
    public bool stopOnPause = true;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sideMenuRectTransform = SideMenu.GetComponent<RectTransform>();
        sideMenuButtonScript = SideMenuButton.GetComponent<MenuButton>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if (PauseMenu.activeSelf)
            {
                PauseMenu.SetActive(false);
                SideMenu.transform.localScale = new Vector3(1, 1, 1);
                SettingsLoader.Instance.timeScale = 1f; 
            }
            else
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
    }
    public void ToggleStop()
    {
        stopOnPause = !stopOnPause;
    }
}


