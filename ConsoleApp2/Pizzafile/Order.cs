using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PizzaSystem
{
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


}
