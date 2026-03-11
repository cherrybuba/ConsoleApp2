using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PizzaSystem
{
    class OrderPriceComparer : IComparer<Order>
    {
        public int Compare(Order x, Order y)
        {
            return x.GetTotalPrice().CompareTo(y.GetTotalPrice());
        }
    }
}
