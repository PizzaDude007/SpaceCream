using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DeactivateCanvas : MonoBehaviour
{
    [SerializeField]
    private GameObject canvas;

    //private bool open;

    // Start is called before the first frame update
    void Start()
    {
        //open = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (SceneManager.GetActiveScene().name.Contains("level") && !LoaderMainMenu.Instance.open) {
            canvas.SetActive(true);
        }
        else
            canvas.SetActive(false);
    }
}
