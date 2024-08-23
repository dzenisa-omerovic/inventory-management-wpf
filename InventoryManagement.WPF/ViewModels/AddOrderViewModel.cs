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

namespace InventoryManagement.WPF.ViewModels
{
    public class AddOrderViewModel : ViewModelBase
    {
        private readonly InventoryManagementDbContext _context;

        private string _orderName;
        private Supplier _selectedSupplier;
        private Product _selectedProduct;
        private int _quantity;

        public ICommand AddOrderItemCommand { get; }
        public ICommand SaveOrderCommand { get; }

        public ObservableCollection<Supplier> Suppliers { get; set; }
        public ObservableCollection<Product> Products { get; set; }
        public ObservableCollection<OrderItem> OrderItems { get; set; }

        public event Action OnOrderSaved;
        private OrderItem _selectedOrderItem;

        public ICommand RemoveOrderItemCommand { get; }

        public OrderItem SelectedOrderItem
        {
            get => _selectedOrderItem;
            set
            {
                _selectedOrderItem = value;
                OnPropertyChanged(nameof(SelectedOrderItem));
            }
        }
        public AddOrderViewModel()
        {
            _context = new InventoryManagementDbContext();
            Suppliers = new ObservableCollection<Supplier>();
            Products = new ObservableCollection<Product>();
            OrderItems = new ObservableCollection<OrderItem>();

            LoadSuppliers();
            LoadProducts();

            AddOrderItemCommand = new RelayCommand(AddOrderItem);
            SaveOrderCommand = new RelayCommand(async () => await SaveOrderAsync());
            RemoveOrderItemCommand = new RelayCommand(RemoveOrderItem);
        }

        public string OrderName
        {
            get => _orderName;
            set
            {
                _orderName = value;
                OnPropertyChanged(nameof(OrderName));
            }
        }
        private void RemoveOrderItem()
        {
            if (OrderItems.Count == 0)
            {
                MessageBox.Show("There are no order items to remove.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SelectedOrderItem == null)
            {
                MessageBox.Show("Please select an order item to remove.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            OrderItems.Remove(SelectedOrderItem);
            MessageBox.Show("Order item removed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public Supplier SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                _selectedSupplier = value;
                OnPropertyChanged(nameof(SelectedSupplier));
            }
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                OnPropertyChanged(nameof(SelectedProduct));
            }
        }

        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }

        private void LoadSuppliers()
        {
            var suppliers = _context.Suppliers.ToList();
            Suppliers.Clear();
            foreach (var supplier in suppliers)
            {
                Suppliers.Add(supplier);
            }
        }
        
        private void AddOrderItem()
        {
            if(SelectedProduct == null || Quantity <= 0)
            {
                MessageBox.Show("Please select a product and enter a valid quantity.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            OrderItems.Add(new OrderItem
            {
                Product = SelectedProduct,
                Quantity = Quantity
            });

            Quantity = 0;

        }


        private void LoadProducts()
        {
            var products = _context.Products.ToList();
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
        }

        private async Task SaveOrderAsync()
        {
            if (string.IsNullOrEmpty(OrderName) || SelectedSupplier == null || !OrderItems.Any())
            {
                MessageBox.Show("Please ensure all fields are filled correctly and that at least one order item is added.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var newOrder = new Order
            {
                Name = OrderName,
                Supplier = SelectedSupplier,
                Date = DateTime.Now,
                OrderItems = OrderItems.ToList(),
                Status = "Pending"
            };

            _context.Orders.Add(newOrder);

            await _context.SaveChangesAsync();
            MessageBox.Show("Order saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            OnOrderSaved?.Invoke();
           
            OrderName = string.Empty;
            SelectedSupplier = null;
            OrderItems.Clear();
        }
        
    }

}

