using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHitbox : MonoBehaviour
{
    public PlayerItems player;
    private Vector3 groundNormal;

    private void OnTriggerEnter(Collider other)
    {
        /*if (other.gameObject.layer == 8) //Enemy layer
        {
            EnemyBehaviour enemy = other.gameObject.GetComponent<EnemyBehaviour>();
            enemy.TakeDamage(player.attackDamage);
            player.CallItemOnHit(enemy);
        }*/
        if(other.gameObject.layer == 7) //Floor layer
        {
            groundNormal = other.gameObject.transform.up;
        }
    }

    public Vector3 GetGroundNormal()
    {
        return groundNormal;
    }
}
