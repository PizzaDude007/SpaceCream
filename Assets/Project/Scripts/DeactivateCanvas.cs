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
        open = LoaderMainMenu.Instance.open;

        if (open)
            canvas.SetActive(false);
        else
            canvas.SetActive(true);
    }
}
