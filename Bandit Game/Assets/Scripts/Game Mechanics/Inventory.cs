using UnityEngine;

public class Inventory : MonoBehaviour
{
    public const int LeftHand = 0, RightHand = 1, SwordSlot = 1, ShieldSlot = 0, BookSlot = 0;

    public Hitbox inventoryHitbox;

    [SerializeField]
    [Tooltip("Don't change this during play.")]
    private Weapon[] inventoryList;
    [SerializeField]
    private Transform[] equipablePositions;

    public enum WeaponCombination { None, SwordShield, Book };
    [SerializeField]
    private WeaponCombination currentCombination;
    public WeaponCombination GetCurrentCombination
    {
        get
        {
            return currentCombination;
        }
    }

    private void Awake()
    {
        LinkInventory();
    }

    public Weapon GetItem(int index)
    {
        if (0 <= index && index < inventoryList.Length)
        {
            return inventoryList[index];
        }
        else
        {
            return null;
        }
    }

    public void LinkInventory()
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
        for (int i = 0; i < inventoryList.Length; i++)
        {
            if (inventoryList[i] == null)
            {
                AddIndex(item, i);
                return true;
            }
        }
        return false;
    }

    public void AddIndex(Weapon item, int index)
    {
        if (index < 0 || inventoryList.Length <= index)
        {
            return;
        }

        if (inventoryList[index])
        {
            RemoveIndex(index);
        }
        inventoryList[index] = item;
        UpdateCombination();

        if (index < equipablePositions.Length && equipablePositions[index])
        {
            item.gameObject.SetActive(true);
            item.EquipTo(equipablePositions[index], inventoryHitbox);
        }
        else
        {
            item.gameObject.SetActive(false);
        }
    }

    public bool Remove(Weapon item)
    {
        for (int i = 0; i < inventoryList.Length; i++)
        {
            if (inventoryList[i] == item)
            {
                RemoveIndex(i);
                return true;
            }
        }
        return false;
    }

    public void RemoveIndex(int index)
    {
        if (0 <= index && index < inventoryList.Length && inventoryList[index])
        {
            inventoryList[index].gameObject.SetActive(true);
            inventoryList[index].EquipTo(null, inventoryHitbox);
            inventoryList[index] = null;
            UpdateCombination();
        }
    }

    public bool GetSwordShield(out Sword sword, out Shield shield)
    {
        if (currentCombination == WeaponCombination.SwordShield)
        {
            sword = GetItem(SwordSlot) as Sword;
            shield = GetItem(ShieldSlot) as Shield;
            return true;
        }
        else
        {
            sword = null;
            shield = null;
            return false;
        }
    }

    public bool GetBook(out MagicBook book)
    {
        if (currentCombination == WeaponCombination.Book)
        {
            book = (MagicBook)GetItem(BookSlot);
            return true;
        }
        else
        {
            book = null;
            return false;
        }
    }

    private void UpdateCombination()
    {
        Sword sword = GetItem(SwordSlot) as Sword;
        Shield shield = GetItem(ShieldSlot) as Shield;

        if (sword || shield)
        {
            currentCombination = WeaponCombination.SwordShield;
            return;
        }

        MagicBook book = GetItem(BookSlot) as MagicBook;

        if (book)
        {
            currentCombination = WeaponCombination.Book;
            return;
        }
    }
}
