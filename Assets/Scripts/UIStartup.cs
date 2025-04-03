using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIStartup : MonoBehaviour
{
    private UIDocument _document;
    private VisualElement _root;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _document = GetComponent<UIDocument>();
        _root = _document.rootVisualElement;

        // Schedule delayed animation start
        _root.schedule.Execute(() =>
        {
            AnimateStartup(_root);
        }).StartingIn(100); // Delay in ms
    }

    void AnimateStartup(VisualElement ve)
    {
        var originalClasses = new List<string>(ve.GetClasses());

        foreach (var className in originalClasses)
        {
            ve.AddToClassList($"{className}-started");
        }

        foreach (var child in ve.Children())
        {
            AnimateStartup(child);
        }
    }
}
