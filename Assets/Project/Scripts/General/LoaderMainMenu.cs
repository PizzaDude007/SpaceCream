using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;
using UnityEngine.EventSystems;

public class LoaderMainMenu : MonoBehaviour
{
    public static LoaderMainMenu Instance;

    public GameObject pauseFirstButton, optionsFirstButton, menuFirstButton, optionsClosedButton;

    public GameObject panelMenu;
    public GameObject panelOpciones;
    public GameObject panelPausa;
    public GameObject canvasMenu;
    public GameObject sfxManager;
    public GameObject musicSource;
    public GameObject PlayerData;
    public GameObject HUDCanvas;
    public string[] escenas;
    public bool open;

    public GameObject opcionesGenerales;
    public GameObject opcionesGraficos;
    public GameObject opcionesSonido;
    public GameObject opcionesControles;

    public AudioMixer mainAudioMixer;

    [SerializeField]
    private GameObject opcionesButton, graficosButton, sonidoButton, controlesButton;

    [SerializeField]
    private Slider musicSlider, SFxSlider, ambientSlider, masterSlider;

    private float currentMusicVolume, currentSFxVolume, currentAmbientVolume, currentMasterVolume;

    public TMP_Dropdown resolutionDropdown;
    public TMP_Dropdown qualityDropdown;
    public TMP_Dropdown textureDropdown;
    public TMP_Dropdown AADropdown;
    Resolution[] resolutions;

