using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletBehaviour : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null) {             
            if (collision.gameObject.tag == "Enemy")
            {
                collision.gameObject.GetComponent<EnemyBehaviour>().TakeDamage(30);
                Debug.Log("Enemy hit");
                Destroy(gameObject);
            }
            else if (collision.gameObject.tag == "Player")
            {
                collision.gameObject.GetComponent<PlayerBehaviour>().TakeDamage(10);
                Destroy(gameObject);
            }
            else if (collision.gameObject.tag == "Floor")
            {
                Destroy(gameObject);
            }
        }
    }
}
