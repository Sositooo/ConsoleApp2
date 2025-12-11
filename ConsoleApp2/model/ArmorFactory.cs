using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2.model
{
    public class ArmorFactory
    {
        private RandomService randomService;

        public ArmorFactory(RandomService randomService)
        {
            this.randomService = randomService;
        }

        public Armor CreateRandomArmor()
        {
            var armors = new[]
            {
            new Armor("Кожаная броня", 5),
            new Armor("Кольчуга", 8),
            new Armor("Латы", 12),
            new Armor("Мантия мага", 6),
            new Armor("Доспех воина", 10)
        };
            return randomService.Choose(armors);
        }

        public Armor CreateStarterArmor()
        {
            return new Armor("Простая одежда", 2);
        }
    }
}
