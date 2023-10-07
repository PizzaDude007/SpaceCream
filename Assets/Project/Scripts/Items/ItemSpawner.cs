using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SpawnRandomItem()
    {
        int random = UnityEngine.Random.Range(0, 10);
        switch (random)
        {
            case 0:
                GameObject gameObject = (GameObject)Resources.Load("Items/HealingItem", typeof(GameObject));
                Instantiate(gameObject, transform.position, Quaternion.identity);
                break;
            case 1:
                GameObject gameObject1 = (GameObject)Resources.Load("Items/FireDamageItem", typeof(GameObject));
                Instantiate(gameObject1, transform.position, Quaternion.identity);
                break;
            case 2:
                GameObject gameObject2 = (GameObject)Resources.Load("Items/HealingAreaItem", typeof(GameObject));
                Instantiate(gameObject2, transform.position, Quaternion.identity);
                break;
            default:
                
                break;
        }
    }
}
