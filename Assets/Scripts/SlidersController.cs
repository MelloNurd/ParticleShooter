using UIRangeSliderNamespace;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SlidersController : MonoBehaviour
{
    public Slider ScreenSpace;
    public Slider NumberOfParticles;
    public Slider NumberOfTypes;

    public UIRangeSlider forces;
    public UIRangeSlider minDistances;
    public UIRangeSlider radii;

    public Slider Friction;
    public Slider Dampening;
    public Slider RepulsionEffector;

    public Slider TimeScale;

    public void OnEnable()
    {
        ScreenSpace.onValueChanged.AddListener(val => 
        {
            SettingsLoader.Instance.screenSpace = CameraScaler.Instance.AdjustByScale(ScreenSpace.value);
            SettingsLoader.Instance.RefreshSettings();
            CameraScaler.Instance.ScaleCamera();
            ScreenSpace.GetComponent<RangeText>().updateValueText(); 
        });

        NumberOfParticles.onValueChanged.AddListener(val => 
        {
            SettingsLoader.Instance.numberOfParticles = (int)NumberOfParticles.value;
            SettingsLoader.Instance.ForceRestart();
            NumberOfParticles.GetComponent<RangeText>().updateValueText(); 
        });

        NumberOfTypes.onValueChanged.AddListener(val => 
        { 
            SettingsLoader.Instance.numberOfTypes = (int)NumberOfTypes.value;
            SettingsLoader.Instance.ForceRestart();
            NumberOfTypes.GetComponent<RangeText>().updateValueText();
        });

        forces.onValuesChanged.AddListener((val1, val2) => 
        {
            SettingsLoader.Instance.forcesRange = new Vector2(forces.valueMin, forces.valueMax);
            SettingsLoader.Instance.RefreshSettings();
            forces.GetComponent<RangeText>().updateRangeText();
        });

        minDistances.onValuesChanged.AddListener((val1, val2) =>
        {
            SettingsLoader.Instance.minDistancesRange = new Vector2(minDistances.valueMin, minDistances.valueMax);
            SettingsLoader.Instance.RefreshSettings();
            minDistances.GetComponent<RangeText>().updateRangeText();
        });

        radii.onValuesChanged.AddListener((val1, val2) => 
        { 
            SettingsLoader.Instance.radiiRange = new Vector2(radii.valueMin, radii.valueMax);
            SettingsLoader.Instance.RefreshSettings();
            radii.GetComponent<RangeText>().updateRangeText();
        });

        Friction.onValueChanged.AddListener(val => 
        { 
            SettingsLoader.Instance.friction = Friction.value;
            SettingsLoader.Instance.RefreshSettings();
            Friction.GetComponent<RangeText>().updateValueText();
        });

        Dampening.onValueChanged.AddListener(val => 
        { 
            SettingsLoader.Instance.dampening = Dampening.value;
            SettingsLoader.Instance.RefreshSettings();
            Dampening.GetComponent<RangeText>().updateValueText();
        });

        RepulsionEffector.onValueChanged.AddListener(val => 
        { 
            SettingsLoader.Instance.repulsion = RepulsionEffector.value;
            SettingsLoader.Instance.RefreshSettings();
            RepulsionEffector.GetComponent<RangeText>().updateValueText();
        });

        TimeScale.onValueChanged.AddListener(val => 
        {
            SettingsLoader.Instance.timeScale = TimeScale.value;
            SettingsLoader.Instance.RefreshSettings();
            TimeScale.GetComponent<RangeText>().updateValueText();
        });

        SettingsLoader.SettingsChanged.AddListener(SetSliders);
    }

    public void OnDisable()
    {
        SettingsLoader.SettingsChanged.RemoveListener(SetSliders);

        ScreenSpace.onValueChanged.RemoveAllListeners();
        NumberOfParticles.onValueChanged.RemoveAllListeners();
        NumberOfTypes.onValueChanged.RemoveAllListeners();
        forces.onValuesChanged.RemoveAllListeners();
        minDistances.onValuesChanged.RemoveAllListeners();
        radii.onValuesChanged.RemoveAllListeners();
        Friction.onValueChanged.RemoveAllListeners();
        Dampening.onValueChanged.RemoveAllListeners();
        RepulsionEffector.onValueChanged.RemoveAllListeners();
        TimeScale.onValueChanged.RemoveAllListeners();
    }

    public void SetSliders()
    {
        SimSettings settings = SettingsLoader.Instance.GetSettings();

        ScreenSpace.value = CameraScaler.Instance.GetScaleRatio(settings.ScreenSpace);

        NumberOfParticles.value = settings.NumberOfParticles;

        NumberOfTypes.value = settings.NumberOfTypes;

        forces.valueMin = settings.forces.x;
        forces.valueMax = settings.forces.y;

        minDistances.valueMin = settings.minDistances.x;
        minDistances.valueMax = settings.minDistances.y;

        radii.valueMin = settings.radii.x;
        radii.valueMax = settings.radii.y;

        Friction.value = settings.Friction;

        Dampening.value = settings.Dampening;

        RepulsionEffector.value = settings.RepulsionEffector;

        TimeScale.value = settings.TimeScale;
    }
}
