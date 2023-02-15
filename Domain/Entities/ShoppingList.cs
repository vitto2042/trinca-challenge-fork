using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class ShoppingList : ValueObject
    {
        public ShoppingList()
        {
        }

        public ShoppingList(
            float vegetables,
            float meat)
        {
            Vegetables = vegetables;
            Meat = meat;
        }

        public float Vegetables { get; private set; } = 0.0f;
        public float Meat { get; private set; } = 0.0f;

        public ShoppingList Add(float vegs, float meat)
        {
            return new ShoppingList(Vegetables + vegs, Meat + meat);
        }

        public ShoppingList Sub(float vegs, float meat)
        {
            return new ShoppingList(Vegetables - vegs, Meat - meat);
        }

        public object TakeSnapShot()
        {
            return new
            {
                Vegatebles = Vegetables.ToString("0.0"),
                Meat = Meat.ToString("0.0")
            };
        }
    }
}
