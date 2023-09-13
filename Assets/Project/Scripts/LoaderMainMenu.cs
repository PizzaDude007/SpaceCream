using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoaderMainMenu : MonoBehaviour
{
    public GameObject panelMenu;
    public GameObject panelOpciones;
    public GameObject panelPausa;
    public GameObject canvasMenu;
    public GameObject sfxManager;
    public GameObject musicSource;
    public string[] escenas;
    private bool open;

    public GameObject opcionesGenerales;
    public GameObject opcionesGraficos;
    public GameObject opcionesSonido;
    public GameObject opcionesControles;

    [SerializeField]
    private GameObject opcionesButton, graficosButton, sonidoButton, controlesButton;
    
    // Start is called before the first frame update
    void Start()
    {
        ShowMenu();
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
        if (SceneManager.GetActiveScene().name.Equals("menu_space"))
            return;
        
        if ((Input.GetKeyDown(KeyCode.Escape) || !Application.isFocused) && !open)
        {
            open = true;
            ShowPause();
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
        OpcionesGenerales();
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

    public void ShowPause()
    {
        DesactivarPaneles();
        panelPausa.SetActive(true);
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void DesactivarPaneles()
    {
        panelMenu.SetActive(false);
        panelOpciones.SetActive(false);
        panelPausa.SetActive(false);
    }

    private void DesactivarPanelesOpciones()
    {
        opcionesGenerales.SetActive(false);
        opcionesGraficos.SetActive(false);
        opcionesSonido.SetActive(false);
        opcionesControles.SetActive(false);
        
        opcionesButton.GetComponent<ButtonTextColor>().ResetColor();
        graficosButton.GetComponent<ButtonTextColor>().ResetColor();
        sonidoButton.GetComponent<ButtonTextColor>().ResetColor();
        controlesButton.GetComponent<ButtonTextColor>().ResetColor();
    }

    public void OpcionesGenerales()
    {
        DesactivarPanelesOpciones();
        opcionesGenerales.SetActive(true);
        opcionesButton.GetComponent<ButtonTextColor>().ChangeColor();
    }

    public void OpcionesGraficos()
    {
        DesactivarPanelesOpciones();
        opcionesGraficos.SetActive(true);
        graficosButton.GetComponent<ButtonTextColor>().ChangeColor();
    }
    
    public void OpcionesSonido()
    {
        DesactivarPanelesOpciones();
        opcionesSonido.SetActive(true);
        sonidoButton.GetComponent<ButtonTextColor>().ChangeColor();
    }
    
    public void OpcionesControles()
    {
        DesactivarPanelesOpciones();
        opcionesControles.SetActive(true);
        controlesButton.GetComponent<ButtonTextColor>().ChangeColor();
    }
}
