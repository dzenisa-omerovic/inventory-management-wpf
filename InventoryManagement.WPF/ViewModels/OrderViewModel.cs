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
using InventoryManagement.WPF.Views;
namespace InventoryManagement.WPF.ViewModels
{
    public class OrderViewModel : ViewModelBase
    {
        
        private readonly InventoryManagementDbContext _context;
        public ICommand OpenAddOrderWindowCommand { get; }
        public ICommand MarkAsArrivedCommand { get; }
        public ICommand AddToWarehouseCommand { get; }
        public ObservableCollection<Order> Orders { get; set; }
        private ObservableCollection<OrderItem> _selectedOrderItems;
        private Order _selectedOrder;
        private decimal _selectedOrderTotalPrice;
        public OrderViewModel()
        {
            _context = new InventoryManagementDbContext();
            Orders = new ObservableCollection<Order>();
            _selectedOrderItems = new ObservableCollection<OrderItem>();
            LoadOrders();

            OpenAddOrderWindowCommand = new RelayCommand(OpenAddOrderWindow);
            MarkAsArrivedCommand = new RelayCommand(MarkAsArrived);
            AddToWarehouseCommand = new RelayCommand(OpenAddToWarehouseWindow);

        }
        private void OpenAddToWarehouseWindow()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Please select an order to add to warehouse.", "No Order Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SelectedOrder.Status != "Arrived")
            {
                MessageBox.Show("The selected order has not been marked as 'Arrived'. Please mark it as arrived before adding it to the warehouse.", "Order Not Arrived", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SelectedOrder.Status == "Added to warehouse")
            {
                MessageBox.Show("The selected order is already added to the warehouse.", "Order Added", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var addToWarehouseWindow = new AddToWarehouseWindow();
            var addToWarehouseViewModel = new AddToWarehouseViewModel(SelectedOrder, _context);
            addToWarehouseViewModel.OnOrderAdded += RefreshOrders;

            addToWarehouseWindow.DataContext = addToWarehouseViewModel;
            addToWarehouseWindow.ShowDialog();
            
            addToWarehouseViewModel.OnOrderAdded -= RefreshOrders;

        }
        private void MarkAsArrived()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Please select an order to mark as arrived.", "No Order Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SelectedOrder.Status == "Added to warehouse")
            {
                MessageBox.Show("The selected order is already added to the warehouse.", "Order Added", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SelectedOrder.Status = "Arrived";
            _context.SaveChanges();
            RefreshOrders();

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
                OnPropertyChanged(nameof(SelectedOrder));
                OnSelectedOrderChanged();
            }
        }

        public decimal SelectedOrderTotalPrice
        {
            get => _selectedOrderTotalPrice;
            set
            {
                _selectedOrderTotalPrice = value;
                OnPropertyChanged(nameof(SelectedOrderTotalPrice));
            }
        }
        private void OnSelectedOrderChanged()
        {
            if (SelectedOrder != null)
            {
                SelectedOrderItems.Clear();
                foreach (var item in SelectedOrder.OrderItems)
                {
                    SelectedOrderItems.Add(item);
                }

                SelectedOrderTotalPrice = SelectedOrderItems.Sum(item => item.Quantity * (item.Product?.Price ?? 0));
            }
        }
        private void LoadOrders()
        {
            var orders = _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Product).Include(o => o.Supplier).ToList();
            Orders.Clear();
            foreach (var order in orders)
            {
                Orders.Add(order);
            }
        }

        private void OpenAddOrderWindow()
        {
            var addOrderWindow = new AddOrderWindow();
            var addOrderViewModel = new AddOrderViewModel();


            addOrderViewModel.OnOrderSaved += RefreshOrders;

            addOrderWindow.DataContext = addOrderViewModel;
            addOrderWindow.ShowDialog();

            addOrderViewModel.OnOrderSaved -= RefreshOrders;
        }

        private void RefreshOrders()
        {
            LoadOrders();
        }
        
    }

}
