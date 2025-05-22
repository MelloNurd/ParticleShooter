using NaughtyAttributes;
using Unity.Cinemachine;
using UnityEngine;

public class CameraScaler : MonoBehaviour
{
    public static CameraScaler Instance { get; private set; }

    [ReadOnly, SerializeField] private CinemachineCamera _myCamera;

    private Vector2 _startScreenSpace;

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
        return new Vector2(_startScreenSpace.x * scale, _startScreenSpace.y * scale);
    }

    public float GetScaleRatio(Vector2 current)
    {
        Debug.Log("calculated: " + current.magnitude / _startScreenSpace.magnitude);
        return current.magnitude / _startScreenSpace.magnitude;
    }

    public float CalculateScale()
    {
        Debug.Log(SettingsLoader.Instance.screenSpace.y * 0.5f);
        return SettingsLoader.Instance.screenSpace.y * 0.5f;
    }

    public void ScaleCamera()
    {
        _myCamera.Lens.OrthographicSize = CalculateScale();
    }
}
