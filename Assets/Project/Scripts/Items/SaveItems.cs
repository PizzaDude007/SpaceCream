using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveItems : MonoBehaviour
{
    private PlayerBehaviour playerData = PlayerBehaviour.Instance;
    public static SaveItems Instance;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SaveItemsToShop()
    {
        int itemsCount = playerData.player.items.Count;
        for (int i = 0; i < itemsCount; i++)
        {
            playerData.player.shop_items.Add(playerData.player.items[i]);
        }
        playerData.player.items.Clear();
    }
}
