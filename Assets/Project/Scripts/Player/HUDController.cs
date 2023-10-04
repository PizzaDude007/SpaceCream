using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    public GameObject vidasPanel;
    public static HUDController Instance;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateLives();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    void UpdateLives()
    {
        for (int i = 0; i < vidasPanel.transform.childCount; i++)
        {
            if (i < PlayerBehaviour.Instance.player.lives)
            {
                vidasPanel.transform.GetChild(i).gameObject.SetActive(true);
                Debug.Log("Vida " + i + " activada");
            }
            else
            {
                vidasPanel.transform.GetChild(i).gameObject.SetActive(false);
            }
        }
    }
}
