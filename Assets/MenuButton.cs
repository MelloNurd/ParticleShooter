using UnityEngine;
using PrimeTween;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;

public class MenuButton : MonoBehaviour
{
    public bool isMenuOpen;

    private TMP_Text arrows;
    private RectTransform parentTransform;

    private float startXPos;
    private float widthToMove;

    private void Awake()
    {
        parentTransform = transform.parent.GetComponent<RectTransform>();
        arrows = GetComponentInChildren<TMP_Text>();

        startXPos = parentTransform.anchoredPosition.x;

        isMenuOpen = parentTransform.anchoredPosition.x > 0;
    }

    public void ToggleMenu()
    {
        widthToMove = parentTransform.rect.width;

        if (isMenuOpen)
        {            
            Tween.UIAnchoredPositionX(parentTransform, -widthToMove, 0.5f).OnComplete(() =>
            {
                arrows.text = ">>";
            });
        }
        else
        {
            Tween.UIAnchoredPositionX(parentTransform, startXPos, 0.5f).OnComplete(() =>
            {
                arrows.text = "<<";
            });
        }
        isMenuOpen = !isMenuOpen;
    }
}
