using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class EnemyType
{
    public abstract string GiveName();
    public abstract int GetAttackDamage();
    public abstract GameObject GetEnemyPrefab();

    public virtual void Update(EnemyBehaviour enemy)
    {

    }
}

public class CrabEnemy : EnemyType
{
    private int attackDamage = 10;
    private GameObject enemyPrefab = (GameObject)Resources.Load("Enemies/Crab", typeof(GameObject));

    public override string GiveName()
    {
        return "Crab";
    }

    public override int GetAttackDamage()
    {
        return attackDamage;
    }

    public override GameObject GetEnemyPrefab()
    {
        return enemyPrefab;
    }
}

public class SpecialCrabEnemy : EnemyType
{
    private int attackDamage = 20;
    private GameObject enemyPrefab = (GameObject)Resources.Load("Enemies/SpecialCrab", typeof(GameObject));

    public override string GiveName()
    {
        return "Special Crab";
    }

    public override int GetAttackDamage()
    {
        return attackDamage;
    }

    public override GameObject GetEnemyPrefab()
    {
        return enemyPrefab;
    }
}