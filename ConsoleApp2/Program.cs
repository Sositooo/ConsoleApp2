using ConsoleApp2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AutoService
{
    public class Part
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
    }

    public class Warehouse
    {
        public int ID { get; set; }
        public decimal Balance { get; set; }
    }

    public class WarehousePart
    {
        public int WarehouseID { get; set; }
        public int PartID { get; set; }
        public int Count { get; set; }
    }

    public class Customer
    {
        public int ID { get; set; }
        public string CarModel { get; set; }
        public int BrokenPartID { get; set; }
        public decimal RepairCost { get; set; }
        public bool IsServed { get; set; }
    }

    public class PurchaseOrder
    {
        public int PartID { get; set; }
        public int Quantity { get; set; }
        public int RemainingCars { get; set; }
    }

    public class AutoServiceGame
    {
        private Warehouse warehouse;
        private List<Part> availableParts;
        private List<WarehousePart> warehouseParts;
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
            LoadWarehouseData();
            LoadAvailableParts();
            Console.WriteLine("Добро пожаловать в автосервис!");
            Console.WriteLine($"Начальный баланс: {warehouse.Balance}");
        }

        private void LoadWarehouseData()
        {
            var dbWarehouse = Core.Context.Sklad.FirstOrDefault();
            if (dbWarehouse != null)
            {
                warehouse = new Warehouse
                {
                    ID = dbWarehouse.ID,
                    Balance = (int)dbWarehouse.Balance
                };
            }
            warehouseParts = new List<WarehousePart>();
            var dbWarehouseParts = Core.Context.DetaleSklad.ToList();
            foreach (var dbPart in dbWarehouseParts)
            {
                warehouseParts.Add(new WarehousePart
                {
                    WarehouseID = dbPart.SkladID,
                    PartID = dbPart.DetaleID,
                    Count = (int)dbPart.Count
                });
            }
        }

        private void LoadAvailableParts()
        {
            availableParts = new List<Part>();
            var dbParts = Core.Context.Detale.ToList();
            foreach (var dbPart in dbParts)
            {
                availableParts.Add(new Part
                {
                    ID = dbPart.ID,
                    Name = dbPart.Name,
                    Price = (decimal)dbPart.Price
                });
            }
        }
        private void SaveGameState()
        {
            try
            {
                var dbWarehouse = Core.Context.Sklad.FirstOrDefault(s => s.ID == warehouse.ID);
                if (dbWarehouse != null)
                {
                    dbWarehouse.Balance = warehouse.Balance;
                }
                foreach (var wp in warehouseParts)
                {
                    var dbWarehousePart = Core.Context.DetaleSklad
                        .FirstOrDefault(ds => ds.SkladID == wp.WarehouseID && ds.DetaleID == wp.PartID);

                    if (dbWarehousePart != null)
                    {
                        dbWarehousePart.Count = wp.Count;
                    }
                    else
                    {
                        var newWarehousePart = new DetaleSklad
                        {
                            SkladID = wp.WarehouseID,
                            DetaleID = wp.PartID,
                            Count = wp.Count
                        };
                        Core.Context.DetaleSklad.Add(newWarehousePart);
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

                if (warehouse.Balance <= 0)
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
                        ShowWarehouseStatus();
                        break;
                    default:
                        Console.WriteLine("Неверный выбор! Попробуйте снова.");
                        break;
                }
            }
        }

        private Customer GenerateCustomer()
        {
            var brokenPart = availableParts[random.Next(availableParts.Count)];
            var workCost = brokenPart.Price * 0.3m;
            var repairCost = brokenPart.Price + workCost;

            return new Customer
            {
                ID = random.Next(1000, 9999),
                CarModel = GenerateCarModel(),
                BrokenPartID = brokenPart.ID,
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
            var brokenPart = availableParts.First(p => p.ID == customer.BrokenPartID);

            Console.WriteLine($"Клиент приехал на {customer.CarModel}");
            Console.WriteLine($"Сломана деталь: {brokenPart.Name}");
            Console.WriteLine($"Стоимость ремонта: {customer.RepairCost}");
            Console.WriteLine($"На складе есть: {GetPartCount(customer.BrokenPartID)} шт.");
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
            var brokenPartID = customer.BrokenPartID;
            var partCount = GetPartCount(brokenPartID);

            if (partCount > 0)
            {
                UsePart(brokenPartID);
                warehouse.Balance += customer.RepairCost;
                customer.IsServed = true;
                Console.WriteLine($"Ремонт выполнен успешно! Получено {customer.RepairCost}");
            }
            else
            {
                ReplaceWithRandomPart(customer);
            }
        }

        private void RefuseOrder(Customer customer)
        {
            var penalty = customer.RepairCost * 0.2m; // 20% штраф
            warehouse.Balance -= penalty;
            Console.WriteLine($"Отказ в обслуживании. Штраф: {penalty}");
        }

        private void ReplaceWithRandomPart(Customer customer)
        {
            var availablePartIDs = warehouseParts.Where(wp => wp.Count > 0).Select(wp => wp.PartID).ToList();

            if (availablePartIDs.Count > 0)
            {
                var randomPartID = availablePartIDs[random.Next(availablePartIDs.Count)];
                var randomPart = availableParts.First(p => p.ID == randomPartID);
                var compensation = customer.RepairCost * 1.5m; // 150% компенсация

                UsePart(randomPartID);
                warehouse.Balance -= compensation;

                Console.WriteLine($"Нужной детали нет! Установлена {randomPart.Name}");
                Console.WriteLine($"Клиент недоволен! Выплачена компенсация: {compensation}");
            }
            else
            {
                Console.WriteLine("На складе нет деталей! Ремонт невозможен.");
            }
        }

        private void UsePart(int partID)
        {
            var warehousePart = warehouseParts.FirstOrDefault(wp => wp.PartID == partID);
            if (warehousePart != null && warehousePart.Count > 0)
            {
                warehousePart.Count--;
            }
        }

        private int GetPartCount(int partID)
        {
            var warehousePart = warehouseParts.FirstOrDefault(wp => wp.PartID == partID);
            return warehousePart?.Count ?? 0;
        }

        private void ShowPurchaseMenu()
        {
            Console.WriteLine("\nМеню закупки деталей");
            Console.WriteLine("Доступные детали:");

            for (int i = 0; i < availableParts.Count; i++)
            {
                var part = availableParts[i];
                var count = GetPartCount(part.ID);
                Console.WriteLine($"{i + 1}. {part.Name} - {part.Price} (на складе: {count})");
            }

            Console.WriteLine($"{availableParts.Count + 1}. Вернуться в главное меню");
            Console.Write("Выберите деталь для заказа: ");

            if (int.TryParse(Console.ReadLine(), out int choice))
            {
                if (choice >= 1 && choice <= availableParts.Count)
                {
                    var selectedPart = availableParts[choice - 1];
                    Console.Write($"Сколько {selectedPart.Name} заказать? ");

                    if (int.TryParse(Console.ReadLine(), out int quantity) && quantity > 0)
                    {
                        var totalCost = selectedPart.Price * quantity;

                        if (warehouse.Balance >= totalCost)
                        {
                            warehouse.Balance -= totalCost;
                            pendingOrders.Add(new PurchaseOrder
                            {
                                PartID = selectedPart.ID,
                                Quantity = quantity,
                                RemainingCars = 2
                            });

                            Console.WriteLine($"Заказ на {quantity} {selectedPart.Name} оформлен!");
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
                else if (choice == availableParts.Count + 1)
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
                    var warehousePart = warehouseParts.FirstOrDefault(wp => wp.PartID == order.PartID);

                    if (warehousePart != null)
                    {
                        warehousePart.Count += order.Quantity;
                    }
                    else
                    {
                        warehouseParts.Add(new WarehousePart
                        {
                            WarehouseID = warehouse.ID,
                            PartID = order.PartID,
                            Count = order.Quantity
                        });
                    }

                    var part = availableParts.First(p => p.ID == order.PartID);
                    Console.WriteLine($"Поставка получена: {order.Quantity} {part.Name}");
                    pendingOrders.RemoveAt(i);
                }
            }
        }

        private void ShowWarehouseStatus()
        {
            Console.WriteLine($"\nБаланс: {warehouse.Balance}");
            Console.WriteLine("Склад:");

            var hasParts = false;
            foreach (var part in availableParts)
            {
                var count = GetPartCount(part.ID);
                if (count > 0)
                {
                    Console.WriteLine($"  {part.Name}: {count} шт.");
                    hasParts = true;
                }
            }

            if (!hasParts)
            {
                Console.WriteLine("  Склад пуст!");
            }

            if (pendingOrders.Count > 0)
            {
                Console.WriteLine("\nОжидаются поставки:");
                foreach (var order in pendingOrders)
                {
                    var part = availableParts.First(p => p.ID == order.PartID);
                    Console.WriteLine($"  {part.Name}: {order.Quantity} шт. (через {order.RemainingCars} машин)");
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