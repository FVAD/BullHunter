using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
// UI Navigation Helper.cs
public class UINavigationHelper : MonoBehaviour
{
    [SerializeField] private Selectable onUp;
    [SerializeField] private Selectable onDown;

    private void Update()
    {
        var current = EventSystem.current.currentSelectedGameObject;
        if (current == gameObject)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow))
                onDown?.Select();
            else if (Input.GetKeyDown(KeyCode.UpArrow))
                onUp?.Select();
        }
    }
}

