using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item;
    public Items itemDrop;

    // Start is called before the first frame update
    void Start()
    {
        item = AssignItem(itemDrop);
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
        foreach(ItemList i in player.items)
        {
            if (i.name == item.GiveName())
            {
                i.stacks++;
                Debug.Log("Picked up " + item.GiveName() + " and added to inventory");
                return;
            }
        }
        player.items.Add(new ItemList(item, item.GiveName(), 1));
        player.UpdateCanvas(item.GiveName());
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
