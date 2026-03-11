using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PizzaSystem
{
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

}
