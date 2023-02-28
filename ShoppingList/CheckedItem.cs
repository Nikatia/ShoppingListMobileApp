using System;
using System.Collections.Generic;
using System.Text;

namespace ShoppingList
{
    internal class CheckedItem
    {
        public int Position { get; set; }
        public bool IsChecked { get; set; }
        public string ProductName { get; set; }
    }
}
