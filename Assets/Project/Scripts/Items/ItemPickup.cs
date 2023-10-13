using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Analytics;

public class ItemPickup : MonoBehaviour
{
    public Item item;
    public Items itemDrop;
    private string levelName;

    // Start is called before the first frame update
    void Start()
    {
        item = AssignItem(itemDrop);
        levelName = SceneManager.GetActiveScene().name;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerItems player = other.GetComponent<PlayerItems>();
        if (player != null)
        {
            AddItem(player);
            Destroy(gameObject);
        }
    }

    public void AddItem(PlayerItems player)
    {
        AnalyticsService.Instance.CustomData("itemPicked", new Dictionary<string, object>
            {
                { "levelName", levelName }, { "itemType", item.GiveName()}
            });

        foreach (ItemList i in player.items)
        {
            if (i.name == item.GiveName())
            {
                i.stacks++;
                Debug.Log("Picked up " + item.GiveName() + " again, stack = " + i.stacks);
                player.UpdateCanvas(item.GiveName(), i.stacks);
                player.CallItemOnPickup(item);
                return;
            }
        }
        player.items.Add(new ItemList(item, item.GiveName(), 1));
        player.UpdateCanvas(item.GiveName());
        player.CallItemOnPickup(item);
        Debug.Log("Picked up " + item.GiveName() + " and added to inventory");
    }

    public Item AssignItem(Items itemToAssign)
    {
        switch (itemToAssign)
        {
            case Items.HealingItem:
                return new HealingItem();
            case Items.FireDamageItem:
                return new FireDamageItem();
            case Items.HealingAreaItem:
                return new HealingAreaItem();
            default:
                return new HealingItem();
        }
    }
}

public enum Items
{
    HealingItem,
    FireDamageItem,
    HealingAreaItem
}
