using System;
using NaughtyAttributes;
using System;
using TMPro;
using UIRangeSliderNamespace;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RangeText : MonoBehaviour
{
    UIRangeSlider rangeSlider;
    Slider slider;
    TMP_InputField upperField;
    TMP_InputField lowerField;
    TMP_InputField singleField;
    public bool wholeNumber = false;

    private Action<float> onValueChanged;
    private Action<float, float> onRangeChanged;
    private Func<float> getValue;
    private Func<float, float> getRange;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        if (rangeSlider = GetComponentInParent<UIRangeSlider>())
        {
            lowerField = transform.Find("LowerField").GetComponent<TMP_InputField>();
            upperField = transform.Find("UpperField").GetComponent<TMP_InputField>();
            if (wholeNumber)
            {
                lowerField.text = rangeSlider.valueMin.ToString("0");
                upperField.text = rangeSlider.valueMax.ToString("0");
            }
            else
            {
                lowerField.text = rangeSlider.valueMin.ToString("0.00");
                upperField.text = rangeSlider.valueMax.ToString("0.00");
            }
        }
        else if (slider = GetComponentInParent<Slider>())
        {
            singleField = transform.Find("SingleField").GetComponent<TMP_InputField>();
            if (wholeNumber)
                singleField.text = slider.value.ToString("0");
            else
                singleField.text = slider.value.ToString("0.00");
        }
    }

    public void BindValue(Func<float> getter, Action<float> setter)
    {
        getValue = getter;
        onValueChanged = setter;
        slider.onValueChanged.AddListener(val => onValueChanged?.Invoke(val));
        slider.value = getValue(); // Initialize slider to current value
    }

    public void BindRange(Func<float, float> getter, Action<float, float> setter)
    {
        getRange = getter;
        onRangeChanged = setter;
        rangeSlider.onValuesChanged.AddListener((val1, val2) => onRangeChanged?.Invoke(val1, val2));
        rangeSlider.valueMin = getRange(0);
        rangeSlider.valueMax = getRange(1);
    }

    public void setRangeText(float min, float max)
    {
        if (wholeNumber)
        {
            lowerField.text = min.ToString("0");
            upperField.text = max.ToString("0");
        }
        else
        {
            lowerField.text = min.ToString("0.00");
            upperField.text = max.ToString("0.00");
        }
    }

    public void setValueText(float value)
    {
        if (wholeNumber)
            singleField.text = value.ToString("0");
        else
            singleField.text = value.ToString("0.00");
    }

    public void updateValueText()
    {
        if (wholeNumber)
            singleField.text = slider.value.ToString("0");
        else
            singleField.text = slider.value.ToString("0.00");
    }

    public void updateRangeText()
    {
        if (wholeNumber)
        {
            lowerField.text = rangeSlider.valueMin.ToString("0");
            upperField.text = rangeSlider.valueMax.ToString("0");
        }
        else
        {
            lowerField.text = rangeSlider.valueMin.ToString("0.00");
            upperField.text = rangeSlider.valueMax.ToString("0.00");
        }
    }

    public void RangeChanged()
    {
        rangeSlider.valueMin = float.Parse(lowerField.text);
        rangeSlider.valueMax = float.Parse(upperField.text);
    }

    public void ValueChanged()
    {
        slider.value = float.Parse(singleField.text);
    }
}
