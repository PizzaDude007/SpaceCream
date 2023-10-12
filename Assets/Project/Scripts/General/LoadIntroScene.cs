using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadIntroScene : MonoBehaviour
{
    public PlayableDirector loadingDirector, introDirector;

    public static LoadIntroScene instance;

    public UnityEvent onStart;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        onStart.Invoke();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ActivateIntroCutScene()
    {
        introDirector.Play();
    }

    public void ActivateLoadingCutScene()
    {
        loadingDirector.Play();
    }

    IEnumerator LoadSceneWait(string sceneName)
    {
        yield return new WaitForSeconds((float)loadingDirector.duration);
        //SceneManager.LoadScene(sceneName);
        LoaderMainMenu.Instance.PlayGame(sceneName);
    }

    IEnumerator LoadSceneWait(string sceneName, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        //SceneManager.LoadScene(sceneIndex);
        LoaderMainMenu.Instance.PlayGame(sceneName);
    }

    public void ActivateLoadSceneWait(string sceneName)
    {
        ActivateIntroCutScene();
        StartCoroutine(LoadSceneWait(sceneName));
    }

    public void ActivateLoadSceneWait(string sceneName, float seconds)
    {
        ActivateIntroCutScene();
        StartCoroutine(LoadSceneWait(sceneName, seconds));
    }

    public void LoadSeconds()
    {
        ActivateIntroCutScene();
        StartCoroutine(LoadSceneWait("ice_cream_shop", 39.5f));
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
