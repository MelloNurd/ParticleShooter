using NaughtyAttributes;
using Unity.Cinemachine;
using UnityEngine;

public class CameraScaler : MonoBehaviour
{
    CinemachineCamera myCamera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        myCamera = GetComponent<CinemachineCamera>();
        ScaleCamera();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ScaleCamera()
    {
        myCamera.Lens.OrthographicSize = SettingsLoader.Instance.screenSpace.y / 2;
    }

}
