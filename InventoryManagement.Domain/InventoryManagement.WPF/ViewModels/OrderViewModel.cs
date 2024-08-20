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
namespace InventoryManagement.WPF.ViewModels
{
    public class OrderViewModel : ViewModelBase
    {
        private readonly InventoryManagementDbContext _context;
        private Order _selectedOrder;
        private string _orderName;
        private Supplier _selectedSupplier;
        private ObservableCollection<OrderItem> _selectedOrderItems;
        public ICommand AddOrderCommand { get; }
        public ICommand UpdateOrderCommand { get; }
        public ICommand DeleteOrderCommand { get; }
        public ICommand TryAddOrderCommand { get; }
        public ICommand TryUpdateOrderCommand { get; }
        public ICommand TryDeleteOrderCommand { get; }
        public ICommand ShowOrderItemsCommand { get; }
        public ObservableCollection<Order> Orders { get; set; } = new ObservableCollection<Order>();
        private ObservableCollection<Supplier> _suppliers { get; set; }

        public OrderViewModel()
        {
            _context = new InventoryManagementDbContext();
            Load();

            AddOrderCommand = new RelayCommand(async () => await AddOrderAsync(), CanAddOrder);
            UpdateOrderCommand = new RelayCommand(async () => await UpdateOrderAsync(), CanUpdateOrder);
            DeleteOrderCommand = new RelayCommand(async () => await DeleteOrderAsync(), CanDeleteOrder);

            TryAddOrderCommand = new RelayCommand(async () => await TryAddOrderAsync());
            TryUpdateOrderCommand = new RelayCommand(async () => await TryUpdateOrderAsync());
            TryDeleteOrderCommand = new RelayCommand(async () => await TryDeleteOrderAsync());
            ShowOrderItemsCommand = new RelayCommand(async () => await ShowOrderItemsAsync());
        }

        public async void Load()
        {
            await LoadOrdersAsync();
        }
        public ObservableCollection<OrderItem> SelectedOrderItems
        {
            get => _selectedOrderItems;
            set
            {
                _selectedOrderItems = value;
                OnPropertyChanged(nameof(SelectedOrderItems));
            }
        }

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                if (_selectedOrder != null)
                {
                    OrderName = _selectedOrder.Name;
                    SelectedSupplier = _selectedOrder.Supplier;
                }
                OnPropertyChanged(nameof(SelectedOrder));
                ((RelayCommand)UpdateOrderCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteOrderCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ShowOrderItemsCommand).RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(IsUpdateEnabled));
            }
        }
        private async Task ShowOrderItemsAsync()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Please select an order to view its order items.", "No Order Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var orderItems = await _context.OrderItems.Where(oi => oi.OrderId == SelectedOrder.Id).Include(oi => oi.Product).Include(oi => oi.Order).ToListAsync();

            if (orderItems.Count == 0)
            {
                MessageBox.Show("The selected order has no order items.", "No Order Items Found", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            SelectedOrderItems = new ObservableCollection<OrderItem>(orderItems);
        }

        public string OrderName
        {
            get => _orderName;
            set
            {
                _orderName = value;
                OnPropertyChanged(nameof(OrderName));
                ((RelayCommand)AddOrderCommand).RaiseCanExecuteChanged();
                ((RelayCommand)UpdateOrderCommand).RaiseCanExecuteChanged();
            }
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
        
        public bool IsUpdateEnabled => SelectedOrder != null;

        private async Task LoadOrdersAsync()
        {
            var orders = await _context.Orders.Include(o => o.Supplier).ToListAsync();
            Orders.Clear();
            foreach (var order in orders)
            {
                Orders.Add(order);
            }
        }
        public ObservableCollection<Supplier> Suppliers
        {
            get
            {
                var suppliers = _context.Suppliers.ToList();
                return new ObservableCollection<Supplier>(suppliers);
            }
        }

        private bool CanAddOrder()
        {
            return !string.IsNullOrWhiteSpace(OrderName) && SelectedSupplier != null;
        }

        private async Task AddOrderAsync()
        {
            if (Orders.Any(p => p.Name == OrderName))
            {
                MessageBox.Show("An order with the same name already exists.", "Duplicate Order", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var order = new Order
            {
                Name = OrderName,
                Date = DateTime.Now,
                SupplierId = SelectedSupplier.Id
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            Orders.Add(order);

            OrderName = string.Empty;
            MessageBox.Show("Order added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task TryAddOrderAsync()
        {
            if (!CanAddOrder())
            {
                MessageBox.Show("You can't add order. Check if the order name is empty or a supplier is selected.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await AddOrderAsync();
        }

        private bool CanUpdateOrder()
        {
            return SelectedOrder != null;
        }

        private async Task UpdateOrderAsync()
        {
            if (SelectedOrder == null)
            {
                return;
            }

            if (MessageBox.Show("Are you sure that you want to update this order?", "Confirm Update", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            SelectedOrder.Name = OrderName;
            SelectedOrder.SupplierId = SelectedSupplier.Id;

            _context.Orders.Update(SelectedOrder);
            await _context.SaveChangesAsync();
            await LoadOrdersAsync();
            MessageBox.Show("Order updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task TryUpdateOrderAsync()
        {
            if (!CanUpdateOrder())
            {
                MessageBox.Show("You can't update order. Check if the order is selected from the list.", "No Order Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await UpdateOrderAsync();
        }

        private bool CanDeleteOrder()
        {
            return SelectedOrder != null;
        }

        private async Task DeleteOrderAsync()
        {
            if (SelectedOrder == null)
            {
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this order?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            _context.Orders.Remove(SelectedOrder);
            await _context.SaveChangesAsync();
            Orders.Remove(SelectedOrder);

            OrderName = string.Empty;
            MessageBox.Show("Order deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task TryDeleteOrderAsync()
        {
            if (!CanDeleteOrder())
            {
                MessageBox.Show("You can't delete order. Check if the order is selected from the list.", "No Order Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await DeleteOrderAsync();
        }
    }
}
