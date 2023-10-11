using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Analytics;
using Unity.Services.Core;
using UnityEngine.UI;

public class InitWithDefault : MonoBehaviour
{
    public GameObject analyticsCanvas;
    public Button consentButton;

    async void Start()
    {
        await UnityServices.InitializeAsync();

        AskForConsent();
    }

    void AskForConsent()
    {
        // ... show the player a UI element that asks for consent.
        analyticsCanvas.SetActive(true);
        consentButton.onClick.AddListener(ConsentGiven);
    }

    void ConsentGiven()
    {
        AnalyticsService.Instance.StartDataCollection();
        analyticsCanvas.SetActive(false);
    }
}
