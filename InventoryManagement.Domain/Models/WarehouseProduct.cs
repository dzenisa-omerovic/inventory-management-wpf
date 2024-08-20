using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Domain.Models
{
    public class WarehouseProduct
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public Product Product { get; set; }
        public int ProductId { get; set; }
        public WarehouseLocation WarehouseLocation { get; set; }
        public int WarehouseLocationId { get; set; }
        public Supplier Supplier { get; set; }
        public int SupplierId { get; set; }
    }
}
