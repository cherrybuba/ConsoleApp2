using System;
using System.Collections.Generic;
using System.Linq;

namespace PizzaSystem
{
    enum PizzaSize
    {
        Маленькая,
        Средняя,
        Большая
    }
    interface IEntity
    {
        Guid Id { get; }
    }

    interface IPriceable
    {
        decimal GetTotalPrice();
    }

    interface ICloneablePizza
    {
        Pizza Clone();
    }
    abstract class Product : IEntity
    {
        public Guid Id { get; }
        private string name;
        private decimal price;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public decimal Price
        {
            get { return price; }
            set
            {
                if (value >= 0)
                    price = value;
            }
        }

        public Product(string name, decimal price)
        {
            Id = Guid.NewGuid();
            this.name = name;
            this.price = price;
        }

        public abstract void Show();

    }

    class Ingredient : Product
    {
        public Ingredient(string name, decimal price) : base(name, price) { }

        public override void Show()
        {
            Console.WriteLine("Ингредиент: " + Name + " - " + Price);
        }
    }

    class PizzaBase : Product
    {
        public bool IsClassic { get; private set; }

        public PizzaBase(string name, decimal price, bool isClassic)
            : base(name, price)
        {
            IsClassic = isClassic;
        }

        public override void Show()
        {
            Console.WriteLine("Основа: " + Name + " - " + Price);
        }
    }

    class Border : Product
    {
        public enum BorderAccessType
        {
            Разрешенные,
            Запрещенные
        }

        public BorderAccessType AccessType { get; set; }
        public List<Guid> PizzaIds { get; private set; }
        private List<Ingredient> borderIngredients;

        public Border(string name, decimal price, BorderAccessType accessType) : base(name, price)
        {
            PizzaIds = new List<Guid>();
            borderIngredients = new List<Ingredient>();
            AccessType = accessType;
        }

        public void AddPizzaId(Guid pizzaId)
        {
            if (!PizzaIds.Contains(pizzaId))
                PizzaIds.Add(pizzaId);
        }

        public void RemovePizzaId(Guid pizzaId)
        {
            PizzaIds.Remove(pizzaId);
        }

        public void AddIngredient(Ingredient ingredient)
        {
            borderIngredients.Add(ingredient);
        }

        public void RemoveIngredient(int index)
        {
            if (index >= 0 && index < borderIngredients.Count)
                borderIngredients.RemoveAt(index);
        }

        public List<Ingredient> GetIngredients()
        {
            return borderIngredients;
        }

        public bool IsAllowedForPizza(Guid pizzaId)
        {
            if (AccessType == BorderAccessType.Разрешенные)
                return PizzaIds.Contains(pizzaId);
            else
                return !PizzaIds.Contains(pizzaId);
        }

        public override void Show()
        {
            Console.WriteLine($"Бортик: {Name} - {Price}");
            Console.WriteLine($"Тип доступа: {AccessType}");
            if (borderIngredients.Count > 0)
            {
                Console.WriteLine("Ингредиенты бортика:");
                foreach (var ing in borderIngredients)
                {
                    Console.WriteLine($"  - {ing.Name}");
                }
            }
        }

        public void ShowPizzaAccess(List<Pizza> allPizzas)
        {
            Console.WriteLine($"Бортик '{Name}':");
            Console.WriteLine($"Тип: {(AccessType == BorderAccessType.Разрешенные ? "Разрешен для" : "Запрещен для")} следующих пицц:");

            foreach (var pizzaId in PizzaIds)
            {
                var pizza = allPizzas.FirstOrDefault(p => p.Id == pizzaId);
                if (pizza != null)
                    Console.WriteLine($"  - {pizza.Name}");
            }
        }
    }

    class CombinedBorder : Border
    {
        public Border FirstHalfBorder { get; private set; }
        public Border SecondHalfBorder { get; private set; }

        public CombinedBorder(Border firstHalf, Border secondHalf, decimal price)
            : base("Комбинированный бортик", price, BorderAccessType.Разрешенные)
        {
            FirstHalfBorder = firstHalf;
            SecondHalfBorder = secondHalf;

            foreach (var pizzaId in firstHalf.PizzaIds)
            {
                if (!PizzaIds.Contains(pizzaId))
                    PizzaIds.Add(pizzaId);
            }

            foreach (var pizzaId in secondHalf.PizzaIds)
            {
                if (!PizzaIds.Contains(pizzaId))
                    PizzaIds.Add(pizzaId);
            }
        }

        public override void Show()
        {
            Console.WriteLine($"Комбинированный бортик:");
            Console.WriteLine("  Первая половина: ");
            FirstHalfBorder.Show();
            Console.WriteLine("  Вторая половина: ");
            SecondHalfBorder.Show();
            Console.WriteLine($"Общая цена: {Price}");
        }

        public bool IsAllowedForPizzaHalf(Guid pizzaId, bool isFirstHalf)
        {
            if (isFirstHalf)
                return FirstHalfBorder.IsAllowedForPizza(pizzaId);
            else
                return SecondHalfBorder.IsAllowedForPizza(pizzaId);
        }
    }

    class Pizza : IEntity, IPriceable, ICloneablePizza
    {
        public Guid Id { get; private set; }
        public string Name { get; set; }
        public PizzaBase Base { get; private set; }
        public Border Border { get; set; }
        public PizzaSize Size { get; set; }
        private List<Ingredient> ingredients;

        private const decimal SMALL_SIZE_MULTIPLIER = 1.0m;
        private const decimal MEDIUM_SIZE_MULTIPLIER = 1.4m;
        private const decimal LARGE_SIZE_MULTIPLIER = 1.8m;

        public Pizza(string name, PizzaBase pizzaBase)
        {
            if (pizzaBase == null)
                throw new Exception("Нельзя создать пиццу без основы!");

            Id = Guid.NewGuid();
            Name = name;
            Base = pizzaBase;
            ingredients = new List<Ingredient>();
            Size = PizzaSize.Маленькая;
        }

        public void SetBase(PizzaBase newBase)
        {
            if (newBase != null)
                Base = newBase;
        }

        public void AddIngredient(Ingredient ingredient)
        {
            ingredients.Add(ingredient);
        }

        public void DoubleIngredient(int index)
        {
            if (index >= 0 && index < ingredients.Count)
            {
                ingredients.Add(ingredients[index]);
            }
        }

        public void RemoveIngredient(int index)
        {
            if (index >= 0 && index < ingredients.Count)
                ingredients.RemoveAt(index);
        }

