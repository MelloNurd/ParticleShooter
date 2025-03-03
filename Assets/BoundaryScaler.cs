using NaughtyAttributes;
using Unity.Cinemachine;
using UnityEngine;

public class BoundaryScaler : MonoBehaviour
{
    [SerializeField] CinemachineConfiner2D confiner;
    BoxCollider2D boxCollider;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        gameObject.transform.localScale = new Vector3(ParticleManager.Instance.ScreenSpace.x, ParticleManager.Instance.ScreenSpace.y, 1);
        confiner.BoundingShape2D = boxCollider;
        confiner.InvalidateBoundingShapeCache();
    }

    // Update is called once per frame
    void Update()
    {
        if (confiner.BoundingShape2D != boxCollider)
        {
            confiner.BoundingShape2D = boxCollider;
            confiner.InvalidateBoundingShapeCache();
        }
        if(gameObject.transform.localScale.x != ParticleManager.Instance.ScreenSpace.x || gameObject.transform.localScale.y != ParticleManager.Instance.ScreenSpace.y)
        {
            gameObject.transform.localScale = new Vector3(ParticleManager.Instance.ScreenSpace.x, ParticleManager.Instance.ScreenSpace.y, 1);
        }
    }
}
