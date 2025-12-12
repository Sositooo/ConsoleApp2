using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ConsoleApp2.model;

namespace ConsoleApp2
{
    public class Game
    {
        private Player player;
        private RandomService randomService;
        private EnemyFactory enemyFactory;
        private WeaponFactory weaponFactory;
        private ArmorFactory armorFactory;
        private int turnCount;

        public Game()
        {
            randomService = new RandomService();
            enemyFactory = new EnemyFactory(randomService);
            weaponFactory = new WeaponFactory(randomService);
            armorFactory = new ArmorFactory(randomService);
            player = new Player(weaponFactory, armorFactory);
            turnCount = 0;
        }

        public void Start()
        {
            Console.WriteLine("ТЕКСТОВЫЙ РОГАЛИК");
            Console.WriteLine("Добро пожаловать в игру!");
            Console.WriteLine("Каждый ход вас ждет либо сундук, либо враг.");
            Console.WriteLine("Каждые 10 ходов - встреча с боссом!\n");

            while (player.IsAlive())
            {
                turnCount++;
                Console.WriteLine($"\nХод {turnCount}");
                DisplayPlayerStatus();

                if (player.IsFrozen)
                {
                    Console.WriteLine("Вы заморожены и пропускаете ход!");
                    player.IsFrozen = false;
                    continue;
                }

                if (turnCount % 10 == 0)
                {
                    Console.WriteLine("\n!!! ПОЯВИЛСЯ БОСС !!!");
                    Enemy boss = enemyFactory.CreateBoss();
                    Fight(boss);
                }
                else
                {
                    if (randomService.Next(2) == 0)
                        FightEnemy();
                    else
                        OpenChest();
                }

                CheckPlayerStatus();
            }
        }

        private void DisplayPlayerStatus()
        {
            Console.WriteLine($"Здоровье: {player.HP}/{player.MaxHP}");
            Console.WriteLine($"Оружие: {player.Weapon.Name} (Атака: {player.Weapon.Attack})");
            Console.WriteLine($"Броня: {player.Armor.Name} (Защита: {player.Armor.Defense})");
        }

        private void CheckPlayerStatus()
        {
            if (!player.IsAlive())
            {
                Console.WriteLine("\nВЫ ПОГИБЛИ");
                Console.WriteLine($"Вы продержались {turnCount} ходов");
            }
        }

        private void FightEnemy()
        {
            Enemy enemy = enemyFactory.CreateNormalEnemy();
            Fight(enemy);
        }

        private void Fight(Enemy enemy)
        {
            Console.WriteLine($"\nПеред вами: {enemy.Name}");
            Console.WriteLine($"HP: {enemy.HP}, Атака: {enemy.Attack}, Защита: {enemy.Defense}");

            while (enemy.IsAlive() && player.IsAlive())
            {
                bool defended = PlayerTurn(enemy);
                if (!enemy.IsAlive()) break;

                EnemyTurn(enemy, defended);
            }

            if (!enemy.IsAlive())
                Console.WriteLine($"\nВы победили {enemy.Name}!");
        }

        private bool PlayerTurn(Enemy enemy)
        {
            Console.WriteLine("\nВаш ход:");
            Console.WriteLine("1 - Атака");
            Console.WriteLine("2 - Защита");

            int choice = GetChoice(1, 2);
            bool defended = false;

            if (choice == 2)
            {
                defended = true;
                Console.WriteLine("Вы готовитесь к защите...");
            }
            else
            {
                AttackEnemy(enemy);
            }
            return defended;
        }

        private void AttackEnemy(Enemy enemy)
        {
            int playerDamage = player.GetAttack();
            int finalDamage = enemy.CalculateDamage(playerDamage);
            enemy.HP -= finalDamage;
            Console.WriteLine($"Вы нанесли {finalDamage} урона!");
            Console.WriteLine($"У {enemy.Name} осталось {Math.Max(0, enemy.HP)} HP");
        }

