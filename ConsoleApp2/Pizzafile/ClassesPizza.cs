using System;
using System.Collections.Generic;
using System.Linq;

namespace PizzaSystem
{
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

}
