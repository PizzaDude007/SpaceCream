using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerItems : MonoBehaviour
{
    //public PlayerBehaviour player;
    public int attackDamage = 10;
    public Color attackColor = Color.white;
    public List<ItemList> items = new List<ItemList>();
    public UpdateItemCanvas itemCanvas;

    public GameObject GunTransform;
    public GunStats gunStats;

    // Start is called before the first frame update
    void Start()
    {
        if(PlayerBehaviour.Instance.player.items.Count > 0)
        {
            UpdateItemsFromPlayer();
        }
        else
        {
            items = new List<ItemList>();
        }

        //HealingItem item = new HealingItem();
        //items.Add(new ItemList(item, item.GiveName(), 1));
        gunStats = GunTransform.GetComponentInChildren<GunStats>();
        attackDamage = gunStats.baseDamage;
        attackColor = Color.white;
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
        //Base Damage
        enemy.TakeDamage(attackDamage);

        //Item Damage
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

    public void UpdateCanvas(string itemName, int stacks)
    {
        itemCanvas.InstanceItem(itemName, stacks);
    }

    public void UpdateCanvas(List<ItemList> items)
    {
        itemCanvas.InstanceListOfItems(items);
    }

    public void CallItemOnPickup(Item item)
    {
        item.OnPickup(this);
        PlayerBehaviour.Instance.player.items.Add(item.GiveName());
    }

    public Vector3 GetGroundNormal()
    {
        PlayerHitbox playerHitbox = gameObject.GetComponent<PlayerHitbox>();
        return playerHitbox.GetGroundNormal();
    }

    public void UpdateItemsFromPlayer()
    {
        List<ItemList> items = new List<ItemList>();
        Dictionary<string, int> itemStacks = new Dictionary<string, int>();
        Dictionary<string, int> itemIndex = new Dictionary<string, int>();
        int index = 0;
        foreach (string i in PlayerBehaviour.Instance.player.items)
        {
            if(itemStacks.ContainsKey(i))
            {
                itemStacks[i]++;
                //items.Find(x => x.name == i).stacks++;
                items[itemIndex[i]].stacks++;
            }
            else
            {
                itemIndex.Add(i, index);
                index++;
                itemStacks.Add(i, 1);
                items.Add(new ItemList(NameToItem(i), i, 1));
            }
        }
        itemCanvas.SetItemDictionary(itemIndex);
        UpdateCanvas(items);
    }

    public Item NameToItem(string name)
    {
        switch (name)
        {
            case "HealingItem":
                return new HealingItem();
            case "FireDamageItem":
                return new FireDamageItem();
            case "HealingAreaItem":
                return new HealingAreaItem();
            default:
                return new HealingItem();
        }
    }
}
