using NaughtyAttributes;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.UI;

public class CameraScaler : MonoBehaviour
{
    public static CameraScaler Instance { get; private set; }

    [ReadOnly, SerializeField] private CinemachineCamera _myCamera;

    private Vector2 _startScreenSpace;

    public bool shapeChanged = false;
    public float lastScale = 1f;

    [SerializeField] Slider screenSpaceSlider;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        _myCamera = GetComponent<CinemachineCamera>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ScaleCamera();
        _startScreenSpace = SettingsLoader.Instance.screenSpace;
    }

    public Vector2 AdjustByScale(float scale)
    {
        // Define a maximum scale factor (adjust as needed)
        float clampedScale = Mathf.Clamp(scale, 0.1f, 5f);
        return new Vector2(_startScreenSpace.x * clampedScale, _startScreenSpace.y * clampedScale);
    }

    public float GetScaleRatio(Vector2 current)
    {
        return current.magnitude / _startScreenSpace.magnitude;
    }

    public float CalculateScale()
    {
        return SettingsLoader.Instance.screenSpace.y * 0.5f;
    }

    public void ScaleCamera()
    {
        _myCamera.Lens.OrthographicSize = CalculateScale();
    }

    public void ScreenShapeChanged(Vector2 oldScreenSpace, Vector2 screenSpace)
    {
        lastScale = GetScaleRatio(oldScreenSpace);
        _startScreenSpace = screenSpace;
        shapeChanged = true;
    }
}
