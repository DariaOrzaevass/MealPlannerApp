using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    internal class Ingredient
    {
        string ingredientName;
        double ingredientAmount;

        public Ingredient(string name, double amount)
        {
            ingredientName = name;
            ingredientAmount = amount;
        }
    }
}
