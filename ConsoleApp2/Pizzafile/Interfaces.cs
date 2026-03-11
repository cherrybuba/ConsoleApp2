using PizzaSystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace PizzaSystem
{
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
}
