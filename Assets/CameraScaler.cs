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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ScaleCamera()
    {
        myCamera.Lens.OrthographicSize = SimParticleManager.Instance.ScreenSpace.y / 2;
    }

}
