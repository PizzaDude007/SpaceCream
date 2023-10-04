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
            player.lives = playerData.lives;
            player.health = playerData.health;
            player.maxLevel = playerData.maxLevel;
            player.currentLevel = playerData.currentLevel;
            player.levelsCompleted = playerData.levelsCompleted;
            player.items = playerData.items;
            player.weapons = playerData.weapons;
            player.lastSaved = playerData.lastSaved;
        }
        else
        {
            player = new Player();
            player.lives = 3;
            player.health = 100;
            player.maxLevel = 1;
            player.currentLevel = "";
            player.levelsCompleted = new List<int>();
            player.items = new List<GameObject>();
            player.weapons = new List<GameObject>();
            player.lastSaved = DateTime.Now;
        }
    }

    public void SavePlayer()
    {
        player.lastSaved = DateTime.Now;
        player.currentLevel = SceneManager.GetActiveScene().name;

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
        if(playerData.health <= 0)
        {
            playerData.lives --;
            playerData.health = 100;
        }
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
    public int maxLevel;
    public string currentLevel;
    public List<int> levelsCompleted;
    public List<GameObject> items;
    public List<GameObject> weapons;
    public DateTime lastSaved;
}