        public void ShowIngredients()
        {
            for (int i = 0; i < ingredients.Count; i++)
            {
                Console.WriteLine(i + " - " + ingredients[i].Name);
            }
        }

        public List<Ingredient> GetIngredients()
        {
            return ingredients;
        }

        protected decimal GetSizeMultiplier()
        {
            switch (Size)
            {
                case PizzaSize.Маленькая:
                    return SMALL_SIZE_MULTIPLIER;
                case PizzaSize.Средняя:
                    return MEDIUM_SIZE_MULTIPLIER;
                case PizzaSize.Большая:
                    return LARGE_SIZE_MULTIPLIER;
                default:
                    return SMALL_SIZE_MULTIPLIER;
            }
        }

        public virtual decimal GetTotalPrice()
        {
            decimal total = Base.Price;

            foreach (var ing in ingredients)
                total += ing.Price;

            if (Border != null)
            {
                total += Border.Price;
                foreach (var ing in Border.GetIngredients())
                    total += ing.Price;
            }

            total *= GetSizeMultiplier();

            return total;
        }

        public virtual Pizza Clone()
        {
            Pizza copy = new Pizza(this.Name, this.Base);
            copy.Size = this.Size;
            copy.Border = this.Border;

            foreach (var ingredient in this.ingredients)
            {
                copy.AddIngredient(ingredient);
            }

            return copy;
        }
        public virtual void Show()
        {
            string sizeName = Size.ToString();
            Console.WriteLine($"Пицца: {Name}");
            Console.WriteLine($"Размер: {sizeName} (x{GetSizeMultiplier():F1})");
            Console.WriteLine($"Основа: {Base.Name}");
            Console.WriteLine("Ингредиенты:");

            ShowIngredients();

            if (Border != null)
            {
                Console.WriteLine("Бортик: " + Border.Name);
                if (Border.GetIngredients().Count > 0)
                {
                    Console.WriteLine("  Ингредиенты бортика:");
                    foreach (var ing in Border.GetIngredients())
                    {
                        Console.WriteLine($"    - {ing.Name}");
                    }
                }
            }

            Console.WriteLine("Цена: " + GetTotalPrice() + " руб.");
        }
    }

    class CombinedPizza : Pizza
    {
        private Pizza firstHalf;
        private Pizza secondHalf;
        public CombinedBorder CombinedBorder { get; set; }

        public CombinedPizza(Pizza p1, Pizza p2, PizzaBase pizzaBase)
            : base("Комбинированная", pizzaBase)
        {
            firstHalf = p1;
            secondHalf = p2;
        }

        public override Pizza Clone()
        {
            CombinedPizza copy = new CombinedPizza(firstHalf.Clone(), secondHalf.Clone(), this.Base);
            copy.Size = this.Size;
            copy.Border = this.Border;
            copy.CombinedBorder = this.CombinedBorder;

            return copy;
        }

        public override decimal GetTotalPrice()
        {
            decimal firstHalfPrice = firstHalf.GetTotalPrice();
            decimal secondHalfPrice = secondHalf.GetTotalPrice();

            if (CombinedBorder != null)
            {
                firstHalfPrice += CombinedBorder.FirstHalfBorder.Price;
                secondHalfPrice += CombinedBorder.SecondHalfBorder.Price;
            }

            decimal averagePrice = (firstHalfPrice + secondHalfPrice) / 2;
            return averagePrice * GetSizeMultiplier();
        }

        public override void Show()
        {
            string sizeName = Size.ToString();
            Console.WriteLine($"Пицца: {Name}");
            Console.WriteLine($"Размер: {sizeName} (x{GetSizeMultiplier():F1})");
            Console.WriteLine($"Основа: {Base.Name}");

            Console.WriteLine("Первая половина:");
            Console.WriteLine($"  Пицца: {firstHalf.Name}");
            firstHalf.ShowIngredients();

            Console.WriteLine("Вторая половина:");
            Console.WriteLine($"  Пицца: {secondHalf.Name}");
            secondHalf.ShowIngredients();

            if (CombinedBorder != null)
            {
                Console.WriteLine("Комбинированный бортик:");
                CombinedBorder.Show();
            }
            else if (Border != null)
            {
                Console.WriteLine("Бортик: " + Border.Name);
                if (Border.GetIngredients().Count > 0)
                {
                    Console.WriteLine("  Ингредиенты бортика:");
                    foreach (var ing in Border.GetIngredients())
                    {
                        Console.WriteLine($"    - {ing.Name}");
                    }
                }
            }

            Console.WriteLine("Цена: " + GetTotalPrice() + " руб.");
        }
    }

    class Order : IEntity, IPriceable
    {
        public Guid Id { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? ScheduledTime { get; set; }

        public string Comment { get; set; }

        private List<Pizza> pizzas;

        public Order()
        {
            Id = Guid.NewGuid();
            CreatedAt = DateTime.Now;
            pizzas = new List<Pizza>();
        }

        public void AddPizza(Pizza pizza)
        {
            pizzas.Add(pizza);
        }

        public decimal GetTotalPrice()
        {
            return pizzas.Sum(p => p.GetTotalPrice());
        }

        public void Show()
        {
            Console.WriteLine($"Заказ {Id}");
            Console.WriteLine("Создан: " + CreatedAt);
            if (ScheduledTime.HasValue)
                Console.WriteLine("Отложен на: " + ScheduledTime.Value);
            if (!string.IsNullOrEmpty(Comment))
                Console.WriteLine("Комментарий: " + Comment);

            Console.WriteLine("Пиццы в заказе:");
            foreach (var p in pizzas)
                p.Show();

            Console.WriteLine("Общая сумма: " + GetTotalPrice() + " руб.");
            Console.WriteLine("------------------------");
        }

        public List<Pizza> GetPizzas()
        {
            return pizzas;
        }
    }

    class OrderPriceComparer : IComparer<Order>
    {
        public int Compare(Order x, Order y)
        {
            return x.GetTotalPrice().CompareTo(y.GetTotalPrice());
        }
    }

    class Program
    {
        static List<Ingredient> ingredients = new List<Ingredient>();
        static List<PizzaBase> bases = new List<PizzaBase>();
        static List<Border> borders = new List<Border>();
        static List<Pizza> pizzas = new List<Pizza>();
        static List<Order> orders = new List<Order>();

        static void Main(string[] args)
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("\n1 - Ингредиенты");
                Console.WriteLine("2 - Основы");
                Console.WriteLine("3 - Бортики");
                Console.WriteLine("4 - Пиццы");
                Console.WriteLine("5 - Заказы");
                Console.WriteLine("0 - Выход");

