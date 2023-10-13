using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UpdateItemCanvas : MonoBehaviour
{
    public GameObject[] itemParent;
    public GameObject[] stacksText;
    private Dictionary<string, int> itemIndex;

    // Start is called before the first frame update
    void Start()
    {
        itemIndex = new Dictionary<string, int>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InstanceItem(string itemName)
    {
        bool found = false;
        int index = 0;
        foreach (GameObject item in itemParent)
        {
            if (item.transform.childCount == 0) {
                found = true;

                GameObject itemResource = (GameObject)Resources.Load("ItemIcon3D/" + itemName, typeof(GameObject));

                GameObject newItem = Instantiate(itemResource, item.transform.position, Quaternion.Euler(Vector3.zero), item.transform);
                itemIndex.Add(itemName, index);

                if (newItem != null)
                    Debug.Log("Added " + itemName + " to inventory");
                else
                    Debug.Log("Failed to add " + itemName + " to inventory");
                break;
            }
            index++;
        }
        if (!found)
        {
            Debug.Log("No empty item slot");
        }
    }

    public void InstanceItem(string itemName, int stacks)
    {
        if(itemIndex.ContainsKey(itemName))
        {
            stacksText[itemIndex[itemName]].SetActive(true);
            stacksText[itemIndex[itemName]].GetComponent<TMP_Text>().text = stacks.ToString();
            return;
        }
        else
        {
            Debug.Log("Item " + itemName + " not found in inventory");
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
