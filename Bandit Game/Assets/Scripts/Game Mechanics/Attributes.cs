namespace Attributes
{
    //This tag allows the struct to be editable in Unity editor.
    [System.Serializable]
    public struct RegulatedAttribute
    {
        public Attribute maximum;
        public Attribute current;
        public Attribute regenRate;

        public void Regen(float timePassed)
        {
            ClampedAdd(regenRate * timePassed);
        }

        public void ClampedAdd(Attribute add)
        {
            current += add;
            Clamp();
        }

        public void ClampedSubtract(Attribute subtract)
        {
            current -= subtract;
            Clamp();
        }

        public void ClampedAdd(float health, float mana, float stamina)
        {
            current.health += health;
            current.mana += mana;
            current.stamina += stamina;
            Clamp();
        }

        /// <summary>
        /// Input attributes are deducted if there is enough. Returns true if requirement is met. False otherwise.
        /// </summary>
        public bool Spend(float health, float mana, float stamina)
        {
            if(current.health > health && current.mana > mana && current.stamina > stamina)
            {
                current.health -= health;
                current.mana -= mana;
                current.stamina -= stamina;
                return true;
            }
            return false;
        }

        public bool SpendOverTime(Attribute activateCost, Attribute drainCost, bool currentlyActive, bool previouslyActive, float timePassed)
        {
            if (currentlyActive)
            {
                if (previouslyActive)
                {
                    return Spend(drainCost.health * timePassed, drainCost.mana * timePassed, drainCost.stamina * timePassed);
                }
                else
                {
                    return Spend(activateCost.health, activateCost.mana, activateCost.stamina);
                }
            }
            return false;
        }

        private void Clamp()
        {
            current.ClampMax(maximum);
            current.ClampMin(Attribute.zero);
        }
    }

    [System.Serializable]
    public struct Attribute
    {
        public float health, mana, stamina;
        public readonly static Attribute zero = new Attribute(0, 0, 0);

        public Attribute(float health, float mana, float stamina)
        {
            this.health = health;
            this.mana = mana;
            this.stamina = stamina;
        }

        public static Attribute operator *(Attribute attribute, float multiplier)
        {
            attribute.health *= multiplier;
            attribute.mana *= multiplier;
            attribute.stamina *= multiplier;
            return attribute;
        }

        public static Attribute operator +(Attribute a, Attribute b)
        {
            a.health += b.health;
            a.mana += b.mana;
            a.stamina += b.stamina;
            return a;
        }

        public static Attribute operator -(Attribute a, Attribute b)
        {
            a.health -= b.health;
            a.mana -= b.mana;
            a.stamina -= b.stamina;
            return a;
        }

        public void ClampMax(Attribute max)
        {
            if (health > max.health)
                health = max.health;
            if (mana > max.mana)
                mana = max.mana;
            if (stamina > max.stamina)
                stamina = max.stamina;
        }
        public void ClampMin(Attribute min)
        {
            if (health < min.health)
                health = min.health;
            if (mana < min.mana)
                mana = min.mana;
            if (stamina < min.stamina)
                stamina = min.stamina;
        }
    }
}
