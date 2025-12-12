using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp2;

namespace ConsoleApp2
{
    public class WeaponFactory
    {
        private RandomService randomService;

        public WeaponFactory(RandomService randomService)
        {
            this.randomService = randomService;
        }

        public Weapon CreateRandomWeapon()
        {
            var weapons = new[]
            {
            new Weapon("Кинжал", 8),
            new Weapon("Меч", 12),
            new Weapon("Топор", 15),
            new Weapon("Посох", 10),
            new Weapon("Двуручный меч", 18)
        };
            return randomService.Choose(weapons);
        }

        public Weapon CreateStarterWeapon()
        {
            return new Weapon("Кулаки", 5);
        }
    }
}