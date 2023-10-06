using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Item 
{
    public abstract string GiveName();

    public virtual void Update(PlayerItems player, int stacks)
    {

    }

    public virtual void OnHit(PlayerItems player, EnemyBehaviour enemy, int stacks)
    {

    }

    public virtual void OnJump(PlayerItems player, int stacks)
    {

    }

    public virtual void OnKill(PlayerItems player, EnemyBehaviour enemy, int stacks)
    {

    }
}

public class HealingItem : Item
{
    public override string GiveName()
    {
        return "Health Item";
    }

    public override void Update(PlayerItems player, int stacks)
    {
        player.Heal(3 + (2*stacks));  //base = 5, 2 stacks = 9, 3 stacks = 11
    }
}

public class FireDamageItem: Item
{
    public override string GiveName()
    {
        return "Fire Damage Item";
    }

    public override void OnHit(PlayerItems player, EnemyBehaviour enemy, int stacks)
    {
        enemy.TakeDamage(10 + stacks); //10 + stack
    }
}

public class HealingAreaItem : Item
{
    float intenalCooldown;
    GameObject effect;
    public override string GiveName()
    {
        return "Healing Area Item";
    }
    public override void Update(PlayerItems player, int stacks)
    {
        intenalCooldown -= 1;
    }
    public override void OnJump(PlayerItems player, int stacks)
    {
        if (intenalCooldown <= 0)
        {
            if(effect == null) 
                effect = (GameObject)Resources.Load("ItemEffects/HealingArea", typeof(GameObject));

            GameObject healingArea = GameObject.Instantiate(effect, player.transform.position, Quaternion.Euler(Vector3.zero));
            
            intenalCooldown = 10;
        }
    }
}
