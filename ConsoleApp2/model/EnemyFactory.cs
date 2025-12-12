using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp2.model
{
    public class EnemyFactory
    {
        private RandomService randomService;

        public EnemyFactory(RandomService randomService)
        {
            this.randomService = randomService;
        }

        public Enemy CreateNormalEnemy()
        {
            var normalEnemies = new List<Enemy>
        {
            new Enemy("Гоблин", 30, 8, 3, critChance: 0.2),
            new Enemy("Скелет", 25, 10, 2, ignoreDefense: true),
            new Enemy("Маг", 20, 12, 1, freezeChance: 0.25),
            new Enemy("Слизень", 35, 6, 1, damageReduction: 2)
        };
            return randomService.Choose(normalEnemies);
        }

        public Enemy CreateBoss()
        {
            var bosses = new List<Enemy>
        {
            new Enemy("ВВГ (Гоблин)", 60, 12, 4, critChance: 0.3),
            new Enemy("Ковальский (Скелет)", 63, 13, 3, ignoreDefense: true),
            new Enemy("Архимаг C++", 36, 19, 1, freezeChance: 0.35),
            new Enemy("Пестов С--", 33, 18, 1, ignoreDefense: true, freezeChance: 0.4),
            new Enemy("Слизнекороль", 50, 10, 2, damageReduction: 3)
        };
            return randomService.Choose(bosses);
        }

    }
}
