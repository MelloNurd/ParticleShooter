using UnityEngine;
using UnityEngine.UI;

public class KeyInputs : MonoBehaviour
{
    [SerializeField] private GameObject PauseMenu;
    [SerializeField] private Button SideMenuButton;
    [SerializeField] private GameObject SideMenu;
    [SerializeField] private Slider TimeSlider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
                TimeSlider.value = 1;
            }
            else
            {
                PauseMenu.SetActive(true);
                TimeSlider.value = 0;
                SideMenu.transform.localScale = new Vector3(0, 0, 0);
            }
        }
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            if (SideMenuButton != null && SideMenu.activeSelf)
            {
                SideMenuButton.onClick.Invoke();
            }
        }
    }
}