                string choice = Console.ReadLine();

                if (choice == "1") IngredientMenu();
                else if (choice == "2") BaseMenu();
                else if (choice == "3") BorderMenu();
                else if (choice == "4") PizzaMenu();
                else if (choice == "5") OrderMenu();
                else if (choice == "0") break;
            }
        }

        static void ShowSizeMenu()
        {
            Console.WriteLine("Выберите размер:");
            Console.WriteLine("0 - Маленькая (x1.0)");
            Console.WriteLine("1 - Средняя (x1.4)");
            Console.WriteLine("2 - Большая (x1.8)");
        }

        static Pizza CreateManualPizza()
        {
            Console.WriteLine("Создание пиццы вручную");

            Console.Write("Название: ");
            string name = Console.ReadLine();

            Console.WriteLine("Выберите основу:");
            for (int i = 0; i < bases.Count; i++)
            {
                Console.WriteLine($"{i} - {bases[i].Name}");
            }

            int baseIndex = int.Parse(Console.ReadLine());

            Pizza pizza = new Pizza(name, bases[baseIndex]);

            Console.WriteLine("Выберите ингредиенты через пробел:");
            for (int i = 0; i < ingredients.Count; i++)
            {
                Console.WriteLine($"{i} - {ingredients[i].Name}");
            }

            string[] nums = Console.ReadLine().Split(' ');

            foreach (var n in nums)
            {
                if (int.TryParse(n, out int index))
                {
                    if (index >= 0 && index < ingredients.Count)
                        pizza.AddIngredient(ingredients[index]);
                }
            }

            return pizza;
        }

        static Pizza CreateCombinedPizza()
        {
            Console.Clear();
            Console.WriteLine("Создание комбинированной пиццы \n");

            if (pizzas.Count < 2)
            {
                Console.WriteLine("Нужно минимум 2 пиццы в системе.");
                Console.ReadLine();
                return null;
            }

            if (bases.Count == 0)
            {
                Console.WriteLine("Сначала создайте основу!");
                Console.ReadLine();
                return null;
            }

            Console.WriteLine("Выберите первую половину:");
            for (int i = 0; i < pizzas.Count; i++)
            {
                Console.WriteLine($"{i} - {pizzas[i].Name}");
            }
            int firstIndex = int.Parse(Console.ReadLine());

            Console.WriteLine("Выберите вторую половину:");
            int secondIndex = int.Parse(Console.ReadLine());

            Console.WriteLine("Выберите основу:");
            for (int i = 0; i < bases.Count; i++)
            {
                Console.WriteLine($"{i} - {bases[i].Name}");
            }
            int baseIndex = int.Parse(Console.ReadLine());

            CombinedPizza combined = new CombinedPizza(
                pizzas[firstIndex],
                pizzas[secondIndex],
                bases[baseIndex]
            );

            if (borders.Count > 0)
            {
                Console.WriteLine("\nХотите добавить бортики? (да/нет)");
                string addBorders = Console.ReadLine().ToLower();

                if (addBorders == "да" || addBorders == "yes" || addBorders == "д")
                {
                    Console.WriteLine("\nВыберите тип бортика:");
                    Console.WriteLine("1 - Обычный бортик (одинаковый для всей пиццы)");
                    Console.WriteLine("2 - Комбинированный бортик (разный для половинок)");

                    string borderType = Console.ReadLine();

                    if (borderType == "1")
                    {
                        Console.WriteLine("\nДоступные бортики:");
                        for (int i = 0; i < borders.Count; i++)
                        {
                            Console.WriteLine($"{i} - {borders[i].Name} - {borders[i].Price}");
                        }

                        Console.Write("Выберите номер бортика: ");
                        int borderIndex = int.Parse(Console.ReadLine());

                        if (borderIndex >= 0 && borderIndex < borders.Count)
                        {
                            combined.Border = borders[borderIndex];
                            Console.WriteLine("Бортик добавлен!");
                        }
                    }
                    else if (borderType == "2")
                    {
                        Console.WriteLine("\nВыберите бортик для ПЕРВОЙ половины:");
                        for (int i = 0; i < borders.Count; i++)
                        {
                            Console.WriteLine($"{i} - {borders[i].Name} - {borders[i].Price}");
                        }
                        Console.Write("Номер бортика: ");
                        int firstBorderIndex = int.Parse(Console.ReadLine());

                        Console.WriteLine("\nВыберите бортик для ВТОРОЙ половины:");
                        for (int i = 0; i < borders.Count; i++)
                        {
                            Console.WriteLine($"{i} - {borders[i].Name} - {borders[i].Price}");
                        }
                        Console.Write("Номер бортика: ");
                        int secondBorderIndex = int.Parse(Console.ReadLine());

                        if (firstBorderIndex >= 0 && firstBorderIndex < borders.Count &&
                            secondBorderIndex >= 0 && secondBorderIndex < borders.Count)
                        {
                            decimal totalPrice = borders[firstBorderIndex].Price + borders[secondBorderIndex].Price;
                            CombinedBorder combinedBorder = new CombinedBorder(
                                borders[firstBorderIndex],
                                borders[secondBorderIndex],
                                totalPrice
                            );
                            combined.CombinedBorder = combinedBorder;
                            Console.WriteLine("Комбинированный бортик добавлен!");
                        }
                    }
                }
            }

            return combined;
        }

