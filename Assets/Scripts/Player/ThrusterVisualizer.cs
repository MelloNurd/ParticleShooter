using PrimeTween;
using UnityEngine;

public class ThrusterVisualizer : MonoBehaviour
{
    private GameObject _exhaust;

    private float _targetThrust = 2;

    private Tween activeTween;

    private void Start()
    {
        _exhaust = transform.GetChild(0).gameObject;
    }

    private void FixedUpdate()
    {
        // We want to apply random jittering to the scale to give it a organic flame effect
        if (_targetThrust > 0)
        {
            _exhaust.transform.localScale = new Vector3(1, Random.Range(0.85f, 1.15f) * _targetThrust, 0);
        }
        else
        {
            _exhaust.transform.localScale = new Vector3(1, 0, 0);
        }
    }

    public void SetThrust(float thrust)
    {
        if (thrust < 0) thrust = 0;

        activeTween.Stop();
        activeTween = Tween.Custom(_targetThrust, thrust, 0.5f, onValueChange: newVal => _targetThrust = newVal);
    }
}
