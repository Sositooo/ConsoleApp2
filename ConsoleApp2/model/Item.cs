using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    public class Item
    {
        public string Name { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }

        public Item(string name, int attack = 0, int defense = 0)
        {
            Name = name;
            Attack = attack;
            Defense = defense;
        }
    }

    public class Weapon : Item
    {
        public Weapon(string name, int attack) : base(name, attack, 0) { }
    }

    public class Armor : Item
    {
        public Armor(string name, int defense) : base(name, 0, defense) { }
    }
}