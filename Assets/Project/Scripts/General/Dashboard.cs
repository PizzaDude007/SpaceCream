using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class Dashboard : MonoBehaviour
{
    public UnityEvent onPlayerEnter;
    public UnityEvent onPlayerExit;
    public UnityEvent onDashBoardOpen;

    public GameObject backButton;

    private bool isPlayerInside = false;

    public PlayerBehaviour playerBehaviour;

    // Start is called before the first frame update
    void Start()
    {
        playerBehaviour = PlayerBehaviour.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        //If player presses E, open dashboard
        if (isPlayerInside && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Joystick1Button1)))
        {
            onDashBoardOpen.Invoke();
            Debug.Log("Open Dashboard");
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(backButton);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger");

        if ((other.gameObject.tag == "Player"))
        {
            onPlayerEnter.Invoke();
            isPlayerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((other.gameObject.tag == "Player"))
        {
            onPlayerExit.Invoke();
            isPlayerInside = false;
        }
    }

    public void SetTimeScale(float time)
    {
        Time.timeScale = time;
    }

    public void LoadRandomLevel()
    {
        SoundFxManager.Instance.PlayAmbient();
        Time.timeScale = 1f;
        playerBehaviour.ResetLives();
        string scene = LoaderMainMenu.Instance.escenas[Random.Range(0, LoaderMainMenu.Instance.escenas.Length)];
        PlayerBehaviour.Instance.SavePlayer(scene);
        SceneManager.LoadScene(scene);
    }
}
