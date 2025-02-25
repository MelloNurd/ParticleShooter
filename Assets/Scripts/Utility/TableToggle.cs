using UnityEngine;

public class TableToggle : MonoBehaviour
{
    [SerializeField] private GameObject table;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            table.SetActive(!table.activeSelf);
            Time.timeScale = table.activeSelf ? 0 : 1;
        }
    }
}
