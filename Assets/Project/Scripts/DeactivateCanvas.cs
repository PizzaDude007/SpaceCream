using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivateCanvas : MonoBehaviour
{
    [SerializeField]
    private GameObject canvas;

    private bool open;

    // Start is called before the first frame update
    void Start()
    {
        open = false;
    }

    // Update is called once per frame
    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.Escape) || !Application.isFocused) && !open)
        {
            open = true;
            canvas.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && open)
        {
            open = false;
            canvas.SetActive(true);
        }
    }
}
