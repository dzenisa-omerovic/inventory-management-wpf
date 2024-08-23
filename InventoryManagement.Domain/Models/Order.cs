using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Domain.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
        public Supplier Supplier { get; set; }
        public int SupplierId { get; set; }
        public IEnumerable<OrderItem> OrderItems { get; set; }
        public string Status { get; set; }
        
    }
    
}
