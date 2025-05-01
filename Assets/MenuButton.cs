using UnityEngine;
using PrimeTween;
using TMPro;

public class MenuButton : MonoBehaviour
{
    TMP_Text arrows;
    RectTransform parentTransform;
    private void Start()
    {
        parentTransform = transform.parent.GetComponent<RectTransform>();
        arrows = GetComponentInChildren<TMP_Text>();
    }
    public void ToggleMenu()
    {
        if(parentTransform.anchoredPosition.x < -1000)
        {            
            Tween.UIAnchoredPositionX(parentTransform, -962, 0.5f);
            arrows.text = "<<";
        }
        else
        {
            Tween.UIAnchoredPositionX(parentTransform, -1871, 0.5f);
            arrows.text = ">>";
        }

    }
}
