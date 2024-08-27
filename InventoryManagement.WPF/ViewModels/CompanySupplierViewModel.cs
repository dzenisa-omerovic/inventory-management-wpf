using InventoryManagement.Domain.Models;
using InventoryManagement.WPF.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace InventoryManagement.WPF.ViewModels
{
    public class CompanySupplierViewModel : SupplierViewModel
    {
        private CompanySupplier _selectedCompanySupplier;
        private string _name;
        private string _address;

        public ObservableCollection<CompanySupplier> CompanySuppliers { get; set; } = new ObservableCollection<CompanySupplier>();

        public CompanySupplierViewModel()
        {
            LoadCompanySuppliersAsync();
        }
        public string Address
        {
            get => _address;
            set
            {
                _address = value;
                OnPropertyChanged(nameof(Address));
                ((RelayCommand)AddSupplierCommand).RaiseCanExecuteChanged();
                ((RelayCommand)UpdateSupplierCommand).RaiseCanExecuteChanged();
            }
        }
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
                ((RelayCommand)AddSupplierCommand).RaiseCanExecuteChanged();
                ((RelayCommand)UpdateSupplierCommand).RaiseCanExecuteChanged();
            }
        }

        public CompanySupplier SelectedCompanySupplier
        {
            get => _selectedCompanySupplier;
            set
            {
                _selectedCompanySupplier = value;
                if (_selectedCompanySupplier != null)
                {
                    Name = _selectedCompanySupplier.Name;
                    Address = _selectedCompanySupplier.Address;
                }
                OnPropertyChanged(nameof(SelectedCompanySupplier));
                ((RelayCommand)UpdateSupplierCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteSupplierCommand).RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(IsUpdateEnabled));
            }
        }

        public bool IsUpdateEnabled => SelectedCompanySupplier != null;

        private async Task LoadCompanySuppliersAsync()
        {
            var suppliers = await _context.CompanySuppliers.ToListAsync();
            CompanySuppliers.Clear();
            foreach (var supplier in suppliers)
            {
                CompanySuppliers.Add(supplier);
            }
        }

        protected override bool CanAddSupplier()
        {
            return !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Address);
        }

        protected override async Task AddSupplierAsync()
        {
            if (CompanySuppliers.Any(s => s.Name == Name && s.Address == Address))
            {
                MessageBox.Show("A supplier with the same name and address already exists.", "Duplicate Supplier", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (CompanySuppliers.Any(s => s.Name == Name))
            {
                MessageBox.Show("A supplier with the same name already exists.", "Duplicate Supplier", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var companySupplier = new CompanySupplier
            {
                Name = Name,
                Address = Address
            };

            _context.CompanySuppliers.Add(companySupplier);
            await _context.SaveChangesAsync();
            CompanySuppliers.Add(companySupplier);

            ClearInputFields();
            MessageBox.Show("Supplier added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override async Task TryAddSupplierAsync()
        {
            if (!CanAddSupplier())
            {
                MessageBox.Show("You can't add supplier. Check if the supplier name and address are empty.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await AddSupplierAsync();
        }

        protected override bool CanUpdateSupplier()
        {
            return SelectedCompanySupplier != null;
        }

        protected override async Task UpdateSupplierAsync()
        {
            if (SelectedCompanySupplier == null)
            {
                return;
            }

            bool isSupplierInOrderItems = await _context.Orders.AnyAsync(o => o.SupplierId == SelectedCompanySupplier.Id);
            bool isSupplierInWarehouse = await _context.WarehouseProducts.AnyAsync(wp => wp.SupplierId == SelectedCompanySupplier.Id);

            if (isSupplierInOrderItems || isSupplierInWarehouse)
            {
                MessageBox.Show("You can update only suppliers which are not added to the order or warehouse.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure that you want to update this supplier?", "Confirm Update", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            SelectedCompanySupplier.Name = Name;
            SelectedCompanySupplier.Address = Address;

            _context.CompanySuppliers.Update(SelectedCompanySupplier);
            await _context.SaveChangesAsync();
            await LoadCompanySuppliersAsync();
            MessageBox.Show("Supplier updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override async Task TryUpdateSupplierAsync()
        {
            if (!CanUpdateSupplier())
            {
                MessageBox.Show("You can't update supplier. Check if the supplier is selected from the list.", "No Supplier Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await UpdateSupplierAsync();
        }

        protected override bool CanDeleteSupplier()
        {
            return SelectedCompanySupplier != null;
        }

        protected override async Task DeleteSupplierAsync()
        {
            if (SelectedCompanySupplier == null)
            {
                return;
            }

            bool isSupplierInOrderItems = await _context.Orders.AnyAsync(o => o.SupplierId == SelectedCompanySupplier.Id);
            bool isSupplierInWarehouse = await _context.WarehouseProducts.AnyAsync(wp => wp.SupplierId == SelectedCompanySupplier.Id);

            if (isSupplierInOrderItems || isSupplierInWarehouse)
            {
                MessageBox.Show("You can delete only suppliers which are not added to the order or warehouse.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this supplier?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            _context.CompanySuppliers.Remove(SelectedCompanySupplier);
            await _context.SaveChangesAsync();
            CompanySuppliers.Remove(SelectedCompanySupplier);

            ClearInputFields();
            SelectedCompanySupplier = null;

            MessageBox.Show("Supplier deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override async Task TryDeleteSupplierAsync()
        {
            if (!CanDeleteSupplier())
            {
                MessageBox.Show("You can't delete supplier. Check if the supplier is selected from the list.", "No Supplier Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await DeleteSupplierAsync();
        }

        private void ClearInputFields()
        {
            Name = string.Empty;
            Address = string.Empty;
        }
    }
}
