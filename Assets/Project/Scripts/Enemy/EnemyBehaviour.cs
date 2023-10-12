using System.Collections;
using System.Collections.Generic;
using Unity.Services.Analytics;
using UnityEngine;
using UnityEngine.Events;

public class EnemyBehaviour : MonoBehaviour
{
    private int health = 100;
    private AssignEnemyType enemyType;
    public UnityEvent onEnemyDeath;
    private string levelName;

    // Start is called before the first frame update
    void Start()
    {
        enemyType = GetComponent<AssignEnemyType>();
        levelName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Enemy took " + damage + " damage, Remaining health = " + health);
        if(health <= 0)
        {
            Die();
        }
    }

    public void Die()
    {
        AnalyticsService.Instance.CustomData("enemyKilled", new Dictionary<string, object>
        {
            { "enemyName", enemyType.enemy.GiveName() }, { "levelName", levelName }
        });

        Destroy(Instantiate(Resources.Load("FX/FX_Explosion"), transform.position, Quaternion.identity), 5f);
        onEnemyDeath.Invoke();
        Destroy(gameObject);
    }
}
