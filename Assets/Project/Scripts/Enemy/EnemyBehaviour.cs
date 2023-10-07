using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    private int health = 100;

    // Start is called before the first frame update
    void Start()
    {
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
        Destroy(gameObject);
    }
}
