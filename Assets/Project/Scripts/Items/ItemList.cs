using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ItemList
{
    public Item item;
    public string name;
    public int stacks;

    public ItemList(Item item, string name, int stacks)
    {
        this.item = item;
        this.name = name;
        this.stacks = stacks;
    }
}
