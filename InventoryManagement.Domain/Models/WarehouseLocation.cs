using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.Domain.Models
{
    public class WarehouseLocation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public Warehouse Warehouse { get; set; }
        public int WarehouseId { get; set; }
    }
}
