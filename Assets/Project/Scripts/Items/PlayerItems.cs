using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItems : MonoBehaviour
{
    //public PlayerBehaviour player;
    public int attackDamage;
    public List<ItemList> items = new List<ItemList>();
    public UpdateItemCanvas itemCanvas;

    // Start is called before the first frame update
    void Start()
    {
        //HealingItem item = new HealingItem();
        //items.Add(new ItemList(item, item.GiveName(), 1));
        StartCoroutine(CallItemUpdate());
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.JoystickButton4))
        {
            CallItemOnJump();
        }
    }

    //public void AddItem(Item item)
    //{
    //    bool found = false;
    //    foreach (ItemList i in items)
    //    {
    //        if (i.name == item.GiveName())
    //        {
    //            i.stacks++;
    //            found = true;
    //        }
    //    }
    //    if (!found)
    //    {
    //        items.Add(new ItemList(item, item.GiveName(), 1));
    //    }
    //}

    public void TakeDamage(int damage)
    {
        PlayerBehaviour.Instance.TakeDamage(damage);
    }

    public void Heal(int heal)
    {
        Debug.Log("Player healed for " + heal + " health");
        PlayerBehaviour.Instance.Heal(heal);
    }

    IEnumerator CallItemUpdate()
    {
        foreach (ItemList i in items)
        {
            i.item.Update(this, i.stacks);
        }
        yield return new WaitForSeconds(1);
        StartCoroutine(CallItemUpdate());
    }

    public void CallItemOnHit(EnemyBehaviour enemy)
    {
        foreach (ItemList i in items)
        {
            i.item.OnHit(this, enemy, i.stacks);
        }
    }

    public void CallItemOnJump()
    {
        foreach (ItemList i in items)
        {
            i.item.OnJump(this, i.stacks);
        }
    }

    public void UpdateCanvas(string itemName)
    {
        itemCanvas.InstanceItem(itemName);
    }

    public void CallItemOnPickup(Item item)
    {
        item.OnPickup(this);
    }
}
