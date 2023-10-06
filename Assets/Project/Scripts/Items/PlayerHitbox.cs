using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    public PlayerItems player;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
        {
            EnemyBehaviour enemy = other.gameObject.GetComponent<EnemyBehaviour>();
            enemy.TakeDamage(player.attackDamage);
            player.CallItemOnHit(enemy);
        }
    }
}