    // Start is called before the first frame update
    void Start()
    {
        // Hides the cursor...
        Cursor.visible = false;

        // Locks the cursor
        Cursor.lockState = CursorLockMode.Locked;

        CreateResolutionDropdown();
        ShowMenu();
        if(mainAudioMixer.GetFloat("masterVolume", out currentMasterVolume))
            masterSlider.value = currentMasterVolume;
        if (mainAudioMixer.GetFloat("musicVolume", out currentMusicVolume))
            musicSlider.value = currentMusicVolume;
        if (mainAudioMixer.GetFloat("ambientVolume", out currentAmbientVolume))
            ambientSlider.value = currentAmbientVolume;
        if (mainAudioMixer.GetFloat("SFxVolume", out currentSFxVolume))
            SFxSlider.value = currentSFxVolume;

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
        {
            if(EventSystem.current.currentSelectedGameObject == null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(menuFirstButton);
            }

            if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Joystick1Button7)))
            {
                ShowMenu();
            }

            return;
        }
        
        if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Joystick1Button7) || !Application.isFocused) && !open)
        {
            open = true;
            ShowPause();
            Time.timeScale = 0f;
        } 
        else if ((Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Joystick1Button7)) && open)
        {
            ReturnToGame();
        }
    }

    void Awake()
    {
        if (Instance != null)
            Destroy(this.gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(this.gameObject);
        DontDestroyOnLoad(canvasMenu);
        //DontDestroyOnLoad(panelOpciones);
        //DontDestroyOnLoad(panelMenu);
        DontDestroyOnLoad(sfxManager);
        DontDestroyOnLoad(musicSource);
        DontDestroyOnLoad(PlayerData);
        //DontDestroyOnLoad(HUDCanvas);
    }

    public void PlayGame()
    {
        DesactivarPaneles();
        SoundFxManager.Instance.PlayAmbient();
        open = false;
        Time.timeScale = 1f;
        string scene = escenas[Random.Range(0, escenas.Length)];
        PlayerBehaviour.Instance.SavePlayer(scene);
        SceneManager.LoadScene(scene);
    }

    public void ContinueGame()
    {
        DesactivarPaneles();
        SoundFxManager.Instance.PlayAmbient();
        open = false;
        Time.timeScale = 1f;
        SceneManager.LoadScene(PlayerBehaviour.Instance.player.currentLevel);
        Debug.Log("Loading level: " + PlayerBehaviour.Instance.player.currentLevel);
    }

    public void ReturnToGame()
    {
        open = false;
        if (SceneManager.GetActiveScene().name == "menu_space")
            ShowMenu();
        else
            DesactivarPaneles();
        Time.timeScale = 1f;
    }

    public void ShowOptions()
    {
        DesactivarPaneles();
        panelOpciones.SetActive(true);
        OpcionesGenerales();

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(optionsFirstButton);
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

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(menuFirstButton);
    }

    public void ShowPause()
    {
        DesactivarPaneles();
        panelPausa.SetActive(true);

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(pauseFirstButton);
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

//Para el audio
    public void OnMasterVolumeChange(float volume)
    {
        mainAudioMixer.SetFloat("masterVolume", volume);
    }
    
    public void OnMusicVolumeChange(float volume)
    {
        mainAudioMixer.SetFloat("musicVolume", volume);
    }

    public void OnAmbientVolumeChange(float volume)
    {
        mainAudioMixer.SetFloat("ambientVolume", volume);
    }

    public void OnSFxVolumeChange(float volume)
    {
        mainAudioMixer.SetFloat("SFxVolume", volume);
    }

//Para opciones graficas
    public void SetFullScreen(bool isFullScreen)
    {
        Screen.fullScreen = isFullScreen;
    }
    
    public void SetTextureQuality(int textureIndex)
    {
        QualitySettings.globalTextureMipmapLimit = textureIndex;
        qualityDropdown.value = 4;
    }

    public void SetAntiAliasing(int AAIndex)
    {
        QualitySettings.antiAliasing = AAIndex;
        qualityDropdown.value = 4;
    }

    public void SetResolution(int resolutionIndex)
    {
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    public void SetQuality(int qualityIndex)
    {
        //No se esta utilizando el modo "custom"
        if(qualityIndex != 4)
            QualitySettings.SetQualityLevel(qualityIndex);

        switch (qualityIndex)
        {
            case 0: //low
                textureDropdown.value = 1;
                AADropdown.value = 0;
                break;
            case 1: //medium
                textureDropdown.value = 0;
                AADropdown.value = 0;
                break;
            case 2: //high
                textureDropdown.value = 0;
                AADropdown.value = 1;
                break;

        }

        qualityDropdown.value = qualityIndex;
    }

    public void SaveSettings()
    {
        PlayerPrefs.SetInt("QualitySettingPreference", qualityDropdown.value);
        PlayerPrefs.SetInt("ResolutionPreference", resolutionDropdown.value);
        PlayerPrefs.SetInt("TextureQualityPreference", textureDropdown.value);
        PlayerPrefs.SetInt("AntiAliasingPreference", AADropdown.value);
        PlayerPrefs.SetInt("FullscreenPreference", Convert.ToInt32(Screen.fullScreen));
        //PlayerPrefs.SetFloat("VolumePreference", currentVolume);
    }

    public void LoadSettings(int currentResolutionIndex)
    {
        if (PlayerPrefs.HasKey("QualitySettingPreference"))
            qualityDropdown.value = PlayerPrefs.GetInt("QualitySettingPreference");
        else
            qualityDropdown.value = 3;
        
        if (PlayerPrefs.HasKey("ResolutionPreference"))
            resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionPreference");
        else
            resolutionDropdown.value = currentResolutionIndex;
        
        if (PlayerPrefs.HasKey("TextureQualityPreference"))
            textureDropdown.value = PlayerPrefs.GetInt("TextureQualityPreference");
        else
            textureDropdown.value = 0;
        
        if (PlayerPrefs.HasKey("AntiAliasingPreference"))
            AADropdown.value = PlayerPrefs.GetInt("AntiAliasingPreference");
        else
            AADropdown.value = 1;
        
        if (PlayerPrefs.HasKey("FullscreenPreference"))
            Screen.fullScreen = Convert.ToBoolean(PlayerPrefs.GetInt("FullscreenPreference"));
        else
            Screen.fullScreen = true;
        
        //if (PlayerPrefs.HasKey("VolumePreference"))
        //    volumeSlider.value = PlayerPrefs.GetFloat("VolumePreference");
        //else
        //    volumeSlider.value = PlayerPrefs.GetFloat("VolumePreference");
    }

    private void CreateResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        resolutions = Screen.resolutions;
        int currentResolutionIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);
            if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.RefreshShownValue();
        LoadSettings(currentResolutionIndex);

    }
}
