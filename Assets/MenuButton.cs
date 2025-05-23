using UnityEngine;
using PrimeTween;
using TMPro;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    public bool isMenuOpen;

    private TMP_Text arrows;
    private RectTransform parentTransform;
    private Outline outline;

    private float startXPos;
    private float widthToMove;

    private void Awake()
    {
        isMenuOpen = true;

        parentTransform = transform.parent.GetComponent<RectTransform>();
        arrows = GetComponentInChildren<TMP_Text>();
        outline = GetComponent<Outline>();

        startXPos = parentTransform.anchoredPosition.x;
    }
    public void ToggleMenu()
    {
        widthToMove = parentTransform.rect.width + outline.effectDistance.x;

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