        static void CreateOrder()
        {
            Console.Clear();
            Console.WriteLine("Создание заказа \n");

            Order order = new Order();
            bool addMorePizzas = true;

            while (addMorePizzas)
            {
                Console.WriteLine("1 - Добавить готовую пиццу");
                Console.WriteLine("2 - Создать вручную");
                Console.WriteLine("3 - Комбинированная пицца");
                Console.WriteLine("0 - Завершить добавление пицц");

                string typeChoice = Console.ReadLine();

                if (typeChoice == "1")
                {
                    if (pizzas.Count == 0)
                    {
                        Console.WriteLine("Нет доступных пицц.");
                        Console.ReadLine();
                        continue;
                    }

                    for (int i = 0; i < pizzas.Count; i++)
                        Console.WriteLine($"{i} - {pizzas[i].Name}");

                    Console.Write("Выберите номер пиццы: ");
                    int index = int.Parse(Console.ReadLine());

                    if (index >= 0 && index < pizzas.Count)
                    {
                        Pizza selectedPizza = pizzas[index].Clone();

                        ShowSizeMenu();
                        int sizeIndex = int.Parse(Console.ReadLine());
                        selectedPizza.Size = (PizzaSize)sizeIndex;

                        if (borders.Count > 0)
                        {
                            Console.WriteLine("Хотите добавить бортик? (да/нет)");
                            string addBorder = Console.ReadLine().ToLower();

                            if (addBorder == "да" || addBorder == "yes" || addBorder == "д")
                            {
                                Console.WriteLine("Доступные бортики:");
                                List<int> availableBorders = new List<int>();

                                for (int i = 0; i < borders.Count; i++)
                                {
                                    if (borders[i].IsAllowedForPizza(selectedPizza.Id))
                                    {
                                        Console.WriteLine($"{i} - {borders[i].Name} - {borders[i].Price}");
                                        availableBorders.Add(i);
                                    }
                                }

                                if (availableBorders.Count > 0)
                                {
                                    Console.Write("Выберите номер бортика: ");
                                    int borderIndex = int.Parse(Console.ReadLine());

                                    if (borderIndex >= 0 && borderIndex < borders.Count &&
                                        borders[borderIndex].IsAllowedForPizza(selectedPizza.Id))
                                    {
                                        selectedPizza.Border = borders[borderIndex];
                                        Console.WriteLine("Бортик добавлен!");
                                    }
                                    else
                                    {
                                        Console.WriteLine("Неверный выбор или бортик недоступен для этой пиццы");
                                    }
                                }
                                else
                                {
                                    Console.WriteLine("Нет доступных бортиков для этой пиццы");
                                }
                            }
                        }

                        Console.WriteLine("Хотите удвоить ингредиенты? (да/нет)");
                        string doubleChoice = Console.ReadLine().ToLower();

                        if (doubleChoice == "да" || doubleChoice == "yes" || doubleChoice == "д")
                        {
                            bool continueDoubling = true;
                            while (continueDoubling)
                            {
                                Console.WriteLine("\nТекущие ингредиенты:");
                                selectedPizza.ShowIngredients();

                                Console.Write("Введите номер ингредиента для удвоения (или 'стоп' для завершения): ");
                                string input = Console.ReadLine();

                                if (input.ToLower() == "стоп" || input.ToLower() == "stop")
                                {
                                    continueDoubling = false;
                                }
                                else if (int.TryParse(input, out int ingIndex))
                                {
                                    selectedPizza.DoubleIngredient(ingIndex);
                                    Console.WriteLine("Ингредиент удвоен!");
                                }
                                else
                                {
                                    Console.WriteLine("Неверный ввод!");
                                }
                            }
                        }

                        order.AddPizza(selectedPizza);
                        Console.WriteLine("Пицца добавлена в заказ!");
                    }
                }
                else if (typeChoice == "2")
                {
                    if (bases.Count == 0 || ingredients.Count == 0)
                    {
                        Console.WriteLine("Сначала создайте основы и ингредиенты!");
                        Console.ReadLine();
                        continue;
                    }

                    Pizza manual = CreateManualPizza();

                    ShowSizeMenu();
                    int sizeIndex = int.Parse(Console.ReadLine());
                    manual.Size = (PizzaSize)sizeIndex;

                    if (borders.Count > 0)
                    {
                        Console.WriteLine("Хотите добавить бортик? (да/нет)");
                        string addBorder = Console.ReadLine().ToLower();

                        if (addBorder == "да" || addBorder == "yes" || addBorder == "д")
                        {
                            Console.WriteLine("Доступные бортики:");
                            List<int> availableBorders = new List<int>();

                            for (int i = 0; i < borders.Count; i++)
                            {
                                if (borders[i].IsAllowedForPizza(manual.Id))
                                {
                                    Console.WriteLine($"{i} - {borders[i].Name} - {borders[i].Price}");
                                    availableBorders.Add(i);
                                }
                            }

                            if (availableBorders.Count > 0)
                            {
                                Console.Write("Выберите номер бортика: ");
                                int borderIndex = int.Parse(Console.ReadLine());

                                if (borderIndex >= 0 && borderIndex < borders.Count &&
                                    borders[borderIndex].IsAllowedForPizza(manual.Id))
                                {
                                    manual.Border = borders[borderIndex];
                                    Console.WriteLine("Бортик добавлен!");
                                }
                                else
                                {
                                    Console.WriteLine("Неверный выбор или бортик недоступен для этой пиццы");
                                }
                            }
                            else
                            {
                                Console.WriteLine("Нет доступных бортиков для этой пиццы");
                            }
                        }
                    }

                    Console.WriteLine("Хотите удвоить ингредиенты? (да/нет)");
                    string doubleChoice = Console.ReadLine().ToLower();

                    if (doubleChoice == "да" || doubleChoice == "yes" || doubleChoice == "д")
                    {
                        bool continueDoubling = true;
                        while (continueDoubling)
                        {
                            Console.WriteLine("\nТекущие ингредиенты:");
                            manual.ShowIngredients();

                            Console.Write("Введите номер ингредиента для удвоения (или 'стоп' для завершения): ");
                            string input = Console.ReadLine();

                            if (input.ToLower() == "стоп" || input.ToLower() == "stop")
                            {
                                continueDoubling = false;
                            }
                            else if (int.TryParse(input, out int ingIndex))
                            {
                                manual.DoubleIngredient(ingIndex);
                                Console.WriteLine("Ингредиент удвоен!");
                            }
                            else
                            {
                                Console.WriteLine("Неверный ввод!");
                            }
                        }
                    }

                    order.AddPizza(manual);
                    Console.WriteLine("Пицца добавлена в заказ!");
                }
                else if (typeChoice == "3")
                {
                    Pizza combined = CreateCombinedPizza();
                    if (combined != null)
                    {
                        ShowSizeMenu();
                        int sizeIndex = int.Parse(Console.ReadLine());
                        combined.Size = (PizzaSize)sizeIndex;

                        order.AddPizza(combined);
                        Console.WriteLine("Комбинированная пицца добавлена в заказ!");
                    }
                }
                else if (typeChoice == "0")
                {
                    addMorePizzas = false;
                }

                if (order.GetPizzas().Count > 0 && addMorePizzas)
                {
                    Console.WriteLine("\nДобавить еще пиццу? (да/нет)");
                    string answer = Console.ReadLine().ToLower();
                    if (answer != "да" && answer != "yes" && answer != "д")
                        addMorePizzas = false;
                }
            }

            if (order.GetPizzas().Count > 0)
            {
                Console.Write("Комментарий к заказу: ");
                order.Comment = Console.ReadLine();

                Console.Write("Сделать заказ отложенным? (да/нет): ");
                string delayed = Console.ReadLine().ToLower();

                if (delayed == "да" || delayed == "yes" || delayed == "д")
                {
                    Console.Write("Введите дату и время (ДД.ММ.ГГГГ ЧЧ:ММ): ");
                    string dateInput = Console.ReadLine();

                    if (DateTime.TryParseExact(dateInput,
                        "dd.MM.yyyy HH:mm",
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out DateTime scheduled))
                    {
                        order.ScheduledTime = scheduled;
                    }
                }

                orders.Add(order);
                Console.WriteLine("\nЗаказ создан!");
            }
            else
            {
                Console.WriteLine("Заказ не создан (нет пицц)");
            }

            Console.WriteLine("\nНажмите Enter для продолжения...");
            Console.ReadLine();
        }

