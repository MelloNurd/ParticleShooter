using UnityEngine;
using PrimeTween;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using NaughtyAttributes;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class MenuButton : MonoBehaviour
{
    public bool isMenuOpen;
    public bool closeOnStart;

    private TMP_Text arrows;
    private RectTransform parentTransform;

    private float startXPos;
    private float widthToMove;
    [Scene] [SerializeField] private string scene;

    public bool keepMenuHidden = false;
    private void Awake()
    {
        parentTransform = transform.parent.GetComponent<RectTransform>();
        arrows = GetComponentInChildren<TMP_Text>();

        startXPos = parentTransform.anchoredPosition.x;

        isMenuOpen = parentTransform.anchoredPosition.x > 0;
    }

    public void Start()
    {
        if(closeOnStart)
        {
            ToggleSideMenu();
        }
    }

    public void ToggleSideMenu()
    {
        widthToMove = parentTransform.rect.width;

        if (isMenuOpen)
        {            
            Tween.UIAnchoredPositionX(parentTransform, -widthToMove, 0.5f, useUnscaledTime: true).OnComplete(() =>
            {
                arrows.text = ">>";
            });
        }
        else
        {
            Tween.UIAnchoredPositionX(parentTransform, startXPos, 0.5f, useUnscaledTime: true).OnComplete(() =>
            {
                arrows.text = "<<";
            });
        }
        isMenuOpen = !isMenuOpen;
    }

    public void ToggleMenu(GameObject menu)
    {
        menu.SetActive(!menu.activeSelf);
    }

    public void SwitchPlayText(TMP_Text playText)
    {
        if (playText.text == "Play")
        {
            playText.text = "Resume";
        }
    }

    public void HideObjectByScale(GameObject obj)
    {
        if (!keepMenuHidden)
        {
            if (obj.transform.localScale.x > 0)
            {
                obj.transform.localScale = new Vector3(0, 0, 0);
            }
            else
            {
                obj.transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Quit");
    }

    public void TogglePostProcessing(Camera camera)
    {
        if (camera.TryGetComponent(out UniversalAdditionalCameraData cameraData))
        {
            // Disable post-processing
            cameraData.renderPostProcessing = !cameraData.renderPostProcessing;
        }
    }

    public void HideSideMenu(GameObject obj)
    {
        obj.SetActive(!obj.activeSelf);
    }
}
