using InventoryManagement.Domain.Models;
using InventoryManagement.EntityFramework.Database;
using InventoryManagement.WPF.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace InventoryManagement.WPF.ViewModels
{
    public class ProductViewModel : ViewModelBase
    {
        private readonly InventoryManagementDbContext _context;
        private Product _selectedProduct;
        private string _productName;
        private decimal _productPrice;
        public ICommand AddProductCommand { get; }
        public ICommand UpdateProductCommand { get; }
        public ICommand DeleteProductCommand { get; }
        public ICommand TryAddProductCommand { get; }
        public ICommand TryUpdateProductCommand { get; }
        public ICommand TryDeleteProductCommand { get; }
        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();

        public ProductViewModel()
        {
            _context = new InventoryManagementDbContext();
            LoadProducts();

            AddProductCommand = new RelayCommand(AddProduct, CanAddProduct);
            UpdateProductCommand = new RelayCommand(UpdateProduct, CanUpdateProduct);
            DeleteProductCommand = new RelayCommand(DeleteProduct, CanDeleteProduct);

            TryAddProductCommand = new RelayCommand(TryAddProduct);
            TryUpdateProductCommand = new RelayCommand(TryUpdateProduct);
            TryDeleteProductCommand = new RelayCommand(TryDeleteProduct);
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                _selectedProduct = value;
                if (_selectedProduct != null)
                {
                    ProductName = _selectedProduct.Name;
                    ProductPrice = _selectedProduct.Price;
                }
                OnPropertyChanged(nameof(SelectedProduct));
                ((RelayCommand)UpdateProductCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteProductCommand).RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(IsUpdateEnabled));
            }
        }

        public string ProductName
        {
            get => _productName;
            set
            {
                _productName = value;
                OnPropertyChanged(nameof(ProductName));
                ((RelayCommand)AddProductCommand).RaiseCanExecuteChanged();
                ((RelayCommand)UpdateProductCommand).RaiseCanExecuteChanged();
            }
        }

        public decimal ProductPrice
        {
            get => _productPrice;
            set
            {
                _productPrice = value;
                OnPropertyChanged(nameof(ProductPrice));
                ((RelayCommand)AddProductCommand).RaiseCanExecuteChanged();
                ((RelayCommand)UpdateProductCommand).RaiseCanExecuteChanged();
            }
        }
        public bool IsUpdateEnabled => SelectedProduct != null;

        private async void LoadProducts()
        {
             var products = await _context.Products.ToListAsync();
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
        }

        private bool CanAddProduct()
        {
            return !string.IsNullOrWhiteSpace(ProductName) && ProductPrice > 0;
        }

        private async void AddProduct()
        {
            ProductPrice = Math.Round(ProductPrice, 2);

            if (Products.Any(p => p.Name == ProductName && p.Price == ProductPrice))
            {
                MessageBox.Show("A product with the same name and price already exists.", "Duplicate Product", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (Products.Any(p => p.Name == ProductName))
            {
                MessageBox.Show("A product with the same name already exists.", "Duplicate Product", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var product = new Product
            {
                Name = ProductName,
                Price = ProductPrice
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            Products.Add(product);

            ProductName = string.Empty;
            ProductPrice = 0;
            MessageBox.Show("Product added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TryAddProduct()
        {
            if (!CanAddProduct())
            {
                MessageBox.Show("You can't add product. Check if the product name is empty and the price is or less than 0.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            AddProduct();
        }

        private bool CanUpdateProduct()
        {
            return SelectedProduct != null;
        }

        private async void UpdateProduct()
        {
            if (SelectedProduct == null)
            {
                return;
            }

            bool isProductInOrderItems = await _context.OrderItems.AnyAsync(oi => oi.ProductId == SelectedProduct.Id);
            bool isProductInWarehouse = await _context.WarehouseProducts.AnyAsync(wp => wp.ProductId == SelectedProduct.Id);

            if (isProductInOrderItems || isProductInWarehouse)
            {
                MessageBox.Show("You can update only products which are not added to the order or warehouse.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure that you want to update this product?", "Confirm Update", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            SelectedProduct.Name = ProductName;
            SelectedProduct.Price = ProductPrice;

            _context.Products.Update(SelectedProduct);
            await _context.SaveChangesAsync();
            LoadProducts();
            MessageBox.Show("Product updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void TryUpdateProduct()
        {
            if (!CanUpdateProduct())
            {
                MessageBox.Show("You can't update product. Check if the product is selected from the list.", "No Product Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            UpdateProduct();
        }

        private bool CanDeleteProduct()
        {
            return SelectedProduct != null;
        }
        private async void DeleteProduct()
        {
            if (SelectedProduct == null)
            {
                return;
            }

            bool isProductInOrderItems = await _context.OrderItems.AnyAsync(oi => oi.ProductId == SelectedProduct.Id);
            bool isProductInWarehouse = await _context.WarehouseProducts.AnyAsync(wp => wp.ProductId == SelectedProduct.Id);

            if (isProductInOrderItems || isProductInWarehouse)
            {
                MessageBox.Show("You can delete only products which are not added to the order or warehouse.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this product?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            _context.Products.Remove(SelectedProduct);
            await _context.SaveChangesAsync();
            Products.Remove(SelectedProduct);

            ProductName = string.Empty;
            ProductPrice = 0;
            MessageBox.Show("Product deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }



        private void TryDeleteProduct()
        {
            if (!CanDeleteProduct())
            {
                MessageBox.Show("You can't delete product. Check if the product is selected from the list.", "No Product Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DeleteProduct();
        }
    }
}