using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyBehaviour : MonoBehaviour
{
    private int health = 100;
    private AssignEnemyType enemyType;
    public UnityEvent onEnemyDeath;

    // Start is called before the first frame update
    void Start()
    {
        enemyType = GetComponent<AssignEnemyType>();
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
        Destroy(Instantiate(Resources.Load("FX/FX_Explosion"), transform.position, Quaternion.identity), 5f);
        onEnemyDeath.Invoke();
        Destroy(gameObject);
    }
}
