using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ButtonTextColor : MonoBehaviour
{
    [SerializeField]
    private Color newColor;
    
    private Color oldColor;
    private TMP_Text text;

    private void Start()
    {
        text = gameObject.GetComponentInChildren<TMP_Text>();
        oldColor = text.color;
        text.alpha = 1f;
    }

    public void ChangeColor()
    {
        text.color = newColor;
        text.alpha = 1f;
    }

    public void ResetColor()
    {
        text.color = oldColor;
        text.alpha = 1f;
    }
}
