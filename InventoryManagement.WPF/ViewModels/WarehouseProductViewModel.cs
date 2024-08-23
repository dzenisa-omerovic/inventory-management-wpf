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
    public class WarehouseProductViewModel : ViewModelBase
    {
        private readonly InventoryManagementDbContext _context;

        private ObservableCollection<WarehouseProduct> _warehouseProducts { get; set; }

        public ICommand GenerateReportCommand { get; }
        private WarehouseProduct _selectedWarehouseProduct;
        public WarehouseProductViewModel()
        {
            _context = new InventoryManagementDbContext();
            GenerateReportCommand = new RelayCommand(GenerateReport);
        }
        public WarehouseProduct SelectedWarehouseProduct
        {
            get => _selectedWarehouseProduct;
            set
            {
                _selectedWarehouseProduct = value;
                OnPropertyChanged(nameof(SelectedWarehouseProduct));
            }
        }
        public ObservableCollection<WarehouseProduct> WarehouseProducts
        {
            get
            {
                var warehouseProducts = _context.WarehouseProducts.Include(wp => wp.WarehouseLocation).ThenInclude(wl => wl.Warehouse).Include(wp => wp.Product).Include(wp => wp.Supplier).ToList();
                return new ObservableCollection<WarehouseProduct>(warehouseProducts);
            }
        }
        private void GenerateReport()
        {
            if (SelectedWarehouseProduct == null)
            {
                MessageBox.Show("Please select a warehouse product to generate a report.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var reportWindow = new ReportWindow(SelectedWarehouseProduct);
            reportWindow.Show();
        }
        
    }
}
