using ConsoleApp2;

using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoService
{
    public class Detale
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class Sklad
    {
        public int ID { get; set; }
        public decimal Balance { get; set; }
    }

    public class SkladDetale
    {
        public int SkladID { get; set; }
        public int DetaleID { get; set; }
        public int Count { get; set; }
    }

    public class Customer
    {
        public int ID { get; set; }
        public string CarModel { get; set; }
        public int BrokenDetaleID { get; set; }
        public decimal RepairCost { get; set; }
        public bool IsServed { get; set; }
    }

    public class PurchaseOrder
    {
        public int DetaleID { get; set; }
        public int Quantity { get; set; }
        public int RemainingCars { get; set; }
    }

    public class AutoServiceGame
    {
        private Sklad Sklad;
        private List<Detale> availableDetales;
        private List<SkladDetale> SkladDetales;
        private List<PurchaseOrder> pendingOrders;
        private Random random;

        public AutoServiceGame()
        {
            this.random = new Random();
            this.pendingOrders = new List<PurchaseOrder>();
            InitializeGame();
        }

        private void InitializeGame()
        {
            LoadSkladData();
            LoadAvailableDetales();
            Console.WriteLine("Добро пожаловать в автосервис!");
            Console.WriteLine($"Начальный баланс: {Sklad.Balance}");
        }

        private void LoadSkladData()
        {
            var dbSklad = Core.Context.Sklad.FirstOrDefault();
            if (dbSklad != null)
            {
                Sklad = new Sklad
                {
                    ID = dbSklad.ID,
                    Balance = (int)dbSklad.Balance
                };
            }
            SkladDetales = new List<SkladDetale>();
            var dbSkladDetales = Core.Context.DetaleSklad.ToList();
            foreach (var dbDetale in dbSkladDetales)
            {
                SkladDetales.Add(new SkladDetale
                {
                    SkladID = dbDetale.SkladID,
                    DetaleID = dbDetale.DetaleID,
                    Count = (int)dbDetale.Count
                });
            }
        }

        private void LoadAvailableDetales()
        {
            availableDetales = new List<Detale>();
            var dbDetales = Core.Context.Detale.ToList();
            foreach (var dbDetale in dbDetales)
            {
                availableDetales.Add(new Detale
                {
                    ID = dbDetale.ID,
                    Name = dbDetale.Name,
                    Price = (decimal)dbDetale.Price
                });
            }
        }
        private void SaveGameState()
        {
            try
            {
                var dbSklad = Core.Context.Sklad.FirstOrDefault(s => s.ID == Sklad.ID);
                if (dbSklad != null)
                {
                    dbSklad.Balance = Sklad.Balance;
                }
                foreach (var wp in SkladDetales)
                {
                    var dbSkladDetale = Core.Context.DetaleSklad
                        .FirstOrDefault(ds => ds.SkladID == wp.SkladID && ds.DetaleID == wp.DetaleID);

                    if (dbSkladDetale != null)
                    {
                        dbSkladDetale.Count = wp.Count;
                    }
                    else
                    {
                        var newSkladDetale = new DetaleSklad
                        {
                            SkladID = wp.SkladID,
                            DetaleID = wp.DetaleID,
                            Count = wp.Count
                        };
                        Core.Context.DetaleSklad.Add(newSkladDetale);
                    }
                }
                Core.Context.SaveChanges();
                Console.WriteLine("Данные сохранены в базу данных.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения в базу данных: {ex.Message}");
            }
        }

        public void StartGame()
        {
            int carCounter = 0;

            while (true)
            {
                carCounter++;
                Console.WriteLine($"\nМашина #{carCounter}");
                ProcessPendingOrders();
                var customer = GenerateCustomer();
                ShowCustomerInfo(customer);
                ProcessCustomerService(customer);
                SaveGameState();

                if (Sklad.Balance <= 0)
                {
                    Console.WriteLine("\nВы банкрот! Игра окончена.");
                    break;
                }
                ShowMainMenu();
            }
        }

        private void ShowMainMenu()
        {
            while (true)
            {
                Console.WriteLine("\nГлавное меню");
                Console.WriteLine("1 - Следующий клиент");
                Console.WriteLine("2 - Заказать детали");
                Console.WriteLine("3 - Показать статус склада");
                Console.Write("Выберите действие: ");

                var choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        return;
                    case "2":
                        ShowPurchaseMenu();
                        SaveGameState();
                        break;
                    case "3":
                        ShowSkladStatus();
                        break;
                    default:
                        Console.WriteLine("Неверный выбор! Попробуйте снова.");
                        break;
                }
            }
        }

        private Customer GenerateCustomer()
        {
            var brokenDetale = availableDetales[random.Next(availableDetales.Count)];
            var workCost = brokenDetale.Price * 0.3m;
            var repairCost = brokenDetale.Price + workCost;

            return new Customer
            {
                ID = random.Next(1000, 9999),
                CarModel = GenerateCarModel(),
                BrokenDetaleID = brokenDetale.ID,
                RepairCost = repairCost,
                IsServed = false
            };
        }

        private string GenerateCarModel()
        {
            var brands = new[] { "Toyota", "Honda", "Ford", "BMW", "Mercedes", "Audi", "Volkswagen", "Hyundai" };
            var models = new[] { "Camry", "Civic", "Focus", "X5", "C-Class", "A4", "Golf", "Elantra" };

            return $"{brands[random.Next(brands.Length)]} {models[random.Next(models.Length)]}";
        }

        private void ShowCustomerInfo(Customer customer)
        {
            var brokenDetale = availableDetales.First(p => p.ID == customer.BrokenDetaleID);

            Console.WriteLine($"Клиент приехал на {customer.CarModel}");
            Console.WriteLine($"Сломана деталь: {brokenDetale.Name}");
            Console.WriteLine($"Стоимость ремонта: {customer.RepairCost}");
            Console.WriteLine($"На складе есть: {GetDetaleCount(customer.BrokenDetaleID)} шт.");
        }

        private void ProcessCustomerService(Customer customer)
        {
            Console.WriteLine("\nВаши действия:");
            Console.WriteLine("1 - Принять заказ и починить");
            Console.WriteLine("2 - Отказать в обслуживании");
            Console.Write("Выберите действие: ");

            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    AcceptOrder(customer);
                    break;
                case "2":
                    RefuseOrder(customer);
                    break;
                default:
                    Console.WriteLine("Неверный выбор! Отказ в обслуживании.");
                    RefuseOrder(customer);
                    break;
            }
        }

        private void AcceptOrder(Customer customer)
        {
            var brokenDetaleID = customer.BrokenDetaleID;
            var DetaleCount = GetDetaleCount(brokenDetaleID);

            if (DetaleCount > 0)
            {
                UseDetale(brokenDetaleID);
                Sklad.Balance += customer.RepairCost;
                customer.IsServed = true;
                Console.WriteLine($"Ремонт выполнен успешно! Получено {customer.RepairCost}");
            }
            else
            {
                ReplaceWithRandomDetale(customer);
            }
        }

        private void RefuseOrder(Customer customer)
        {
            var penalty = customer.RepairCost * 0.2m; // 20% штраф
            Sklad.Balance -= penalty;
            Console.WriteLine($"Отказ в обслуживании. Штраф: {penalty}");
        }

        private void ReplaceWithRandomDetale(Customer customer)
        {
            var availableDetaleIDs = SkladDetales.Where(wp => wp.Count > 0).Select(wp => wp.DetaleID).ToList();

            if (availableDetaleIDs.Count > 0)
            {
                var randomDetaleID = availableDetaleIDs[random.Next(availableDetaleIDs.Count)];
                var randomDetale = availableDetales.First(p => p.ID == randomDetaleID);
                var compensation = customer.RepairCost * 1.5m; // 150% компенсация

                UseDetale(randomDetaleID);
                Sklad.Balance -= compensation;

                Console.WriteLine($"Нужной детали нет! Установлена {randomDetale.Name}");
                Console.WriteLine($"Клиент недоволен! Выплачена компенсация: {compensation}");
            }
            else
            {
                Console.WriteLine("На складе нет деталей! Ремонт невозможен.");
            }
        }

        private void UseDetale(int DetaleID)
        {
            var SkladDetale = SkladDetales.FirstOrDefault(wp => wp.DetaleID == DetaleID);
            if (SkladDetale != null && SkladDetale.Count > 0)
            {
                SkladDetale.Count--;
            }
        }

        private int GetDetaleCount(int DetaleID)
        {
            var SkladDetale = SkladDetales.FirstOrDefault(wp => wp.DetaleID == DetaleID);
            return SkladDetale?.Count ?? 0;
        }

        private void ShowPurchaseMenu()
        {
            Console.WriteLine("\nМеню закупки деталей");
            Console.WriteLine("Доступные детали:");

            for (int i = 0; i < availableDetales.Count; i++)
            {
                var Detale = availableDetales[i];
                var count = GetDetaleCount(Detale.ID);
                Console.WriteLine($"{i + 1}. {Detale.Name} - {Detale.Price} (на складе: {count})");
            }

            Console.WriteLine($"{availableDetales.Count + 1}. Вернуться в главное меню");
            Console.Write("Выберите деталь для заказа: ");

            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                if (choice >= 1 && choice <= availableDetales.Count)
                {
                    var selectedDetale = availableDetales[choice - 1];
                    Console.Write($"Сколько {selectedDetale.Name} заказать? ");

                    if (int.TryParse(Console.ReadLine(), out int quantity) && quantity > 0)
                    {
                        var totalCost = selectedDetale.Price * quantity;

                        if (Sklad.Balance >= totalCost)
                        {
                            Sklad.Balance -= totalCost;
                            pendingOrders.Add(new PurchaseOrder
                            {
                                DetaleID = selectedDetale.ID,
                                Quantity = quantity,
                                RemainingCars = 2
                            });

                            Console.WriteLine($"Заказ на {quantity} {selectedDetale.Name} оформлен!");
                            Console.WriteLine($"Спиcано: {totalCost}. Поставка через 2 машины.");
                        }
                        else
                        {
                            Console.WriteLine("Недостаточно средств!");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Неверное количество!");
                    }
                }
                else if (choice == availableDetales.Count + 1)
                {
                    Console.WriteLine("Возврат в главное меню...");
                }
                else
                {
                    Console.WriteLine("Неверный выбор!");
                }
            }
            else
            {
                Console.WriteLine("Неверный ввод!");
            }
        }

        private void ProcessPendingOrders()
        {
            for (int i = pendingOrders.Count - 1; i >= 0; i--)
            {
                pendingOrders[i].RemainingCars--;

                if (pendingOrders[i].RemainingCars <= 0)
                {
                    var order = pendingOrders[i];
                    var SkladDetale = SkladDetales.FirstOrDefault(wp => wp.DetaleID == order.DetaleID);

                    if (SkladDetale != null)
                    {
                        SkladDetale.Count += order.Quantity;
                    }
                    else
                    {
                        SkladDetales.Add(new SkladDetale
                        {
                            SkladID = Sklad.ID,
                            DetaleID = order.DetaleID,
                            Count = order.Quantity
                        });
                    }

                    var Detale = availableDetales.First(p => p.ID == order.DetaleID);
                    Console.WriteLine($"Поставка получена: {order.Quantity} {Detale.Name}");
                    pendingOrders.RemoveAt(i);
                }
            }
        }

        private void ShowSkladStatus()
        {
            Console.WriteLine($"\nБаланс: {Sklad.Balance}");
            Console.WriteLine("Склад:");

            var hasDetales = false;
            foreach (var Detale in availableDetales)
            {
                var count = GetDetaleCount(Detale.ID);
                if (count > 0)
                {
                    Console.WriteLine($"  {Detale.Name}: {count} шт.");
                    hasDetales = true;
                }
            }

            if (!hasDetales)
            {
                Console.WriteLine("  Склад пуст!");
            }

            if (pendingOrders.Count > 0)
            {
                Console.WriteLine("\nОжидаются поставки:");
                foreach (var order in pendingOrders)
                {
                    var Detale = availableDetales.First(p => p.ID == order.DetaleID);
                    Console.WriteLine($"  {Detale.Name}: {order.Quantity} шт. (через {order.RemainingCars} машин)");
                }
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var game = new AutoServiceGame();
                game.StartGame();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }

            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}