using NaughtyAttributes;
using TMPro;
using UIRangeSliderNamespace;
using UnityEngine;
using UnityEngine.UI;

public class RangeText : MonoBehaviour
{
    TMP_Text values;
    UIRangeSlider rangeSlider;
    Slider slider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        values = GetComponent<TMP_Text>();
        if(rangeSlider = GetComponentInParent<UIRangeSlider>())
        {
            values.text = rangeSlider.valueMin.ToString("0.00") + " - " + rangeSlider.valueMax.ToString("0.00");
        }
        else if (slider = GetComponentInParent<Slider>())
        {
            values.text = slider.value.ToString("0");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setRangeText(float min, float max)
    {
        values.text = min.ToString("0.00") + " - " + max.ToString("0.00");
    }

    public void setValueText(float value)
    {
        if(value == (int)value)
            values.text = value.ToString("0");
        else
            values.text = value.ToString("0.00");
    }
}
