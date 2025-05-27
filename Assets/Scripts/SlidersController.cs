using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UIRangeSliderNamespace;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SlidersController : MonoBehaviour
{
    public static SlidersController Instance { get; private set; }

    public Slider ScreenSpace;
    public Slider NumberOfParticles;
    public Slider NumberOfTypes;

    public UIRangeSlider Forces;
    public UIRangeSlider MinDistances;
    public UIRangeSlider Radii;

    public Slider Friction;
    public Slider Dampening;
    public Slider RepulsionEffector;

    public Slider TimeScale;

    public Button RandomizeButton;

    public Button SaveButton;
    public Button LoadButton;
    public Button ImportButton;

    bool wasAlreadyHerePal = false;
    bool first = true;

    // Limits
    private float numParticlesLimit;
    private float numTypesLimit;
    private float forcesLimit;
    private float minDistancesLimit;
    private float radiiLimit;

    [SerializeField] private GameObject warningMsg;

    public void OnEnable()
    {
        // In SlidersController.cs, replace the ScreenSpace.onValueChanged listener with this:
        ScreenSpace.onValueChanged.AddListener(val =>
        {
            if (wasAlreadyHerePal) return; // Skip if we're already processing
            wasAlreadyHerePal = true;

            // If shape changed was triggered by screen resize, set the value once and exit
            if (CameraScaler.Instance.shapeChanged)
            {
                ScreenSpace.SetValueWithoutNotify(CameraScaler.Instance.lastScale); // Use this instead of value = to avoid loop
                SettingsLoader.Instance.screenSpace = CameraScaler.Instance.AdjustByScale(CameraScaler.Instance.lastScale);
                CameraScaler.Instance.shapeChanged = false;
            }
            else
            {
                // Normal slider movement by user
                SettingsLoader.Instance.screenSpace = CameraScaler.Instance.AdjustByScale(val);
            }

            SettingsLoader.Instance.RefreshSettings();
            CameraScaler.Instance.ScaleCamera();
            ScreenSpace.GetComponent<RangeText>().updateValueText();
            wasAlreadyHerePal = false;
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

        Forces.onValuesChanged.AddListener((val1, val2) =>
        {
            SettingsLoader.Instance.forcesRange = new Vector2(Forces.valueMin, Forces.valueMax);
            SettingsLoader.Instance.RefreshSettings();
            Forces.GetComponent<RangeText>().updateRangeText();
        });

        MinDistances.onValuesChanged.AddListener((val1, val2) =>
        {
            SettingsLoader.Instance.minDistancesRange = new Vector2(MinDistances.valueMin, MinDistances.valueMax);
            SettingsLoader.Instance.RefreshSettings();
            MinDistances.GetComponent<RangeText>().updateRangeText();
        });

        Radii.onValuesChanged.AddListener((val1, val2) =>
        {
            SettingsLoader.Instance.radiiRange = new Vector2(Radii.valueMin, Radii.valueMax);
            SettingsLoader.Instance.RefreshSettings();
            Radii.GetComponent<RangeText>().updateRangeText();
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

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        numParticlesLimit = NumberOfParticles.maxValue;
        numTypesLimit = NumberOfTypes.maxValue;
        forcesLimit = Forces.maxLimit;
        minDistancesLimit = MinDistances.maxLimit;
        radiiLimit = Radii.maxLimit;
    }

    private void Start()
    {
        RandomizeButton.onClick.AddListener(RandomizeSliders);
        SaveButton.onClick.AddListener(() => SaveSystem.Instance.SaveSettingsToFile());
        LoadButton.onClick.AddListener(() => SaveSystem.Instance.ShowLoadMenu());
        ImportButton.onClick.AddListener(() => SaveSystem.Instance.ImportSettings());
    }

    private void Update()
    {
        if(first)
        {
            first = false;
            SetSliders();
        }
    }

    public void OnDisable()
    {
        SettingsLoader.SettingsChanged.RemoveListener(SetSliders);

        ScreenSpace.onValueChanged.RemoveAllListeners();
        NumberOfParticles.onValueChanged.RemoveAllListeners();
        NumberOfTypes.onValueChanged.RemoveAllListeners();
        Forces.onValuesChanged.RemoveAllListeners();
        MinDistances.onValuesChanged.RemoveAllListeners();
        Radii.onValuesChanged.RemoveAllListeners();
        Friction.onValueChanged.RemoveAllListeners();
        Dampening.onValueChanged.RemoveAllListeners();
        RepulsionEffector.onValueChanged.RemoveAllListeners();
        TimeScale.onValueChanged.RemoveAllListeners();
    }

    public void RandomizeSliders()
    {
        //RandomizeSlider(ScreenSpace);
        RandomizeSlider(NumberOfParticles);
        RandomizeSlider(NumberOfTypes);
        RandomizeRangeSlider(Forces);
        RandomizeRangeSlider(MinDistances);
        RandomizeRangeSlider(Radii);
        //RandomizeSlider(Friction);
        //RandomizeSlider(Dampening);
        //RandomizeSlider(RepulsionEffector);
        //RandomizeSlider(TimeScale);
        SettingsLoader.Instance.RefreshSettings();
    }

    private void RandomizeRangeSlider(UIRangeSlider slider)
    {
        float newVal1 = Random.Range(slider.minLimit, slider.maxLimit);
        float newVal2 = Random.Range(slider.minLimit, slider.maxLimit);

        slider.valueMin = Mathf.Min(newVal1, newVal2);
        slider.valueMax = Mathf.Max(newVal1, newVal2);
    }

    private void RandomizeSlider(Slider slider)
    {
        slider.value = Random.Range(slider.minValue, slider.maxValue);
    }

    public void ToggleSlidersLimit()
    {
        if(NumberOfParticles.maxValue == numParticlesLimit)
        {
            IncreaseSlidersLimit();
        }
        else
        {
            DecreaseSlidersLimit();
        }
    }

    public void IncreaseSlidersLimit(float multiplier = 5f)
    {
        DecreaseSlidersLimit(); // Using this as a reset to default limits, so it doesn't stack
        NumberOfParticles.maxValue = NumberOfParticles.maxValue * multiplier;
        NumberOfTypes.maxValue = NumberOfTypes.maxValue * multiplier;
        Forces.maxLimit = Forces.maxLimit * multiplier;
        MinDistances.maxLimit = MinDistances.maxLimit * multiplier;
        Radii.maxLimit = Radii.maxLimit * multiplier;
        PlayerPrefs.SetInt("SliderLimit", 1);
        warningMsg.SetActive(true);
    }

    public void DecreaseSlidersLimit()
    {
        Debug.Log(numParticlesLimit);
        NumberOfParticles.maxValue = numParticlesLimit;
        NumberOfTypes.maxValue = numTypesLimit;
        Forces.maxLimit = forcesLimit;
        MinDistances.maxLimit = minDistancesLimit;
        Radii.maxLimit = radiiLimit;
        PlayerPrefs.SetInt("SliderLimit", 0);
        warningMsg.SetActive(false);
    }

    public void SetSliders()
    {
        SimSettings settings = SettingsLoader.Instance.GetSettings();

        ScreenSpace.value = CameraScaler.Instance.GetScaleRatio(settings.ScreenSpace);

        NumberOfParticles.value = settings.NumberOfParticles;

        NumberOfTypes.value = settings.NumberOfTypes;

        Forces.valueMin = settings.forces.x;
        Forces.valueMax = settings.forces.y;

        MinDistances.valueMin = settings.minDistances.x;
        MinDistances.valueMax = settings.minDistances.y;

        Radii.valueMin = settings.radii.x;
        Radii.valueMax = settings.radii.y;

        Friction.value = settings.Friction;

        Dampening.value = settings.Dampening;

        RepulsionEffector.value = settings.RepulsionEffector;

        TimeScale.value = settings.TimeScale;
    }
}
