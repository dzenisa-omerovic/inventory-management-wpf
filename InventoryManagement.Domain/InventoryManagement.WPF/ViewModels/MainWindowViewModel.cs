using InventoryManagement.WPF.Commands;
using InventoryManagement.WPF.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace InventoryManagement.WPF.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelTab _selectedView;
        public ObservableCollection<ViewModelTab> Views { get; }

        public MainWindowViewModel()
        {
            Views = new ObservableCollection<ViewModelTab>
            {
                new ViewModelTab { DisplayName = "Products", Content = new ProductViewModel() },
                new ViewModelTab { DisplayName = "Warehouses", Content = new WarehouseViewModel() },
                new ViewModelTab { DisplayName = "Warehouse locations", Content = new WarehouseLocationViewModel() },
                new ViewModelTab { DisplayName = "Person suppliers", Content = new PersonSupplierViewModel() },
                new ViewModelTab { DisplayName = "Company suppliers", Content = new CompanySupplierViewModel() },
                new ViewModelTab { DisplayName = "Warehouse products", Content = new WarehouseProductViewModel() },
                new ViewModelTab { DisplayName = "Order items", Content = new OrderItemViewModel() },
                new ViewModelTab { DisplayName = "Orders", Content = new OrderViewModel() }
            };
        }

        public ViewModelTab SelectedView
        {
            get => _selectedView;
            set
            {
                _selectedView = value;
                OnPropertyChanged(nameof(SelectedView));
            }
        }
    }

    public class ViewModelTab
    {
        public string DisplayName { get; set; }
        public ViewModelBase Content { get; set; }
    }
}