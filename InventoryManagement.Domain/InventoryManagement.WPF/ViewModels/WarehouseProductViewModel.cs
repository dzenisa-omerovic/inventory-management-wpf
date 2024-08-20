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
    public class WarehouseProductViewModel : ViewModelBase
    {
        private readonly InventoryManagementDbContext _context;
        private WarehouseProduct _selectedWarehouseProduct;
        private int _warehouseProductQuantity;
        private Product _selectedProduct;
        private WarehouseLocation _selectedWarehouseLocation;
        private Supplier _selectedSupplier;
        public ICommand AddWarehouseProductCommand { get; }
        public ICommand UpdateWarehouseProductCommand { get; }
        public ICommand DeleteWarehouseProductCommand { get; }
        public ICommand TryAddWarehouseProductCommand { get; }
        public ICommand TryUpdateWarehouseProductCommand { get; }
        public ICommand TryDeleteWarehouseProductCommand { get; }
        public ObservableCollection<WarehouseProduct> WarehouseProducts { get; set; } = new ObservableCollection<WarehouseProduct>();
        private ObservableCollection<Product> _products { get; set; }
        private ObservableCollection<WarehouseLocation> _warehouseLocations { get; set; }
        private ObservableCollection<Supplier> _suppliers { get; set; }

        public WarehouseProductViewModel()
        {
            _context = new InventoryManagementDbContext();
            Load();

            AddWarehouseProductCommand = new RelayCommand(async () => await AddWarehouseProductAsync(), CanAddWarehouseProduct);
            UpdateWarehouseProductCommand = new RelayCommand(async () => await UpdateWarehouseProductAsync(), CanUpdateWarehouseProduct);
            DeleteWarehouseProductCommand = new RelayCommand(async () => await DeleteWarehouseProductAsync(), CanDeleteWarehouseProduct);

            TryAddWarehouseProductCommand = new RelayCommand(async () => await TryAddWarehouseProductAsync());
            TryUpdateWarehouseProductCommand = new RelayCommand(async () => await TryUpdateWarehouseProductAsync());
            TryDeleteWarehouseProductCommand = new RelayCommand(async () => await TryDeleteWarehouseProductAsync());
        }

        public async void Load()
        {
            await LoadWarehouseProductsAsync();
        }

        public WarehouseProduct SelectedWarehouseProduct
        {
            get => _selectedWarehouseProduct;
            set
            {
                _selectedWarehouseProduct = value;
                if (_selectedWarehouseProduct != null)
                {
                    WarehouseProductQuantity = _selectedWarehouseProduct.Quantity;
                    SelectedProduct = _selectedWarehouseProduct.Product;
                    SelectedWarehouseLocation = _selectedWarehouseProduct.WarehouseLocation;
                    SelectedSupplier = _selectedWarehouseProduct.Supplier;
                }
                OnPropertyChanged(nameof(SelectedWarehouseProduct));
                ((RelayCommand)UpdateWarehouseProductCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteWarehouseProductCommand).RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(IsUpdateEnabled));
            }
        }

        public int WarehouseProductQuantity
        {
            get => _warehouseProductQuantity;
            set
            {
                _warehouseProductQuantity = value;
                OnPropertyChanged(nameof(WarehouseProductQuantity));
                ((RelayCommand)AddWarehouseProductCommand).RaiseCanExecuteChanged();
                ((RelayCommand)UpdateWarehouseProductCommand).RaiseCanExecuteChanged();
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

        public WarehouseLocation SelectedWarehouseLocation
        {
            get => _selectedWarehouseLocation;
            set
            {
                _selectedWarehouseLocation = value;
                OnPropertyChanged(nameof(SelectedWarehouseLocation));
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

        public bool IsUpdateEnabled => SelectedWarehouseProduct != null;

        private async Task LoadWarehouseProductsAsync()
        {
            var warehouseProducts = await _context.WarehouseProducts.Include(wp => wp.Product).Include(wp => wp.WarehouseLocation).Include(wp => wp.Supplier).ToListAsync();
            WarehouseProducts.Clear();
            foreach (var warehouseProduct in warehouseProducts)
            {
                WarehouseProducts.Add(warehouseProduct);
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
        public ObservableCollection<WarehouseLocation> WarehouseLocations
        {
            get
            {
                var warehouseLocations = _context.WarehouseLocations.Include(wl => wl.Warehouse).ToList();
                return new ObservableCollection<WarehouseLocation>(warehouseLocations);
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

        private bool CanAddWarehouseProduct()
        {
            return WarehouseProductQuantity != 0 && SelectedWarehouseLocation != null && SelectedProduct != null && SelectedSupplier != null;
        }

        private async Task AddWarehouseProductAsync()
        {
            var warehouseProduct = new WarehouseProduct
            {
                Quantity = WarehouseProductQuantity,
                WarehouseLocationId = SelectedWarehouseLocation.Id,
                ProductId = SelectedProduct.Id,
                SupplierId = SelectedSupplier.Id
            };

            _context.WarehouseProducts.Add(warehouseProduct);
            await _context.SaveChangesAsync();
            WarehouseProducts.Add(warehouseProduct);

            WarehouseProductQuantity = 0;
            MessageBox.Show("Warehouse product added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task TryAddWarehouseProductAsync()
        {
            if (!CanAddWarehouseProduct())
            {
                MessageBox.Show("You can't add warehouse product. Check if the warehouse product quantity is empty or a product, warehouse location or supplier is selected.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await AddWarehouseProductAsync();
        }

        private bool CanUpdateWarehouseProduct()
        {
            return SelectedWarehouseProduct != null;
        }

        private async Task UpdateWarehouseProductAsync()
        {
            if (SelectedWarehouseProduct == null)
            {
                return;
            }

            if (MessageBox.Show("Are you sure that you want to update this warehouse product?", "Confirm Update", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            SelectedWarehouseProduct.Quantity = WarehouseProductQuantity;
            SelectedWarehouseProduct.WarehouseLocationId = SelectedWarehouseLocation.Id;
            SelectedWarehouseProduct.ProductId = SelectedProduct.Id;
            SelectedWarehouseProduct.SupplierId = SelectedSupplier.Id;

            _context.WarehouseProducts.Update(SelectedWarehouseProduct);
            await _context.SaveChangesAsync();
            await LoadWarehouseProductsAsync();
            MessageBox.Show("Warehouse product updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task TryUpdateWarehouseProductAsync()
        {
            if (!CanUpdateWarehouseProduct())
            {
                MessageBox.Show("You can't update warehouse product. Check if the warehouse product is selected from the list.", "No Warehouse Product Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await UpdateWarehouseProductAsync();
        }

        private bool CanDeleteWarehouseProduct()
        {
            return SelectedWarehouseProduct != null;
        }

        private async Task DeleteWarehouseProductAsync()
        {
            if (SelectedWarehouseProduct == null)
            {
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this warehouse product?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            _context.WarehouseProducts.Remove(SelectedWarehouseProduct);
            await _context.SaveChangesAsync();
            WarehouseProducts.Remove(SelectedWarehouseProduct);

            WarehouseProductQuantity = 0;
            MessageBox.Show("Warehouse product deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task TryDeleteWarehouseProductAsync()
        {
            if (!CanDeleteWarehouseProduct())
            {
                MessageBox.Show("You can't delete warehouse product. Check if the warehouse product is selected from the list.", "No Warehouse Product Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await DeleteWarehouseProductAsync();
        }
    }
}
