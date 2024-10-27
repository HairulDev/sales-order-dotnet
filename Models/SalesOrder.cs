using System;
using System.Collections.Generic;

namespace CrudApp.Models
{
    public class SalesOrder
    {
        public string? Id_Order { get; set; }
        public string Number_Order { get; set; }
        public DateTime Date { get; set; }
        public string Customer { get; set; }
        public string Address { get; set; }
        public List<ItemOrder> Items { get; set; } = new List<ItemOrder>();
    }
}
