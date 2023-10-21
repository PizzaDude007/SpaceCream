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
        /*foreach(GameObject item in stacksText)
        {
            item.SetActive(false);
        }*/
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
            Debug.Log("Keys: "+itemIndex.Keys.ToString()+", Values: "+ itemIndex.Values.ToString());
        }
    }

    public void InstanceItemWithStacks(string itemName, int stacks)
    {
        if (itemIndex.ContainsKey(itemName))
        {
            GameObject itemResource = (GameObject)Resources.Load("ItemIcon3D/" + itemName, typeof(GameObject));
            GameObject item = itemParent[itemIndex[itemName]];
            GameObject newItem = Instantiate(itemResource, item.transform.position, Quaternion.Euler(Vector3.zero), item.transform);
            if (stacks == 1)
            {
                stacksText[itemIndex[itemName]].SetActive(false);
                return;
            }
            stacksText[itemIndex[itemName]].SetActive(true);
            stacksText[itemIndex[itemName]].GetComponent<TMP_Text>().text = stacks.ToString();
            return;
        }
        else
        {
            Debug.Log("Item " + itemName + " not found in inventory");
        }
    }

    public void InstanceItem(ItemList itemList)
    {
        if(itemList.stacks == 1)
            InstanceItem(itemList.name);
        else
            InstanceItem(itemList.name, itemList.stacks);
    }

    public void InstanceListOfItems(List<ItemList> items)
    {
        foreach(ItemList i in items)
        {
            //InstanceItem(i);
            InstanceItemWithStacks(i.name, i.stacks);
        }
    }

    public void RemoveItem(string itemName)
    {
        if (itemIndex.ContainsKey(itemName))
        {
            TMP_Text text = stacksText[itemIndex[itemName]].GetComponent<TMP_Text>();
            if(text.text.Equals("1") || text.text.Equals("0"))
            {
                Destroy(itemParent[itemIndex[itemName]].transform.GetChild(0).gameObject);
                itemIndex.Remove(itemName);
                stacksText[itemIndex[itemName]].SetActive(false);
                Debug.Log("Removed " + itemName + " from inventory");
            }
            else
            {
                int stacks = int.Parse(text.text);
                stacks--;
                stacksText[itemIndex[itemName]].GetComponent<TMP_Text>().text = stacks.ToString();
            }
        }
        else
        {
            Debug.Log("Item " + itemName + " not found in inventory");
        }
    }

    public void SetItemDictionary(Dictionary<string, int> itemIndex)
    {
        this.itemIndex = itemIndex;
    }
}
