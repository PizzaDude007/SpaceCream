using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class InitWithDefault : MonoBehaviour
{
    public GameObject analyticsCanvas;
    public Button consentButton;
    public UnityEvent onConsent;

    async void Start()
    {
        await UnityServices.InitializeAsync();
        SceneManager.LoadScene("menu_space", LoadSceneMode.Additive);

        AskForConsent();
    }

    void AskForConsent()
    {
        // ... show the player a UI element that asks for consent.
        analyticsCanvas.SetActive(true);
        consentButton.onClick.AddListener(ConsentGiven);
    }

    public void ConsentGiven()
    {
        AnalyticsService.Instance.StartDataCollection();
        analyticsCanvas.SetActive(false);
        onConsent.Invoke();
        SceneManager.UnloadSceneAsync("loader");    
    }
}
