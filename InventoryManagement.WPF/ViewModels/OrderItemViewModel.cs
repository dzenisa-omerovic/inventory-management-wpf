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
    public class OrderItemViewModel : ViewModelBase
    {
        private readonly InventoryManagementDbContext _context;
        private OrderItem _selectedOrderItem;
        private int _orderItemQuantity;
        private Product _selectedProduct;
        private Order _selectedOrder;
        public ICommand AddOrderItemCommand { get; }
        public ICommand UpdateOrderItemCommand { get; }
        public ICommand DeleteOrderItemCommand { get; }
        public ICommand TryAddOrderItemCommand { get; }
        public ICommand TryUpdateOrderItemCommand { get; }
        public ICommand TryDeleteOrderItemCommand { get; }
        private ObservableCollection<Product> _products { get; set; }
        public ObservableCollection<OrderItem> OrderItems { get; set; } = new ObservableCollection<OrderItem>();
        private ObservableCollection<Order> _orders { get; set; }

        public OrderItemViewModel()
        {
            _context = new InventoryManagementDbContext();
            Load();

            AddOrderItemCommand = new RelayCommand(async () => await AddOrderItemAsync(), CanAddOrderItem);
            UpdateOrderItemCommand = new RelayCommand(async () => await UpdateOrderItemAsync(), CanUpdateOrderItem);
            DeleteOrderItemCommand = new RelayCommand(async () => await DeleteOrderItemAsync(), CanDeleteOrderItem);

            TryAddOrderItemCommand = new RelayCommand(async () => await TryAddOrderItemAsync());
            TryUpdateOrderItemCommand = new RelayCommand(async () => await TryUpdateOrderItemAsync());
            TryDeleteOrderItemCommand = new RelayCommand(async () => await TryDeleteOrderItemAsync());
        }

        public async void Load()
        {
            await LoadOrderItemsAsync();
        }
        public ObservableCollection<Order> Orders
        {
            get
            {
                var orders = _context.Orders.ToList();
                return new ObservableCollection<Order>(orders);
            }
        }
        public OrderItem SelectedOrderItem
        {
            get => _selectedOrderItem;
            set
            {
                _selectedOrderItem = value;
                if (_selectedOrderItem != null)
                {
                    OrderItemQuantity = _selectedOrderItem.Quantity;
                    SelectedProduct = _selectedOrderItem.Product;
                    SelectedOrder = _selectedOrderItem.Order;
                }
                OnPropertyChanged(nameof(SelectedOrderItem));
                ((RelayCommand)UpdateOrderItemCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteOrderItemCommand).RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(IsUpdateEnabled));
            }
        }

        public int OrderItemQuantity
        {
            get => _orderItemQuantity;
            set
            {
                _orderItemQuantity = value;
                OnPropertyChanged(nameof(OrderItemQuantity));
                ((RelayCommand)AddOrderItemCommand).RaiseCanExecuteChanged();
                ((RelayCommand)UpdateOrderItemCommand).RaiseCanExecuteChanged();
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

        public Order SelectedOrder
        {
            get => _selectedOrder;
            set
            {
                _selectedOrder = value;
                OnPropertyChanged(nameof(SelectedOrder));
            }
        }


        public bool IsUpdateEnabled => SelectedOrderItem != null;

        private async Task LoadOrderItemsAsync()
        {
            var orderItems = await _context.OrderItems.Include(oi => oi.Product).Include(oi => oi.Order).ToListAsync();
            OrderItems.Clear();
            foreach (var orderItem in orderItems)
            {
                OrderItems.Add(orderItem);
            }
        }

        public ObservableCollection<Product> Products
        {
            get
            {
                var products = _context.Products.ToList();
                return new ObservableCollection<Product>(products);
            }
        }

        private bool CanAddOrderItem()
        {
            return OrderItemQuantity != 0 && SelectedOrder != null && SelectedProduct != null;
        }

        private async Task AddOrderItemAsync()
        {
            var orderItem = new OrderItem
            {
                Quantity = OrderItemQuantity,
                ProductId = SelectedProduct.Id,
                OrderId = SelectedOrder.Id
            };

            _context.OrderItems.Add(orderItem);
            await _context.SaveChangesAsync();
            OrderItems.Add(orderItem);

            OrderItemQuantity = 0;
            MessageBox.Show("Order item added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task TryAddOrderItemAsync()
        {
            if (!CanAddOrderItem())
            {
                MessageBox.Show("You can't add order item. Check if the order item quantity is empty or a product or order is selected.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await AddOrderItemAsync();
        }

        private bool CanUpdateOrderItem()
        {
            return SelectedOrderItem != null;
        }

        private async Task UpdateOrderItemAsync()
        {
            if (SelectedOrderItem == null)
            {
                return;
            }

            if (MessageBox.Show("Are you sure that you want to update this order item?", "Confirm Update", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            SelectedOrderItem.Quantity = OrderItemQuantity;
            SelectedOrderItem.ProductId = SelectedProduct.Id;
            SelectedOrderItem.OrderId = SelectedOrder.Id;

            _context.OrderItems.Update(SelectedOrderItem);
            await _context.SaveChangesAsync();
            await LoadOrderItemsAsync();
            MessageBox.Show("Order item updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task TryUpdateOrderItemAsync()
        {
            if (!CanUpdateOrderItem())
            {
                MessageBox.Show("You can't update order item. Check if the order item is selected from the list.", "No Order Item Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await UpdateOrderItemAsync();
        }

        private bool CanDeleteOrderItem()
        {
            return SelectedOrderItem != null;
        }

        private async Task DeleteOrderItemAsync()
        {
            if (SelectedOrderItem == null)
            {
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this order item?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            _context.OrderItems.Remove(SelectedOrderItem);
            await _context.SaveChangesAsync();
            OrderItems.Remove(SelectedOrderItem);

            OrderItemQuantity = 0;
            MessageBox.Show("Order item deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task TryDeleteOrderItemAsync()
        {
            if (!CanDeleteOrderItem())
            {
                MessageBox.Show("You can't delete order item. Check if the order item is selected from the list.", "No Order Item Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await DeleteOrderItemAsync();
        }
    }
}
