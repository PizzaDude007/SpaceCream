using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadIntroScene : MonoBehaviour
{
    public PlayableDirector loadingDirector, introDirector;

    public static LoadIntroScene instance;

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
}
