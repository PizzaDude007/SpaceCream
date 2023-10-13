using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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

    public virtual void OnPickup(PlayerItems player)
    {

    }

    public virtual void SpawnItem(Vector3 position)
    {
        
    }
}

public class HealingItem : Item
{
    GameObject effect;
    bool effectInstantiated = false;

    public override string GiveName()
    {
        return "Health Item";
    }

    public override void Update(PlayerItems player, int stacks)
    {
        player.Heal(3 + (2*stacks));  //base = 5, 2 stacks = 9, 3 stacks = 11
    }

    public override void OnPickup(PlayerItems player)
    {
        if(effectInstantiated)
            return;

        if (effect == null)
            effect = (GameObject)Resources.Load("ItemEffects/HealingEffect", typeof(GameObject));

        GameObject healingEffect = GameObject.Instantiate(effect, player.transform.position, Quaternion.Euler(Vector3.zero), player.transform);
        healingEffect.transform.position = new Vector3(healingEffect.transform.position.x, healingEffect.transform.position.y + 1, healingEffect.transform.position.z);

        if (healingEffect != null)
        {
            effectInstantiated = true;
            Debug.Log("Healing effect instantiated");
        }
        else
            Debug.Log("Healing effect not instantiated");
    }

    public override void SpawnItem(Vector3 position)
    {
        GameObject gameObject = (GameObject)Resources.Load("Items/HealingItem", typeof(GameObject));
        GameObject.Instantiate(gameObject, position, Quaternion.identity);
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
        player.attackColor = Color.Lerp(Color.red, Color.yellow, 0.5f);
    }

    public override void SpawnItem(Vector3 position)
    {
        GameObject gameObject = (GameObject)Resources.Load("Items/FireDamageItem", typeof(GameObject));
        GameObject.Instantiate(gameObject, position, Quaternion.identity);
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
            healingArea.transform.position = new Vector3(healingArea.transform.position.x, healingArea.transform.position.y - 0.7f, healingArea.transform.position.z);

            //Align normal to ground
            healingArea.transform.rotation = Quaternion.FromToRotation(Vector3.up, player.GetGroundNormal());

            GameObject.Destroy(healingArea, 15f);
            
            intenalCooldown = 10;
        }
    }

    public override void SpawnItem(Vector3 position)
    {
        GameObject gameObject = (GameObject)Resources.Load("Items/HealingAreaItem", typeof(GameObject));
        GameObject.Instantiate(gameObject, position, Quaternion.identity);
    }
}
