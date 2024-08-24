using InventoryManagement.Domain.Models;
using InventoryManagement.EntityFramework.Database;
using InventoryManagement.WPF.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace InventoryManagement.WPF.ViewModels
{
    public class AddToWarehouseViewModel : ViewModelBase
    {
        private readonly InventoryManagementDbContext _context;
        private readonly System.Windows.Window _window;
        private Order _selectedOrder; 
        public event Action OnOrderAdded;
        public ICommand AddToWarehouseCommand { get; }
        public ObservableCollection<WarehouseLocation> WarehouseLocations { get; set; }
        private WarehouseLocation _selectedWarehouseLocation;
        
        public AddToWarehouseViewModel(Order selectedOrder, InventoryManagementDbContext context, System.Windows.Window window)
        {
            _context = context;
            _window = window;
            _selectedOrder = selectedOrder;

            WarehouseLocations = new ObservableCollection<WarehouseLocation>(_context.WarehouseLocations.Include(wl => wl.Warehouse).ToList());
            AddToWarehouseCommand = new RelayCommand(AddToWarehouse);
        }

        public WarehouseLocation SelectedWarehouseLocation
        {
            get => _selectedWarehouseLocation;
            set
            {
                _selectedWarehouseLocation = value;
                OnPropertyChanged(nameof(SelectedWarehouseLocation));
            }
        }
        
        private void AddToWarehouse()
        {
            if (SelectedWarehouseLocation == null)
            {
                MessageBox.Show("Please select a warehouse location.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            foreach (var orderItem in _selectedOrder.OrderItems)
            {
                var existingProduct = _context.Products.Find(orderItem.Product.Id);
                if (existingProduct == null)
                {
                    MessageBox.Show($"Product {orderItem.Product.Name} not found in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var existingSupplier = _context.Suppliers.Find(_selectedOrder.Supplier.Id);
                if (existingSupplier == null)
                {
                    MessageBox.Show($"Supplier {_selectedOrder.Supplier.DisplayName} not found in the database.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var existingWarehouseProduct = _context.WarehouseProducts.FirstOrDefault(wp => wp.Product.Id == existingProduct.Id && wp.WarehouseLocation.Id == SelectedWarehouseLocation.Id);

                if (existingWarehouseProduct != null)
                {
                    MessageBox.Show($"Product {orderItem.Product.Name} already exists in the selected warehouse.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var warehouseProduct = new WarehouseProduct
                {
                    Quantity = orderItem.Quantity,
                    WarehouseLocation = SelectedWarehouseLocation,
                    Product = existingProduct,
                    Supplier = existingSupplier
                };

                _context.WarehouseProducts.Add(warehouseProduct);
             
            }

            _selectedOrder.Status = "Added to warehouse";
            _context.SaveChanges();
            MessageBox.Show("Order added to warehouse and status updated.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            OnOrderAdded?.Invoke();
            _window?.Close();

        }
    }
}