using System;
using System.Collections.Generic;
using System.Text;

namespace ShoppingList.Models
{
    internal class Purchase
    {
        public int PurchaseId { get; set; }

        public int ProductId { get; set; }

        public DateTime Date { get; set; }

        public int Amount { get; set; }
    }
}
