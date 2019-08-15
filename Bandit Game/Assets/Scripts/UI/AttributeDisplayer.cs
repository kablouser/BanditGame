using UnityEngine;
using Attributes;
using UnityEngine.UI;

public class AttributeDisplayer : MonoBehaviour
{
    public Entity entity;
    public Slider healthBar;
    public Slider manaBar;
    public Slider staminaBar;

    private const float baseDivisor = 100f;

    void Update()
    {
        if (entity)
        {
            RegulatedAttribute attribute = entity.GetAttribute;
            if (attribute.maximum.health == 0)
                healthBar.value = attribute.current.health / baseDivisor;
            else
                healthBar.value = attribute.current.health / attribute.maximum.health;

            if (attribute.maximum.mana == 0)
                manaBar.value = attribute.current.mana / baseDivisor;
            else
                manaBar.value = attribute.current.mana / attribute.maximum.mana;

            if (attribute.maximum.health == 0)
                staminaBar.value = attribute.current.stamina / baseDivisor;
            else
                staminaBar.value = attribute.current.stamina / attribute.maximum.stamina;
        }
    }
}
