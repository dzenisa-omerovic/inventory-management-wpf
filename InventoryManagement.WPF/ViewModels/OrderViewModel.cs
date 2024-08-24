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
        public ICommand ApplyFiltersCommand { get; }
        private readonly InventoryManagementDbContext _context;
        private ObservableCollection<OrderItem> _selectedOrderItems;
        private Order _selectedOrder;
        private decimal _selectedOrderTotalPrice;
        private DateTime? _startDate;
        private DateTime? _endDate;
        public ICommand OpenAddOrderWindowCommand { get; }
        public ICommand MarkAsArrivedCommand { get; }
        public ICommand AddToWarehouseCommand { get; }
        public ICommand DeleteOrderCommand { get; }
        public ICommand GenerateReportCommand { get; }
        public ICommand ResetFiltersCommand { get; }
        public ICommand ViewPieChartStatusCommand { get; }
        public ObservableCollection<Order> Orders { get; set; }
        public OrderViewModel()
        {
            _context = new InventoryManagementDbContext();
            Orders = new ObservableCollection<Order>();
            _selectedOrderItems = new ObservableCollection<OrderItem>();
            LoadOrders();
            
            OpenAddOrderWindowCommand = new RelayCommand(OpenAddOrderWindow);
            MarkAsArrivedCommand = new RelayCommand(MarkAsArrived);
            AddToWarehouseCommand = new RelayCommand(OpenAddToWarehouseWindow);
            DeleteOrderCommand = new RelayCommand(DeleteOrder);
            GenerateReportCommand = new RelayCommand(GenerateReport);
            ViewPieChartStatusCommand = new RelayCommand(OpenPieChartWindow);
            ApplyFiltersCommand = new RelayCommand(ApplyFilters);
            ResetFiltersCommand = new RelayCommand(ResetFilters);

        }
        private void GenerateReport()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Please select an order to generate a report.", "No Order Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var reportOrderWindow = new ReportOrderWindow(SelectedOrder);
            reportOrderWindow.Show();
        }
        private void DeleteOrder()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Please select an order to delete.", "No Order Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedOrder.Status != "Pending")
            {
                MessageBox.Show("Only orders with a status of 'Pending' can be deleted.", "Invalid Order Status", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _context.Orders.Remove(SelectedOrder);
            _context.SaveChanges();
            Orders.Remove(SelectedOrder);
            SelectedOrderItems.Clear();
            MessageBox.Show("Order deleted successfully.", "Order Deleted", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void OpenAddToWarehouseWindow()
        {
            if (SelectedOrder == null)
            {
                MessageBox.Show("Please select an order to add to warehouse.", "No Order Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SelectedOrder.Status == "Added to warehouse")
            {
                MessageBox.Show("The selected order is already added to the warehouse.", "Order Added", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (SelectedOrder.Status != "Arrived")
            {
                MessageBox.Show("The selected order has not been marked as 'Arrived'. Please mark it as arrived before adding it to the warehouse.", "Order Not Arrived", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var addToWarehouseWindow = new AddToWarehouseWindow();
            var addToWarehouseViewModel = new AddToWarehouseViewModel(SelectedOrder, _context, addToWarehouseWindow);
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
        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                OnPropertyChanged(nameof(StartDate));
            }
        }

        public DateTime? EndDate
        {
            get => _endDate;
            set
            {
                _endDate = value;
                OnPropertyChanged(nameof(EndDate));
            }
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
            var addOrderViewModel = new AddOrderViewModel(addOrderWindow);


            addOrderViewModel.OnOrderSaved += RefreshOrders;

            addOrderWindow.DataContext = addOrderViewModel;
            addOrderWindow.ShowDialog();

            addOrderViewModel.OnOrderSaved -= RefreshOrders;
        }

        private void RefreshOrders()
        {
            LoadOrders();
        }
        private void OpenPieChartWindow()
        {
            var pieChartWindow = new PieChartWindow();
            pieChartWindow.Show();
        }
        private void ApplyFilters()
        {
            if (StartDate.HasValue && EndDate.HasValue)
            {
                if (EndDate < StartDate)
                {
                    MessageBox.Show("End date cannot be earlier than start date.", "Invalid Date Range", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            var orders = _context.Orders.AsQueryable();

            if (StartDate.HasValue)
            {
                orders = orders.Where(o => o.Date >= StartDate.Value);
            }

            if (EndDate.HasValue)
            {
                orders = orders.Where(o => o.Date <= EndDate.Value);
            }

            Orders.Clear();
            foreach (var order in orders.ToList())
            {
                Orders.Add(order);
            }
        }
        private void ResetFilters()
        {
            StartDate = null;
            EndDate = null;
            LoadOrders();
        }
    }
}
