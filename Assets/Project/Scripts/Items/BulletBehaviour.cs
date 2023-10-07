using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    public PlayerItems player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindAnyObjectByType<PlayerItems>();

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(gameObject.transform.position.y <= 0.1)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null) {             
            if (collision.gameObject.layer == 8) //Layer 8 = Enemy
            {
                EnemyBehaviour enemy = collision.gameObject.GetComponent<EnemyBehaviour>();
                enemy.TakeDamage(player.attackDamage);
                player.CallItemOnHit(enemy);
                //Displays damage dealt to enemy
                enemy.gameObject.GetComponent<EnemyDamageOutput>().TakeDamage(player.attackDamage, player.attackColor);
                Debug.Log("Enemy hit");
                Destroy(Instantiate(Resources.Load("FX/FX_BloodShot"), collision.GetContact(0).point, Quaternion.identity), 2f);
                Destroy(gameObject);
            }
            else if (collision.gameObject.layer == 6) //Layer 6 = Player
            {
                collision.gameObject.GetComponent<PlayerBehaviour>().TakeDamage(10);
                Destroy(gameObject);
            }
            else if (collision.gameObject.layer == 7) //Layer 7 = Floor
            {
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Hit "+collision.gameObject.name+", Layer = "+collision.gameObject.layer);
                Destroy(gameObject);
            }
        }
    }
}
