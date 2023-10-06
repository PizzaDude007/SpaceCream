using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateItemCanvas : MonoBehaviour
{
    public GameObject[] itemParent;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InstanceItem(string itemName)
    {
        bool found = false;
        foreach (GameObject item in itemParent)
        {
            if(item.transform.childCount == 0) {     
                found = true;

                GameObject itemResource = (GameObject)Resources.Load("ItemIcon3D/"+itemName, typeof(GameObject));

                GameObject newItem = Instantiate(itemResource, item.transform.position, Quaternion.Euler(Vector3.zero), item.transform);

                if(newItem != null)
                    Debug.Log("Added " + itemName + " to inventory");
                else
                    Debug.Log("Failed to add " + itemName + " to inventory");
                break;
            }
        }
        if (!found)
        {
            Debug.Log("No empty item slot");
        }
    }

    public void RemoveItem(string itemName)
    {
        bool found = false;
        foreach (GameObject item in itemParent)
        {
            if (item.transform.childCount != 0)
            {
                if (item.transform.GetChild(0).name == itemName)
                {
                    found = true;
                    Destroy(item.transform.GetChild(0).gameObject);
                    Debug.Log("Removed " + itemName + " from inventory");
                    break;
                }
            }
        }
        if (!found)
        {
            Debug.Log("No item found");
        }
    }
}
