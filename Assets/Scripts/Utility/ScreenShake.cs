using System.Collections;
using UnityEngine;

public class ScreenShake : MonoBehaviour
{
    public bool shake = false;
    public AnimationCurve curve;
    public float duration = .5f;
    [SerializeField] GameObject red;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(shake)
        {
            StartCoroutine(Shaking());
            shake = false;
        }
    }

    IEnumerator Shaking()
    {
        Vector3 originalPosition = transform.position;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            red.SetActive(true);
            elapsed += Time.deltaTime;
            float strength = curve.Evaluate(elapsed / duration);
            transform.position = originalPosition + Random.insideUnitSphere * strength;
            if (elapsed >= duration * 0.5f)
            {
                red.SetActive(false);
            }
            yield return null;
        }

        transform.localPosition = originalPosition;
    }
}
