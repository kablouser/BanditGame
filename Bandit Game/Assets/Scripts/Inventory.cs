using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField]
    [Header("Don't change this during play.")]
    private Weapon[] inventoryList;
    [SerializeField]
    private Transform[] equipablePositions;
    
    public Weapon GetItem(int index)
    {
        if(0 <= index && index < inventoryList.Length)
        {
            return inventoryList[index];
        }
        else
        {
            return null;
        }
    }

    [ContextMenu("Spawn Prefabs")]
    public void SpawnPrefabs()
    {
        for (int i = 0; i < inventoryList.Length; i++)
        {
            if (!inventoryList[i])
                continue;

            Weapon newItem = inventoryList[i];
            inventoryList[i] = null;
            AddIndex(newItem, i);
        }
    }

    public bool Add(Weapon item)
    {
        for(int i = 0; i < inventoryList.Length; i++)
        {
            if(inventoryList[i] == null)
            {
                AddIndex(item, i);
                return true;
            }
        }
        return false;
    }

    public void AddIndex(Weapon item, int index)
    {
        if (index >= inventoryList.Length)
        {
            return;
        }

        if (inventoryList[index])
        {
            Remove(item);
        }
        inventoryList[index] = item;

        if (index < equipablePositions.Length && equipablePositions[index])
        {
            item.gameObject.SetActive(true);
            item.EquipTo(equipablePositions[index]);
        }
        else
        {
            item.gameObject.SetActive(false);
        }        
    }

    public bool Remove(Weapon item)
    {
        for(int i = 0; i < inventoryList.Length; i++)
        {
            if(inventoryList[i] == item)
            {
                RemoveIndex(i);
                return true;
            }
        }
        return false;
    }

    public void RemoveIndex(int index)
    {
        if(index < inventoryList.Length && inventoryList[index])
        {
            inventoryList[index].gameObject.SetActive(true);
            inventoryList[index].EquipTo(null);
            inventoryList[index] = null;
        }
    }
}
