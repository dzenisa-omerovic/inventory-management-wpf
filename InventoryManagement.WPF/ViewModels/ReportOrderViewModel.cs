using InventoryManagement.Domain.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InventoryManagement.WPF.ViewModels
{
    public class ReportOrderViewModel : ViewModelBase
    {
        private Order _order;
        public ObservableCollection<OrderItem> OrderItems { get; private set; }
        public string Name { get; private set; }
        public string OrderDate { get; private set; }
        public string Supplier { get; private set; }
        public string Status { get; private set; }
        public decimal TotalPrice { get; private set; }
        public ReportOrderViewModel(Order order)
        {
            _order = order;
            LoadOrderDetails();
        }
        private void LoadOrderDetails()
        {
            if (_order != null)
            {
                OrderItems = new ObservableCollection<OrderItem>(_order.OrderItems);
                Name = _order.Name;
                OrderDate = _order.Date.ToString("MM/dd/yyyy HH:mm:ss");
                Supplier = _order.Supplier?.DisplayName ?? "Unknown";
                Status = _order.Status;
                TotalPrice = OrderItems.Sum(item => item.Quantity * (item.Product?.Price ?? 0));
                OnPropertyChanged(nameof(OrderItems));
                OnPropertyChanged(nameof(OrderDate));
                OnPropertyChanged(nameof(Supplier));
                OnPropertyChanged(nameof(Status));
                OnPropertyChanged(nameof(TotalPrice));
            }
        }
    }
}
