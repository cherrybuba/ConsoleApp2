using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PizzaSystem
{
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

}
