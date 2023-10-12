using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using TMPro;
using UnityEngine.Experimental.Playables;

public class PlayerBehaviour : MonoBehaviour
{
    //public PlayerData playerData;
    public Player player;

    public string fileName;
    private StreamWriter sw;
    private StreamReader sr;
    private string fileContent;

    public static PlayerBehaviour Instance;

    public GameObject savedTextObject;
    private TMP_Text savedText;

    private void Awake()
    {
        if (Instance != null)
            Destroy(this.gameObject);
        else
            Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        savedText = savedTextObject.GetComponent<TMP_Text>();
        savedTextObject.SetActive(false);

        if (File.Exists(fileName))
        {
            sr = new StreamReader(Application.persistentDataPath + "/" + fileName, false);
            Debug.Log("Path: " + Application.persistentDataPath + "/" + fileName);
            fileContent = sr.ReadToEnd();
            Debug.Log("fileContent:" + fileContent);
            player = new Player();
            //players = new List<Player>();
            player = JsonUtility.FromJson<Player>(fileContent);
            //players = JsonUtility.FromJson<List<Player>>(fileContent);
            sr.Close();
        }
        /*else if (playerData != null)
        {
            player = new Player();
            UpdatePlayer();
        }*/
        else
        {
            player = new Player();
            player.lives = 3;
            player.maxLives = 3;
            player.health = 100;
            player.maxLevel = "ice_cream_shop";
            player.currentLevel = "";
            player.levelsCompleted = new List<int>();
            player.items = new List<GameObject>();
            player.weapons = new List<GameObject>();
            player.lastSaved = DateTime.Now.ToShortDateString() + " " + DateTime.Now.Hour + ":" + DateTime.Now.Minute;

            SavePlayer();
        }
    }

    public void SavePlayer()
    {
        string scene = SceneManager.GetActiveScene().name;

        if(scene.Equals("loader") || scene.Equals("menu_space") || scene.Equals("intro_panels"))
        {
            scene = player.maxLevel;
        }

        SavePlayer(scene);
    }

    public void SavePlayer(string scene)
    {
        if(scene.Equals("loader") || scene.Equals("menu_space") || scene.Equals("intro_panels"))
        {
            scene = player.maxLevel;
        } 
        else if (scene.StartsWith("level_"))
        {
            player.maxLevel = scene;
        }

        player.lastSaved = DateTime.Now.ToShortDateString() +" "+DateTime.Now.Hour+":"+DateTime.Now.Minute;
        player.currentLevel = scene;

        /*playerData.lives = player.lives;
        playerData.maxLives = player.maxLives;
        playerData.health = player.health;
        playerData.maxLevel = player.maxLevel;
        playerData.currentLevel = player.currentLevel;
        playerData.levelsCompleted = player.levelsCompleted;
        playerData.items = player.items;
        playerData.weapons = player.weapons;
        playerData.lastSaved = player.lastSaved;*/

        sw = new StreamWriter(Application.persistentDataPath + "/" + fileName, false);
        Debug.Log("Path: " + Application.persistentDataPath + "/" + fileName);
        fileContent = JsonUtility.ToJson(player);
        sw.Write(fileContent);
        sw.Close();

        if(fileContent != null)
        {
            StartCoroutine(SavedText());
        }
        else
        {
            Debug.Log("Error saving file");
        }
    }

    IEnumerator SavedText()
    {
        savedText.text = "Guardado: "+player.lastSaved;
        savedTextObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        savedTextObject.SetActive(false);
    }

    public void TakeDamage(int damage)
    {
        player.health -= damage;
        Debug.Log("Took Damage, Player health: " + player.health);
        if(player.health <= 0)
        {
            player.lives --;
            player.health = 100;
            HUDController.Instance.UpdateLives();
            Debug.Log("Lost one life");
            
        } 
        if (player.lives <= 0)
        {
            Debug.Log("Game Over");
            DeathController playerDeath = FindAnyObjectByType<DeathController>();
            playerDeath.OnPlayerDeath();
            player.maxLevel = SceneManager.GetActiveScene().name;
            player.lives = 3;
            player.health = 100;
            SavePlayer("ice_cream_shop");
        }
        //UpdatePlayer();
        Debug.Log("Player lives: " + player.lives);
    }

    public void Heal(int heal)
    {
        if(player.health >= 100)
        {
            Debug.Log("Player is already at full health");
            return;
        }
        player.health += heal;
        //playerData.health += heal;
    }

    /*private void UpdatePlayer()
    {
        player.lives = playerData.lives;
        player.health = playerData.health;
        player.maxLevel = playerData.maxLevel;
        player.currentLevel = playerData.currentLevel;
        player.levelsCompleted = playerData.levelsCompleted;
        player.items = playerData.items;
        player.weapons = playerData.weapons;
        player.lastSaved = playerData.lastSaved;
    }*/

    private void OnApplicationQuit()
    {
        string scene = SceneManager.GetActiveScene().name;
        if(!scene.Equals("menu_space") && !scene.Equals("loader") || scene.Equals("intro_panels"))
        {
            SavePlayer();
        }
    }

    public void ResetLives()
    {
        player.lives = player.maxLives;
        //playerData.lives = player.maxLives;
    }

}

[Serializable]
public class Player
{
    public int lives = 3;
    public int maxLives = 3;
    public float health = 100;
    public string maxLevel;
    public string currentLevel;
    public List<int> levelsCompleted;
    public List<GameObject> items;
    public List<GameObject> weapons;
    public string lastSaved;
}