        private void EnemyTurn(Enemy enemy, bool defended)
        {
            Console.WriteLine($"\nХод {enemy.Name}:");

            if (defended && randomService.CheckProbability(0.4))
            {
                Console.WriteLine("Вы успешно уклонились от атаки!");
                return;
            }

            int enemyDamage = CalculateEnemyDamage(enemy);
            bool isFrozen = randomService.CheckProbability(enemy.FreezeChance);

            if (isFrozen)
            {
                player.IsFrozen = true;
                Console.WriteLine("Вас заморозили! Вы пропустите следующий ход.");
            }

            int finalDamage = CalculateFinalDamage(enemy, enemyDamage, defended);
            player.HP -= finalDamage;
            Console.WriteLine($"Вам нанесли {finalDamage} урона!");
            Console.WriteLine($"У вас осталось {Math.Max(0, player.HP)} HP");
        }

        private int CalculateEnemyDamage(Enemy enemy)
        {
            int damage = enemy.Attack;
            if (randomService.CheckProbability(enemy.CritChance))
            {
                damage = (int)(damage * 1.5);
                Console.WriteLine("Критический удар!");
            }
            return damage;
        }

        private int CalculateFinalDamage(Enemy enemy, int enemyDamage, bool defended)
        {
            if (enemy.IgnoreDefense)
            {
                Console.WriteLine("Враг игнорирует вашу защиту!");
                return enemyDamage;
            }

            if (defended)
            {
                double blockPercent = randomService.GetBlockCoefficient();
                int blockedDamage = (int)(player.GetDefense() * blockPercent);
                int finalDamage = Math.Max(0, enemyDamage - blockedDamage);
                Console.WriteLine($"Вы заблокировали {blockedDamage} урона");
                return finalDamage;
            }

            return Math.Max(0, enemyDamage - player.GetDefense());
        }

        private void OpenChest()
        {
            Console.WriteLine("\nВы нашли сундук!");
            int chestType = randomService.Next(3);

            switch (chestType)
            {
                case 0:
                    HandleHealingChest();
                    break;
                case 1:
                    HandleWeaponChest();
                    break;
                case 2:
                    HandleArmorChest();
                    break;
            }
        }

        private void HandleHealingChest()
        {
            Console.WriteLine("В сундуке лечебное зелье!");
            player.Heal();
            Console.WriteLine("Ваше здоровье полностью восстановлено!");
        }

        private void HandleWeaponChest()
        {
            Weapon newWeapon = weaponFactory.CreateRandomWeapon();
            Console.WriteLine($"В сундуке оружие: {newWeapon.Name} (Атака: {newWeapon.Attack})");
            Console.WriteLine($"Ваше текущее оружие: {player.Weapon.Name} (Атака: {player.Weapon.Attack})");
            Console.WriteLine("Взять новое оружие? (1 - да, 2 - нет)");

            if (GetChoice(1, 2) == 1)
            {
                player.Weapon = newWeapon;
                Console.WriteLine("Вы экипировали новое оружие!");
            }
        }

        private void HandleArmorChest()
        {
            Armor newArmor = armorFactory.CreateRandomArmor();
            Console.WriteLine($"В сундуке броня: {newArmor.Name} (Защита: {newArmor.Defense})");
            Console.WriteLine($"Ваша текущая броня: {player.Armor.Name} (Защита: {player.Armor.Defense})");
            Console.WriteLine("Взять новую броню? (1 - да, 2 - нет)");

            if (GetChoice(1, 2) == 1)
            {
                player.Armor = newArmor;
                Console.WriteLine("Вы экипировали новую броню!");
            }
        }

        private int GetChoice(int min, int max)
        {
            while (true)
            {
                Console.Write($"Выберите действие ({min}-{max}): ");
                if (int.TryParse(Console.ReadLine(), out int choice) && choice >= min && choice <= max)
                    return choice;
                Console.WriteLine("Неверный ввод!");
            }
        }
    }
}