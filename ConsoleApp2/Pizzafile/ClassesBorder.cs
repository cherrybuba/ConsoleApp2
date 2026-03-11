using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PizzaSystem
{
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

}
