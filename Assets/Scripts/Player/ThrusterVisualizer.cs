using NUnit.Framework;
using PrimeTween;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ThrusterVisualizer : MonoBehaviour
{
    private GameObject _exhaust;

    private float _targetThrust = 2;

    private Tween activeTween;

    private Player _player;

    private List<SpriteRenderer> flames = new List<SpriteRenderer>();
    private List<Color> originalFlameColors = new List<Color>();
    private List<Color> reverseColors = new List<Color>();

    private void Start()
    {
        _exhaust = transform.GetChild(0).gameObject;

        _player = transform.parent.GetComponent<Player>();

        // Cache the original colors of the flames
        flames = _exhaust.GetComponentsInChildren<SpriteRenderer>().ToList();
        foreach (var flame in flames)
        {
            originalFlameColors.Add(flame.color);
            Color invertedColor = new Color(1 - flame.color.r, 1 - flame.color.g, 1 - flame.color.b);
            reverseColors.Add(invertedColor);
        }
    }

    private void Update()
    {
        if (_player == null) return;

        // If the player is boosting, we want to change the color of the flames
        if (_player.movingBackwards)
        {
            for (int i = 0; i < flames.Count; i++)
            {
                flames[i].color = reverseColors[i];
            }
        }
        else if (_player.movingForwards)
        {
            for (int i = 0; i < flames.Count; i++)
            {
                flames[i].color = originalFlameColors[i];
            }
        }
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
