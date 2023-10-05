using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerData", menuName = "ScriptableObjects/PlayerData", order = 1)]
public class PlayerData : ScriptableObject
{
    public int lives = 3;
    public float health = 100;
    public string maxLevel;
    public string currentLevel;
    public List<int> levelsCompleted;
    public List<GameObject> items;
    public List<GameObject> weapons;
    public DateTime lastSaved;
}