        static void OrderMenu()
        {
            Console.Clear();
            Console.WriteLine("1 - Создать заказ");
            Console.WriteLine("2 - Показать все заказы");
            Console.WriteLine("3 - Фильтр по ингредиенту");
            Console.WriteLine("4 - Фильтр по дате");
            Console.WriteLine("0 - Назад");

            string choice = Console.ReadLine();

            if (choice == "1")
            {
                CreateOrder();
            }
            else if (choice == "2")
            {
                ShowOrders();
            }
            else if (choice == "3")
            {
                FilterByIngredient();
            }
            else if (choice == "4")
            {
                FilterByDate();
            }
        }

        static void FilterByIngredient()
        {
            Console.Clear();
            Console.WriteLine("Поиск заказов по ингредиенту \n");

            if (!orders.Any())
            {
                Console.WriteLine("Нет заказов для поиска.");
                Console.ReadLine();
                return;
            }

            var allIngredients = orders
                .SelectMany(o => o.GetPizzas())
                .SelectMany(p => p.GetIngredients())
                .Select(i => i.Name)
                .Distinct()
                .OrderBy(name => name)
                .ToList();

            if (!allIngredients.Any())
            {
                Console.WriteLine("В заказах пока нет ингредиентов.");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Доступные ингредиенты:");
            foreach (var ing in allIngredients)
                Console.WriteLine("- " + ing);

            Console.Write("\nВведите название ингредиента: ");
            string? input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return;

            var filteredOrders = orders
                .Where(o => o.GetPizzas()
                    .Any(p => p.GetIngredients()
                        .Any(i => i.Name.Contains(input, StringComparison.OrdinalIgnoreCase))))
                .ToList();

            Console.Clear();

            if (filteredOrders.Any())
            {
                Console.WriteLine($"Найдено заказов: {filteredOrders.Count}\n");

                foreach (var order in filteredOrders)
                    order.Show();
            }
            else
            {
                Console.WriteLine("Заказы не найдены.");
            }

            Console.WriteLine("\nНажмите Enter для продолжения...");
            Console.ReadLine();
        }

        static void FilterByDate()
        {
            Console.Clear();
            Console.WriteLine("Поиск заказов по дате \n");
            Console.Write("Введите дату (ДД.ММ.ГГГГ): ");

            string? input = Console.ReadLine();

            if (DateTime.TryParseExact(input, "dd.MM.yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out DateTime filterDate))
            {
                var filteredOrders = orders
                    .Where(o => o.CreatedAt.Date == filterDate.Date ||
                                (o.ScheduledTime.HasValue &&
                                 o.ScheduledTime.Value.Date == filterDate.Date))
                    .ToList();

                Console.Clear();

                if (filteredOrders.Any())
                {
                    Console.WriteLine($"Найдено заказов: {filteredOrders.Count}\n");

                    foreach (var order in filteredOrders)
                        order.Show();
                }
                else
                {
                    Console.WriteLine("Заказы не найдены.");
                }
            }
            else
            {
                Console.WriteLine("Неверный формат даты.");
            }

            Console.WriteLine("\nНажмите Enter для продолжения...");
            Console.ReadLine();
        }

        static void IngredientMenu()
        {
            Console.Clear();
            Console.WriteLine("\n1 - Создать");
            Console.WriteLine("2 - Редактировать");
            Console.WriteLine("3 - Удалить");
            Console.WriteLine("4 - Показать список");
            Console.WriteLine("0 - Назад");

            string choice = Console.ReadLine();
            if (choice == "1")
            {
                Console.Clear();
                Console.Write("Название: ");
                string name = Console.ReadLine();

                Console.Write("Цена: ");
                if (decimal.TryParse(Console.ReadLine(), out decimal price))
                {
                    ingredients.Add(new Ingredient(name, price));
                    Console.WriteLine("Ингредиент добавлен!");
                }
                else
                {
                    Console.WriteLine("Неверная цена!");
                }
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            else if (choice == "2")
            {
                Console.Clear();
                ShowIngredients();
                Console.Write("Номер: ");
                if (int.TryParse(Console.ReadLine(), out int index) && index >= 0 && index < ingredients.Count)
                {
                    Console.Write("Новое название: ");
                    ingredients[index].Name = Console.ReadLine();

                    Console.Write("Новая цена: ");
                    if (decimal.TryParse(Console.ReadLine(), out decimal price))
                        ingredients[index].Price = price;
                }
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            else if (choice == "3")
            {
                Console.Clear();
                ShowIngredients();
                Console.Write("Номер: ");
                if (int.TryParse(Console.ReadLine(), out int index) && index >= 0 && index < ingredients.Count)
                {
                    ingredients.RemoveAt(index);
                    Console.WriteLine("Ингредиент удален!");
                }
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            else if (choice == "4")
            {
                Console.Clear();
                ShowIngredients();
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        static void ShowIngredients()
        {
            if (ingredients.Count == 0)
            {
                Console.WriteLine("Ингредиентов нет");
                return;
            }

            for (int i = 0; i < ingredients.Count; i++)
            {
                Console.Write(i + " - ");
                ingredients[i].Show();
            }
        }

        static void BaseMenu()
        {
            Console.Clear();
            Console.WriteLine("\n1 - Создать");
            Console.WriteLine("2 - Редактировать");
            Console.WriteLine("3 - Удалить");
            Console.WriteLine("4 - Показать список");
            Console.WriteLine("0 - Назад");

            string choice = Console.ReadLine();

            if (choice == "1")
            {
                Console.Clear();
                Console.Write("Название: ");
                string name = Console.ReadLine();

                Console.Write("Цена: ");
                if (decimal.TryParse(Console.ReadLine(), out decimal price))
                {
                    bool isClassic = bases.Count == 0;

                    if (!isClassic && bases.Count > 0)
                    {
                        decimal classicPrice = bases[0].Price;

                        if (price > classicPrice * 1.2m)
                        {
                            Console.WriteLine("Цена превышает 20% от классической!");
                            Console.WriteLine("\nНажмите Enter для продолжения...");
                            Console.ReadLine();
                            return;
                        }
                    }

                    bases.Add(new PizzaBase(name, price, isClassic));
                    Console.WriteLine("Основа добавлена!");
                }
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            else if (choice == "2")
            {
                Console.Clear();
                ShowBases();
                Console.Write("Номер: ");
                if (int.TryParse(Console.ReadLine(), out int index) && index >= 0 && index < bases.Count)
                {
                    Console.Write("Новое название: ");
                    bases[index].Name = Console.ReadLine();

                    Console.Write("Новая цена: ");
                    if (decimal.TryParse(Console.ReadLine(), out decimal newPrice))
                    {
                        if (!bases[index].IsClassic && bases.Count > 0)
                        {
                            decimal classicPrice = bases[0].Price;

                            if (newPrice > classicPrice * 1.2m)
                            {
                                Console.WriteLine("Цена превышает 20%!");
                                Console.WriteLine("\nНажмите Enter для продолжения...");
                                Console.ReadLine();
                                return;
                            }
                        }

                        bases[index].Price = newPrice;
                        Console.WriteLine("Основа обновлена!");
                    }
                }
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            else if (choice == "3")
            {
                Console.Clear();
                ShowBases();
                Console.Write("Номер: ");
                if (int.TryParse(Console.ReadLine(), out int index) && index >= 0 && index < bases.Count)
                {
                    bases.RemoveAt(index);
                    Console.WriteLine("Основа удалена!");
                }
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            else if (choice == "4")
            {
                Console.Clear();
                ShowBases();
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        static void ShowBases()
        {
            if (bases.Count == 0)
            {
                Console.WriteLine("Основ нет");
                return;
            }

            for (int i = 0; i < bases.Count; i++)
            {
                Console.Write(i + " - ");
                bases[i].Show();
            }
        }

        static void BorderMenu()
        {
            Console.Clear();
            Console.WriteLine("\n=== МЕНЮ БОРТИКОВ ===");
            Console.WriteLine("1 - Создать бортик");
            Console.WriteLine("2 - Редактировать бортик");
            Console.WriteLine("3 - Удалить бортик");
            Console.WriteLine("4 - Показать все бортики");
            Console.WriteLine("5 - Управление доступом бортика к пиццам");
            Console.WriteLine("6 - Добавить ингредиент в бортик");
            Console.WriteLine("7 - Удалить ингредиент из бортика");
            Console.WriteLine("0 - Назад");

            string choice = Console.ReadLine();

            if (choice == "1")
            {
                CreateBorder();
            }
            else if (choice == "2")
            {
                EditBorder();
            }
            else if (choice == "3")
            {
                DeleteBorder();
            }
            else if (choice == "4")
            {
                ShowBorders();
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            else if (choice == "5")
            {
                ManageBorderAccess();
            }
            else if (choice == "6")
            {
                AddIngredientToBorder();
            }
            else if (choice == "7")
            {
                RemoveIngredientFromBorder();
            }
        }

        static void CreateBorder()
        {
            Console.Clear();
            Console.WriteLine("Создание нового бортика\n");

            Console.Write("Название бортика: ");
            string name = Console.ReadLine();

            Console.Write("Цена бортика: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal price))
            {
                Console.WriteLine("Неверная цена!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Выберите тип доступа:");
            Console.WriteLine("0 - Разрешенные пиццы");
            Console.WriteLine("1 - Запрещенные пиццы");

            if (!int.TryParse(Console.ReadLine(), out int accessTypeIndex) || (accessTypeIndex != 0 && accessTypeIndex != 1))
            {
                Console.WriteLine("Неверный выбор!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
                return;
            }

            Border.BorderAccessType accessType = accessTypeIndex == 0 ?
                Border.BorderAccessType.Разрешенные :
                Border.BorderAccessType.Запрещенные;

            Border newBorder = new Border(name, price, accessType);
            borders.Add(newBorder);

            Console.WriteLine($"Бортик '{name}' создан!");
            Console.WriteLine("\nНажмите Enter для продолжения...");
            Console.ReadLine();
        }

        static void EditBorder()
        {
            Console.Clear();
            if (borders.Count == 0)
            {
                Console.WriteLine("Нет бортиков для редактирования");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
                return;
            }

            ShowBorders();
            Console.Write("Выберите номер бортика для редактирования: ");

            if (!int.TryParse(Console.ReadLine(), out int index) || index < 0 || index >= borders.Count)
            {
                Console.WriteLine("Неверный номер!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
                return;
            }

            Border border = borders[index];

            Console.Clear();
            Console.WriteLine($"\nРедактирование бортика '{border.Name}':");
            Console.WriteLine("1 - Изменить название");
            Console.WriteLine("2 - Изменить цену");
            Console.WriteLine("3 - Изменить тип доступа");

            string choice = Console.ReadLine();

            if (choice == "1")
            {
                Console.Write("Новое название: ");
                border.Name = Console.ReadLine();
                Console.WriteLine("Название изменено!");
            }
            else if (choice == "2")
            {
                Console.Write("Новая цена: ");
                if (decimal.TryParse(Console.ReadLine(), out decimal newPrice))
                {
                    border.Price = newPrice;
                    Console.WriteLine("Цена изменена!");
                }
            }
            else if (choice == "3")
            {
                Console.WriteLine("Выберите новый тип доступа:");
                Console.WriteLine("0 - Разрешенные пиццы");
                Console.WriteLine("1 - Запрещенные пиццы");

                if (int.TryParse(Console.ReadLine(), out int accessTypeIndex) && (accessTypeIndex == 0 || accessTypeIndex == 1))
                {
                    Border.BorderAccessType newAccessType = accessTypeIndex == 0 ?
                        Border.BorderAccessType.Разрешенные :
                        Border.BorderAccessType.Запрещенные;

                    border.PizzaIds.Clear();
                    border.AccessType = newAccessType;
                    Console.WriteLine("Тип доступа изменен! Список пицц очищен.");
                }
            }

            Console.WriteLine("\nНажмите Enter для продолжения...");
            Console.ReadLine();
        }

        static void DeleteBorder()
        {
            Console.Clear();
            if (borders.Count == 0)
            {
                Console.WriteLine("Нет бортиков для удаления");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
                return;
            }

            ShowBorders();
            Console.Write("Выберите номер бортика для удаления: ");

            if (!int.TryParse(Console.ReadLine(), out int index) || index < 0 || index >= borders.Count)
            {
                Console.WriteLine("Неверный номер!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
                return;
            }

            bool isUsed = pizzas.Any(p => p.Border == borders[index]);
            bool isUsedInCombined = pizzas.OfType<CombinedPizza>().Any(p => p.CombinedBorder?.FirstHalfBorder == borders[index] || p.CombinedBorder?.SecondHalfBorder == borders[index]);

            if (isUsed || isUsedInCombined)
            {
                Console.WriteLine("Этот бортик используется в пиццах! Сначала удалите его из пицц.");
            }
            else
            {
                borders.RemoveAt(index);
                Console.WriteLine("Бортик удален!");
            }

            Console.WriteLine("\nНажмите Enter для продолжения...");
            Console.ReadLine();
        }

        static void ShowBorders()
        {
            if (borders.Count == 0)
            {
                Console.WriteLine("Бортиков нет");
                return;
            }

            for (int i = 0; i < borders.Count; i++)
            {
                Console.WriteLine($"\n--- Бортик №{i} ---");
                borders[i].Show();
            }
        }

        static void ManageBorderAccess()
        {
            Console.Clear();
            if (borders.Count == 0)
            {
                Console.WriteLine("Сначала создайте бортики!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
                return;
            }

            if (pizzas.Count == 0)
            {
                Console.WriteLine("Сначала создайте пиццы!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
                return;
            }

            ShowBorders();
            Console.Write("Выберите номер бортика: ");

            if (!int.TryParse(Console.ReadLine(), out int borderIndex) || borderIndex < 0 || borderIndex >= borders.Count)
            {
                Console.WriteLine("Неверный номер!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
                return;
            }

            Border border = borders[borderIndex];

            Console.Clear();
            Console.WriteLine($"Управление доступом для бортика '{border.Name}'");
            Console.WriteLine($"Текущий тип доступа: {border.AccessType}");

            Console.WriteLine("\n1 - Добавить пиццу в список");
            Console.WriteLine("2 - Удалить пиццу из списка");
            Console.WriteLine("3 - Показать текущий список пицц");
            Console.WriteLine("0 - Назад");

            string choice = Console.ReadLine();

            if (choice == "1")
            {
                Console.Clear();
                Console.WriteLine("Доступные пиццы:");
                List<int> availablePizzas = new List<int>();

                for (int i = 0; i < pizzas.Count; i++)
                {
                    if (!border.PizzaIds.Contains(pizzas[i].Id))
                    {
                        Console.WriteLine($"{i} - {pizzas[i].Name}");
                        availablePizzas.Add(i);
                    }
                }

                if (availablePizzas.Count > 0)
                {
                    Console.Write("Выберите номер пиццы для добавления: ");
                    if (int.TryParse(Console.ReadLine(), out int pizzaIndex) && pizzaIndex >= 0 && pizzaIndex < pizzas.Count)
                    {
                        border.AddPizzaId(pizzas[pizzaIndex].Id);
                        Console.WriteLine("Пицца добавлена в список!");
                    }
                }
                else
                {
                    Console.WriteLine("Все пиццы уже в списке!");
                }
            }
            else if (choice == "2")
            {
                Console.Clear();
                border.ShowPizzaAccess(pizzas);

                Console.Write("\nВведите название пиццы для удаления из списка: ");
                string pizzaName = Console.ReadLine();

                var pizzaToRemove = pizzas.FirstOrDefault(p => p.Name.Equals(pizzaName, StringComparison.OrdinalIgnoreCase));

                if (pizzaToRemove != null && border.PizzaIds.Contains(pizzaToRemove.Id))
                {
                    border.RemovePizzaId(pizzaToRemove.Id);
                    Console.WriteLine("Пицца удалена из списка!");
                }
                else
                {
                    Console.WriteLine("Пицца не найдена в списке!");
                }
            }
            else if (choice == "3")
            {
                Console.Clear();
                border.ShowPizzaAccess(pizzas);
            }

            Console.WriteLine("\nНажмите Enter для продолжения...");
            Console.ReadLine();
        }

        static void AddIngredientToBorder()
        {
            Console.Clear();
            if (borders.Count == 0)
            {
                Console.WriteLine("Сначала создайте бортики!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
                return;
            }

            if (ingredients.Count == 0)
            {
                Console.WriteLine("Сначала создайте ингредиенты!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
                return;
            }

            ShowBorders();
            Console.Write("Выберите номер бортика: ");

            if (!int.TryParse(Console.ReadLine(), out int borderIndex) || borderIndex < 0 || borderIndex >= borders.Count)
            {
                Console.WriteLine("Неверный номер!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
                return;
            }

            Border border = borders[borderIndex];

            Console.Clear();
            Console.WriteLine($"Добавление ингредиента в бортик '{border.Name}'");
            ShowIngredients();

            Console.Write("Выберите номер ингредиента для добавления: ");
            if (int.TryParse(Console.ReadLine(), out int ingIndex) && ingIndex >= 0 && ingIndex < ingredients.Count)
            {
                border.AddIngredient(ingredients[ingIndex]);
                Console.WriteLine("Ингредиент добавлен в бортик!");
            }

            Console.WriteLine("\nНажмите Enter для продолжения...");
            Console.ReadLine();
        }

        static void RemoveIngredientFromBorder()
        {
            Console.Clear();
            if (borders.Count == 0)
            {
                Console.WriteLine("Сначала создайте бортики!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
                return;
            }

            ShowBorders();
            Console.Write("Выберите номер бортика: ");

            if (!int.TryParse(Console.ReadLine(), out int borderIndex) || borderIndex < 0 || borderIndex >= borders.Count)
            {
                Console.WriteLine("Неверный номер!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
                return;
            }

            Border border = borders[borderIndex];

            if (border.GetIngredients().Count == 0)
            {
                Console.WriteLine("В этом бортике нет ингредиентов!");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
                return;
            }

            Console.Clear();
            Console.WriteLine($"Удаление ингредиента из бортика '{border.Name}'");

            var borderIngredients = border.GetIngredients();
            for (int i = 0; i < borderIngredients.Count; i++)
            {
                Console.WriteLine($"{i} - {borderIngredients[i].Name}");
            }

            Console.Write("Выберите номер ингредиента для удаления: ");
            if (int.TryParse(Console.ReadLine(), out int ingIndex) && ingIndex >= 0 && ingIndex < borderIngredients.Count)
            {
                border.RemoveIngredient(ingIndex);
                Console.WriteLine("Ингредиент удален из бортика!");
            }

            Console.WriteLine("\nНажмите Enter для продолжения...");
            Console.ReadLine();
        }

        static void PizzaMenu()
        {
            Console.Clear();
            Console.WriteLine("\n1 - Создать");
            Console.WriteLine("2 - Редактировать");
            Console.WriteLine("3 - Удалить");
            Console.WriteLine("4 - Показать список");
            Console.WriteLine("0 - Назад");

            string choice = Console.ReadLine();

            if (choice == "1")
            {
                Console.Clear();
                if (bases.Count == 0)
                {
                    Console.WriteLine("Сначала создайте основу!");
                    Console.WriteLine("\nНажмите Enter для продолжения...");
                    Console.ReadLine();
                    return;
                }

                Console.Write("Название: ");
                string name = Console.ReadLine();

                ShowBases();
                Console.Write("Выберите основу: ");
                if (int.TryParse(Console.ReadLine(), out int baseIndex) && baseIndex >= 0 && baseIndex < bases.Count)
                {
                    Pizza pizza = new Pizza(name, bases[baseIndex]);

                    if (ingredients.Count > 0)
                    {
                        ShowIngredients();
                        Console.WriteLine("Введите номера ингредиентов через пробел (или оставьте пустым):");
                        string input = Console.ReadLine();

                        if (!string.IsNullOrWhiteSpace(input))
                        {
                            string[] nums = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                            foreach (var n in nums)
                            {
                                if (int.TryParse(n, out int index) && index >= 0 && index < ingredients.Count)
                                    pizza.AddIngredient(ingredients[index]);
                            }
                        }
                    }

                    pizzas.Add(pizza);
                    Console.WriteLine("Пицца создана!");
                }
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            else if (choice == "2")
            {
                Console.Clear();
                ShowPizzas();
                Console.Write("Номер пиццы: ");
                if (int.TryParse(Console.ReadLine(), out int index) && index >= 0 && index < pizzas.Count)
                {
                    Pizza pizza = pizzas[index];

                    Console.Clear();
                    Console.WriteLine($"Редактирование пиццы '{pizza.Name}':");
                    Console.WriteLine("1 - Изменить название");
                    Console.WriteLine("2 - Добавить ингредиент");
                    Console.WriteLine("3 - Удалить ингредиент");
                    Console.WriteLine("4 - Добавить бортик");
                    Console.WriteLine("5 - Удалить бортик");

                    string editChoice = Console.ReadLine();

                    if (editChoice == "1")
                    {
                        Console.Write("Новое название: ");
                        pizza.Name = Console.ReadLine();
                        Console.WriteLine("Название изменено!");
                    }
                    else if (editChoice == "2")
                    {
                        if (ingredients.Count > 0)
                        {
                            ShowIngredients();
                            Console.Write("Введите номер ингредиента для добавления: ");
                            if (int.TryParse(Console.ReadLine(), out int ingIndex) && ingIndex >= 0 && ingIndex < ingredients.Count)
                            {
                                pizza.AddIngredient(ingredients[ingIndex]);
                                Console.WriteLine("Ингредиент добавлен!");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Нет ингредиентов для добавления");
                        }
                    }
                    else if (editChoice == "3")
                    {
                        pizza.ShowIngredients();
                        Console.Write("Введите номер ингредиента для удаления: ");
                        if (int.TryParse(Console.ReadLine(), out int ingIndex))
                        {
                            pizza.RemoveIngredient(ingIndex);
                            Console.WriteLine("Ингредиент удален!");
                        }
                    }
                    else if (editChoice == "4")
                    {
                        if (borders.Count > 0)
                        {
                            Console.WriteLine("Доступные бортики:");
                            for (int i = 0; i < borders.Count; i++)
                            {
                                if (borders[i].IsAllowedForPizza(pizza.Id))
                                {
                                    Console.WriteLine($"{i} - {borders[i].Name}");
                                }
                            }

                            Console.Write("Выберите номер бортика: ");
                            if (int.TryParse(Console.ReadLine(), out int borderIndex) && borderIndex >= 0 && borderIndex < borders.Count)
                            {
                                if (borders[borderIndex].IsAllowedForPizza(pizza.Id))
                                {
                                    pizza.Border = borders[borderIndex];
                                    Console.WriteLine("Бортик добавлен!");
                                }
                                else
                                {
                                    Console.WriteLine("Этот бортик нельзя использовать с данной пиццей!");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine("Нет доступных бортиков");
                        }
                    }
                    else if (editChoice == "5")
                    {
                        if (pizza.Border != null)
                        {
                            pizza.Border = null;
                            Console.WriteLine("Бортик удален!");
                        }
                        else
                        {
                            Console.WriteLine("У этой пиццы нет бортика!");
                        }
                    }
                }
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            else if (choice == "3")
            {
                Console.Clear();
                ShowPizzas();
                Console.Write("Номер: ");
                if (int.TryParse(Console.ReadLine(), out int index) && index >= 0 && index < pizzas.Count)
                {
                    pizzas.RemoveAt(index);
                    Console.WriteLine("Пицца удалена!");
                }
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
            else if (choice == "4")
            {
                Console.Clear();
                ShowPizzas();
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
            }
        }

        static void ShowPizzas()
        {
            if (pizzas.Count == 0)
            {
                Console.WriteLine("Пицц нет");
                return;
            }

            for (int i = 0; i < pizzas.Count; i++)
            {
                Console.WriteLine($"\n=== Пицца №{i} ===");
                pizzas[i].Show();
            }
        }

        static void ShowOrders()
        {
            Console.Clear();

            if (orders.Count == 0)
            {
                Console.WriteLine("Заказов нет");
                Console.WriteLine("\nНажмите Enter для продолжения...");
                Console.ReadLine();
                return;
            }

            orders.Sort(new OrderPriceComparer());

            Console.WriteLine($"Всего заказов: {orders.Count}\n");
            foreach (var order in orders)
            {
                order.Show();
            }

            Console.WriteLine("\nНажмите Enter для продолжения...");
            Console.ReadLine();
        }
    }
}
