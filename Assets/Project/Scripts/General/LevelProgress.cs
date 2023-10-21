using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelProgress : MonoBehaviour
{
    public float timePassed = 0f;
    public int itemsCollected = 0;
    public static int itemsToCollect = 10;
    public static float timeToComplete = 120f;
    public static float itemDropPercentage = 0.25f;
    private int actualItemsToCollect = 10;
    public bool levelComplete = false;
    [SerializeField] private GameObject battery100, battery75, battery25, battery0;
    public UpdateItemCanvas itemCanvas;

    public TMP_Text message;
    private string oldText;

    // Start is called before the first frame update
    void Start()
    {
        timePassed = 0f;
        itemsCollected = PlayerBehaviour.Instance.player.items.Count;
        actualItemsToCollect = itemsToCollect + itemsCollected;
        levelComplete = false;
        battery100.SetActive(false);
        battery75.SetActive(false);
        battery25.SetActive(false);
        battery0.SetActive(true);

        //oldText = message.text;
        oldText = "Menú de navegación";
    }

    // Update is called once per frame
    void Update()
    {
        timePassed += Time.deltaTime;
        if(timePassed >= timeToComplete || itemsCollected >= actualItemsToCollect)
        {
            levelComplete = true;
            battery75.SetActive(false);
            battery100.SetActive(true);
        } 
        else if(timePassed >= timeToComplete * 0.75f)
        {
            battery25.SetActive(false);
            battery75.SetActive(true);
        } 
        else if(timePassed >= timeToComplete * 0.25f)
        {
            battery0.SetActive(false);
            battery25.SetActive(true);
        }
    }

    public void AdvanceLevel()
    {
        if (levelComplete)
        {
            PlayerBehaviour.Instance.player.levelsCompleted.Add(SceneManager.GetActiveScene().name);
            LoaderMainMenu.Instance.LoadLevel();
        } 
        else
        {
            //Show some message
            StartCoroutine("ShowMessage");

        }
    }

    public void ReturnToShop()
    {
        if (levelComplete)
        {
            ResetRun();
            LoaderMainMenu.Instance.LoadLevel("ice_cream_shop");
        }
        else
        {
            //Show some message
            StartCoroutine("ShowMessage");
        }
    }

    IEnumerator ShowMessage()
    {
        message.text = "La batería aún no está llena, consigue más ingredientes o espera a que cargue";
        message.fontSize = 20;
        yield return new WaitForSeconds(5f);
        message.text = "Menú de Navegación";
        message.fontSize = 36;
    }

    public void PlayerDead()
    {
        //Drop random items
        for (int i = 0; i < System.Math.Ceiling(itemsCollected * 0.25); i++)
        {
            int randomItem = Random.Range(0, PlayerBehaviour.Instance.player.items.Count);
            itemCanvas.RemoveItem(PlayerBehaviour.Instance.player.items[randomItem]);
            PlayerBehaviour.Instance.player.items.RemoveAt(randomItem);
        }

        //LoaderMainMenu.Instance.LoadLevel("ice_cream_shop");
    }

    public void ResetRun()
    {
        PlayerBehaviour.Instance.player.levelsCompleted.Clear();
    }

    public void SetMessage(TMP_Text text)
    {
        message = text;
        //oldText = message.text;
    }

}
