using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class ShoppingList : ValueObject
    {
        public ShoppingList(
            float vegetables,
            float meat)
        {
            Vegetables = vegetables;
            Meat = meat;
        }

        public float Vegetables { get; private set; } = 0.0f;
        public float Meat { get; private set; } = 0.0f;

        public object TakeSnapShot()
        {
            return new
            {
                Vegetables,
                Meat
            };
        }
    }
}
