using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignEnemyType : MonoBehaviour
{
    public EnemyType enemy;
    public EnemyTypes enemyTypes;
    public int AttackDamage;

    // Start is called before the first frame update
    void Start()
    {
        enemy = AssignEnemy(enemyTypes);
        AttackDamage = enemy.GetAttackDamage();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public EnemyType AssignEnemy(EnemyTypes enemyToAssign)
    {
        switch (enemyToAssign)
        {
            case EnemyTypes.Crab:
                return new CrabEnemy();
            case EnemyTypes.SpecialCrab:
                return new SpecialCrabEnemy();
            default:
                return new CrabEnemy();
        }
    }
}

public enum EnemyTypes
{
    Crab,
    SpecialCrab
}
