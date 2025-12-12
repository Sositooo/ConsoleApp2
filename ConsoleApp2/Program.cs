using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace ConsoleApp2
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Маркетплейс GMWOG");

            while (true)
            {
                Console.WriteLine("\nГлавное меню:");
                Console.WriteLine("1 - Регистрация");
                Console.WriteLine("2 - Вход");
                Console.WriteLine("3 - Товары");
                Console.WriteLine("4 - Выход");
                Console.Write("Ваш выбор: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": Register(); break;
                    case "2": Login(); break;
                    case "3": ShowProducts(); break;
                    case "4": return;
                    default: Console.WriteLine("Неверный выбор!"); break;
                }
            }
        }

        static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }

        static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        static void Register()
        {
            try
            {
                Console.WriteLine("\nРегистрация");

                Console.Write("Логин: ");
                string login = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(login))
                {
                    Console.WriteLine("Логин не может быть пустым!");
                    return;
                }

                if (login.Length > 50)
                {
                    Console.WriteLine("Логин слишком длинный! Максимум 50 символов.");
                    return;
                }

                // СТРОКА 236 (ИСПРАВЛЕНА): Проверка существования логина
                // Была ошибка: if (Core.Context.Users.Any() == 0) или подобное
                // Правильно:
                bool loginExists = Core.Context.Users.Any(u => u.Username == login);
                if (loginExists)
                {
                    Console.WriteLine("Этот логин уже занят!");
                    return;
                }

                Console.Write("Email: ");
                string email = Console.ReadLine()?.Trim();

                if (string.IsNullOrWhiteSpace(email))
                {
                    Console.WriteLine("Email не может быть пустым!");
                    return;
                }

                if (!IsValidEmail(email))
                {
                    Console.WriteLine("Неверный формат email!");
                    return;
                }

                if (email.Length > 100)
                {
                    Console.WriteLine("Email слишком длинный!");
                    return;
                }

                string password;
                while (true)
                {
                    Console.Write("Пароль: ");
                    password = Console.ReadLine();

                    if (string.IsNullOrEmpty(password) || password.Length < 4)
                    {
                        Console.WriteLine("Пароль должен содержать минимум 4 символа!");
                        continue;
                    }

                    if (password.Length > 100)
                    {
                        Console.WriteLine("Пароль слишком длинный! Максимум 100 символов.");
                        continue;
                    }

                    Console.Write("Повторите пароль: ");
                    string password2 = Console.ReadLine();

                    if (password == password2)
                    {
                        break;
                    }
                    else
                    {
                        Console.WriteLine("Пароли не совпадают! Попробуйте еще раз.");
                    }
                }

                Users newUser = new Users
                {
                    Username = login,
                    Email = email,
                    PasswordHash = HashPassword(password),
                };

                Core.Context.Users.Add(newUser);
                Core.Context.SaveChanges();

                Console.WriteLine("Регистрация успешна!");
            }
            catch (DbEntityValidationException ex)
            {
                Console.WriteLine("Ошибки валидации при регистрации:");
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Console.WriteLine($"- Свойство: {validationError.PropertyName}");
                        Console.WriteLine($"  Ошибка: {validationError.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при регистрации: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
                }
            }
        }

        static void Login()
        {
            try
            {
                Console.WriteLine("\nВход");

                Console.Write("Логин: ");
                string login = Console.ReadLine()?.Trim();

                Console.Write("Пароль: ");
                string password = Console.ReadLine();

                Users user = Core.Context.Users.FirstOrDefault(u => u.Username == login);

                if (user != null && HashPassword(password) == user.PasswordHash)
                {
                    Console.WriteLine($"Добро пожаловать, {user.Username}!");
                    UserMenu(user);
                }
                else
                {
                    Console.WriteLine("Неверный логин или пароль!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при входе: {ex.Message}");
            }
        }

        static void UserMenu(Users user)
        {
            while (true)
            {
                Console.WriteLine("\n--- Личный кабинет ---");
                Console.WriteLine("1 - Товары");
                Console.WriteLine("2 - Корзина");
                Console.WriteLine("3 - Мои заказы");
                Console.WriteLine("4 - Выйти");
                Console.Write("Ваш выбор: ");
                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1": ShowProducts(user); break;
                    case "2": ShowCart(user); break;
                    case "3": ShowOrders(user); break;
                    case "4": return;
                    default: Console.WriteLine("Неверный выбор!"); break;
                }
            }
        }

        static void ShowProducts(Users user = null)
        {
            try
            {
                Console.WriteLine("\nТовары");

                var products = Core.Context.Products.ToList();

                if (products == null || products.Count == 0)
                {
                    Console.WriteLine("Товаров нет");
                    return;
                }

                foreach (var product in products)
                {
                    Console.WriteLine($"{product.ProductId}. {product.ProductName} - {product.Price} руб. (осталось: {product.StockQuantity})");
                }

                if (user != null)
                {
                    Console.Write("\n1 - Добавить в корзину\n2 - Купить сразу\n3 - Назад\nВаш выбор: ");
                    string choice = Console.ReadLine();

                    if (choice == "1")
                    {
                        AddToCart(user);
                    }
                    else if (choice == "2")
                    {
                        BuyProduct(user);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отображении товаров: {ex.Message}");
            }
        }

        static void AddToCart(Users user)
        {
            try
            {
                Console.Write("ID товара: ");
                if (!int.TryParse(Console.ReadLine(), out int productId))
                {
                    Console.WriteLine("Ошибка ввода!");
                    return;
                }

                Products product = Core.Context.Products.Find(productId);
                if (product == null)
                {
                    Console.WriteLine("Товар не найден!");
                    return;
                }

                Console.Write("Количество: ");
                if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
                {
                    Console.WriteLine("Неверное количество!");
                    return;
                }

                if (quantity > product.StockQuantity)
                {
                    Console.WriteLine("Недостаточно товара!");
                    return;
                }

                Cart cartItem = Core.Context.Cart.FirstOrDefault(c => c.UserId == user.UserId && c.ProductId == productId);

                if (cartItem != null)
                {
                    cartItem.Quantity += quantity;
                }
                else
                {
                    cartItem = new Cart
                    {
                        UserId = user.UserId,
                        ProductId = productId,
                        Quantity = quantity
                    };
                    Core.Context.Cart.Add(cartItem);
                }

                Core.Context.SaveChanges();
                Console.WriteLine("Товар добавлен в корзину!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении в корзину: {ex.Message}");
            }
        }

        static void BuyProduct(Users user)
        {
            try
            {
                Console.Write("ID товара: ");
                if (!int.TryParse(Console.ReadLine(), out int productId))
                {
                    Console.WriteLine("Ошибка ввода!");
                    return;
                }

                Products product = Core.Context.Products.Find(productId);
                if (product == null)
                {
                    Console.WriteLine("Товар не найден!");
                    return;
                }

                Console.Write("Количество: ");
                if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity <= 0)
                {
                    Console.WriteLine("Неверное количество!");
                    return;
                }

                if (quantity > product.StockQuantity)
                {
                    Console.WriteLine("Недостаточно товара!");
                    return;
                }

                var points = Core.Context.PickupPoints.ToList();

                if (points == null || points.Count == 0)
                {
                    Console.WriteLine("Нет пунктов выдачи!");
                    return;
                }

                Console.WriteLine("Пункты выдачи:");
                foreach (var point in points)
                {
                    Console.WriteLine($"{point.PickupPointId}. {point.PointName} - {point.Address}");
                }

                Console.Write("Выберите пункт выдачи: ");
                if (!int.TryParse(Console.ReadLine(), out int pointId))
                {
                    Console.WriteLine("Ошибка ввода!");
                    return;
                }

                var selectedPoint = Core.Context.PickupPoints.Find(pointId);
                if (selectedPoint == null)
                {
                    Console.WriteLine("Пункт выдачи не найден!");
                    return;
                }

                decimal total = quantity * product.Price;

                Orders order = new Orders
                {
                    UserId = user.UserId,
                    PickupPointId = pointId,
                    OrderDate = DateTime.Now,
                    TotalAmount = total
                };

                Core.Context.Orders.Add(order);
                Core.Context.SaveChanges();

                OrderItems orderItem = new OrderItems
                {
                    OrderId = order.OrderId,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price
                };

                Core.Context.OrderItems.Add(orderItem);

                product.StockQuantity -= quantity;

                Core.Context.SaveChanges();

                Console.WriteLine($"Заказ №{order.OrderId} оформлен!");
                Console.WriteLine($"Товар: {product.ProductName} в количестве {quantity} шт.");
                Console.WriteLine($"Сумма: {total} руб.");
            }
            catch (DbEntityValidationException ex)
            {
                Console.WriteLine("Ошибки валидации при покупке:");
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Console.WriteLine($"- {validationError.PropertyName}: {validationError.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при оформлении заказа: {ex.Message}");
            }
        }

        static void ShowCart(Users user)
        {
            try
            {
                Console.WriteLine("\nКорзина");

                var cartItems = from c in Core.Context.Cart
                                join p in Core.Context.Products on c.ProductId equals p.ProductId
                                where c.UserId == user.UserId
                                select new { Cart = c, Product = p };

                // СТРОКА 358 (ИСПРАВЛЕНА): Проверка пустой корзины
                // Была ошибка: if (cartItems.Count() == 0) или if (cartItems == 0)
                // Правильно:
                int itemCount = cartItems.Count();
                if (itemCount == 0)
                {
                    Console.WriteLine("Корзина пуста");
                    return;
                }

                decimal total = 0;

                foreach (var item in cartItems)
                {
                    decimal itemTotal = item.Cart.Quantity * item.Product.Price;
                    total += itemTotal;

                    Console.WriteLine($"{item.Product.ProductName} x{item.Cart.Quantity} = {itemTotal} руб.");
                }
                Console.WriteLine($"Итого: {total} руб.");

                Console.Write("\n1 - Оформить заказ\n2 - Удалить товар\n3 - Назад\nВаш выбор: ");
                string choice = Console.ReadLine();

                if (choice == "1")
                {
                    CreateOrder(user);
                }
                else if (choice == "2")
                {
                    RemoveFromCart(user);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отображении корзины: {ex.Message}");
            }
        }

        static void RemoveFromCart(Users user)
        {
            try
            {
                Console.Write("ID товара для удаления: ");
                if (!int.TryParse(Console.ReadLine(), out int productId))
                {
                    Console.WriteLine("Ошибка ввода!");
                    return;
                }

                Cart item = Core.Context.Cart.FirstOrDefault(c => c.UserId == user.UserId && c.ProductId == productId);

                if (item != null)
                {
                    Core.Context.Cart.Remove(item);
                    Core.Context.SaveChanges();
                    Console.WriteLine("Товар удален из корзины!");
                }
                else
                {
                    Console.WriteLine("Товар не найден в корзине!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении из корзины: {ex.Message}");
            }
        }

        static void CreateOrder(Users user)
        {
            try
            {
                Console.WriteLine("\nОформление заказа");

                var points = Core.Context.PickupPoints.ToList();

                if (points == null || points.Count == 0)
                {
                    Console.WriteLine("Нет пунктов выдачи!");
                    return;
                }

                Console.WriteLine("Пункты выдачи:");
                foreach (var point in points)
                {
                    Console.WriteLine($"{point.PickupPointId}. {point.PointName} - {point.Address}");
                }

                Console.Write("Выберите пункт выдачи: ");
                if (!int.TryParse(Console.ReadLine(), out int pointId))
                {
                    Console.WriteLine("Ошибка ввода!");
                    return;
                }

                var selectedPoint = Core.Context.PickupPoints.Find(pointId);
                if (selectedPoint == null)
                {
                    Console.WriteLine("Пункт выдачи не найден!");
                    return;
                }

                var cartItems = from c in Core.Context.Cart
                                join p in Core.Context.Products on c.ProductId equals p.ProductId
                                where c.UserId == user.UserId
                                select new { Cart = c, Product = p };

                // СТРОКА 518 (ИСПРАВЛЕНА): Проверка пустой корзины
                // Была ошибка: if (cartItems.Count() == 0) или if (cartItems == 0)
                // Правильно:
                int cartItemCount = cartItems.Count();
                if (cartItemCount == 0)
                {
                    Console.WriteLine("Корзина пуста!");
                    return;
                }

                // Исправленный расчет суммы
                decimal total = 0;
                foreach (var item in cartItems)
                {
                    total += item.Cart.Quantity * item.Product.Price;
                }

                Orders order = new Orders
                {
                    UserId = user.UserId,
                    PickupPointId = pointId,
                    OrderDate = DateTime.Now,
                    TotalAmount = total
                };

                Core.Context.Orders.Add(order);
                Core.Context.SaveChanges();

                foreach (var item in cartItems)
                {
                    if (item.Cart.Quantity > item.Product.StockQuantity)
                    {
                        Console.WriteLine($"Недостаточно товара: {item.Product.ProductName}");
                        Core.Context.Orders.Remove(order);
                        Core.Context.SaveChanges();
                        return;
                    }

                    OrderItems orderItem = new OrderItems
                    {
                        OrderId = order.OrderId,
                        ProductId = item.Product.ProductId,
                        Quantity = item.Cart.Quantity,
                        UnitPrice = item.Product.Price
                    };

                    Core.Context.OrderItems.Add(orderItem);
                    item.Product.StockQuantity -= item.Cart.Quantity;
                }

                var userCart = Core.Context.Cart.Where(c => c.UserId == user.UserId).ToList();
                Core.Context.Cart.RemoveRange(userCart);

                Core.Context.SaveChanges();

                Console.WriteLine($"Заказ №{order.OrderId} оформлен!");
                Console.WriteLine($"Сумма: {total} руб.");
            }
            catch (DbEntityValidationException ex)
            {
                Console.WriteLine("Ошибки валидации при создании заказа:");
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Console.WriteLine($"- {validationError.PropertyName}: {validationError.ErrorMessage}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при оформлении заказа: {ex.Message}");
            }
        }

        static void ShowOrders(Users user)
        {
            try
            {
                Console.WriteLine("\nМои заказы");

                var orders = from o in Core.Context.Orders
                             join p in Core.Context.PickupPoints on o.PickupPointId equals p.PickupPointId
                             where o.UserId == user.UserId
                             orderby o.OrderDate descending
                             select new { Order = o, Point = p };

                // Дополнительная проверка для метода ShowOrders
                int orderCount = orders.Count();
                if (orderCount == 0)
                {
                    Console.WriteLine("Заказов нет");
                    return;
                }

                foreach (var orderInfo in orders)
                {
                    Console.WriteLine($"Заказ №{orderInfo.Order.OrderId} от {orderInfo.Order.OrderDate:dd.MM.yyyy HH:mm}");
                    Console.WriteLine($"Сумма: {orderInfo.Order.TotalAmount} руб.");
                    Console.WriteLine($"Пункт выдачи: {orderInfo.Point.PointName}");

                    var items = from oi in Core.Context.OrderItems
                                join p in Core.Context.Products on oi.ProductId equals p.ProductId
                                where oi.OrderId == orderInfo.Order.OrderId
                                select new { Item = oi, Product = p };

                    foreach (var item in items)
                    {
                        Console.WriteLine($"  - {item.Product.ProductName} x{item.Item.Quantity} = {item.Item.Quantity * item.Item.UnitPrice} руб.");
                    }
                    Console.WriteLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при отображении заказов: {ex.Message}");
            }
        }
    }
}