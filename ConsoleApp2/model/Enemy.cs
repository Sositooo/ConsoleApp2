using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2.model
{
    public class Enemy
    {
        public string Name { get; set; }
        public int MaxHP { get; set; }
        public int HP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public double CritChance { get; set; }
        public double FreezeChance { get; set; }
        public bool IgnoreDefense { get; set; }
        public int DamageReduction { get; set; }

        public Enemy(string name, int hp, int attack, int defense,
                    double critChance = 0, double freezeChance = 0,
                    bool ignoreDefense = false, int damageReduction = 0)
        {
            Name = name;
            MaxHP = hp;
            HP = hp;
            Attack = attack;
            Defense = defense;
            CritChance = critChance;
            FreezeChance = freezeChance;
            IgnoreDefense = ignoreDefense;
            DamageReduction = damageReduction;
        }

        public bool IsAlive() => HP > 0;

        public virtual int CalculateDamage(int incomingDamage)
        {
            return Math.Max(0, incomingDamage - DamageReduction);
        }S
    }
}
