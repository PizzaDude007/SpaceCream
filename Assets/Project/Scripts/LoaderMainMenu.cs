using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoaderMainMenu : MonoBehaviour
{
    public GameObject panelMenu;
    public GameObject panelOpciones;
    public GameObject canvasMenu;
    public GameObject sfxManager;
    public GameObject musicSource;
    public string[] escenas;
    private bool open;
    
    // Start is called before the first frame update
    void Start()
    {
        if (SceneManager.GetActiveScene().name == "menu_space")
        {
            ShowMenu();
            open = true;
            //SoundFxManager.Instance.sfxAudioSource.outputAudioMixerGroup = 
            SoundFxManager.Instance.PlayMusic(0);
        }
        else if (SceneManager.GetActiveScene().name == "loader")
        {
            SoundFxManager.Instance.PlayMusic(0);
            SceneManager.LoadScene("menu_space", LoadSceneMode.Single);    
        }
        else
        {
            SoundFxManager.Instance.StopMusic();
            SoundFxManager.Instance.PlayAmbient();
            DesactivarPaneles();
            open = false;
        }
        panelMenu = canvasMenu.transform.Find("PanelMenu").gameObject;
        panelOpciones = canvasMenu.transform.Find("PanelOpciones").gameObject;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !open)
        {
            open = true;
            ShowOptions();
            Time.timeScale = 0f;
        } 
        else if (Input.GetKeyDown(KeyCode.Escape) && open)
        {
            open = false;
            DesactivarPaneles();
            Time.timeScale = 1f;
        }
    }

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(canvasMenu);
        //DontDestroyOnLoad(panelOpciones);
        //DontDestroyOnLoad(panelMenu);
        DontDestroyOnLoad(sfxManager);
        DontDestroyOnLoad(musicSource);
    }

    public void PlayGame()
    {
        DesactivarPaneles();
        SoundFxManager.Instance.PlayAmbient();
        open = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenas[Random.Range(0, escenas.Length)]);
    }

    public void ShowOptions()
    {
        DesactivarPaneles();
        panelOpciones.SetActive(true);
    }

    public void ShowMenu()
    {
        if (SceneManager.GetActiveScene().name != "menu_space")
        {
            SoundFxManager.Instance.StopMusic();
            SoundFxManager.Instance.PlayMusic(0);
            SceneManager.LoadScene("menu_space");
        }

        DesactivarPaneles();
        panelMenu.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void DesactivarPaneles()
    {
        panelMenu.SetActive(false);
        panelOpciones.SetActive(false);
    }

}
