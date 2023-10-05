using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using JetBrains.Annotations;
using UnityEngine.SceneManagement;

public class PlayerBehaviour : MonoBehaviour
{
    public PlayerData playerData;
    public Player player;

    public string fileName;
    private StreamWriter sw;
    private StreamReader sr;
    private string fileContent;

    public static PlayerBehaviour Instance;

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
        else if (playerData != null)
        {
            player = new Player();
            UpdatePlayer();
        }
        else
        {
            player = new Player();
            player.lives = 3;
            player.health = 100;
            player.maxLevel = "level_desert";
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

        if(scene.Equals("loader") || scene.Equals("main_menu"))
        {
            scene = player.maxLevel;
        }

        SavePlayer(scene);
    }

    public void SavePlayer(string scene)
    {
        player.lastSaved = DateTime.Now.ToShortDateString() +" "+DateTime.Now.Hour+":"+DateTime.Now.Minute;
        player.currentLevel = scene;

        playerData.lives = player.lives;
        playerData.health = player.health;
        playerData.maxLevel = player.maxLevel;
        playerData.currentLevel = player.currentLevel;
        playerData.levelsCompleted = player.levelsCompleted;
        playerData.items = player.items;
        playerData.weapons = player.weapons;
        playerData.lastSaved = player.lastSaved;

        sw = new StreamWriter(Application.persistentDataPath + "/" + fileName, false);
        Debug.Log("Path: " + Application.persistentDataPath + "/" + fileName);
        fileContent = JsonUtility.ToJson(player);
        sw.Write(fileContent);
        sw.Close();
    }

    public void TakeDamage(int damage)
    {
        playerData.health -= damage;
        Debug.Log("Took Damage, Player health: " + playerData.health);
        if(playerData.health <= 0)
        {
            playerData.lives --;
            playerData.health = 100;
            player.lives = playerData.lives;
            HUDController.Instance.UpdateLives();
            Debug.Log("Lost one life");
            
        } 
        if (playerData.lives <= 0)
        {
            Debug.Log("Game Over");
            playerData.maxLevel = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene("ice_cream_shop");
            playerData.lives = 3;
            playerData.health = 100;
            SavePlayer("ice_cream_shop");
        }
        UpdatePlayer();
        Debug.Log("Player lives: " + playerData.lives);
    }

    private void UpdatePlayer()
    {
        player.lives = playerData.lives;
        player.health = playerData.health;
        player.maxLevel = playerData.maxLevel;
        player.currentLevel = playerData.currentLevel;
        player.levelsCompleted = playerData.levelsCompleted;
        player.items = playerData.items;
        player.weapons = playerData.weapons;
        player.lastSaved = playerData.lastSaved;
    }

    private void OnApplicationQuit()
    {
        SavePlayer();
    }
}

[Serializable]
public class Player
{
    public int lives = 3;
    public float health = 100;
    public string maxLevel;
    public string currentLevel;
    public List<int> levelsCompleted;
    public List<GameObject> items;
    public List<GameObject> weapons;
    public string lastSaved;
}
