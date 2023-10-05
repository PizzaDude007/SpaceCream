using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        //If the scene is not a level, deactivate the canvas
        if (!SceneManager.GetActiveScene().name.Contains("level")){ 
            canvas.SetActive(false);
        }//If the scene is a level, and the pause menu is not open, activate the canvas
        else
        {
            canvas.SetActive(!open);
        }
      
    }
}
