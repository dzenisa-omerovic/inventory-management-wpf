using InventoryManagement.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.WPF.ViewModels
{
    public class ReportWarehouseProductViewModel
    {
        public string ProductName { get; }
        public string Quantity { get; }
        public string WarehouseLocation { get; }
        public string Warehouse { get; }
        public string Supplier { get; }

        public ReportWarehouseProductViewModel(WarehouseProduct warehouseProduct)
        {
            ProductName = warehouseProduct.Product.Name;
            Quantity = warehouseProduct.Quantity.ToString();
            WarehouseLocation = warehouseProduct.WarehouseLocation.Name;
            Warehouse = warehouseProduct.WarehouseLocation.Warehouse.Name;
            Supplier = warehouseProduct.Supplier.DisplayName;
        }
    }
}
