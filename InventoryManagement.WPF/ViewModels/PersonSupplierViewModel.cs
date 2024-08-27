using InventoryManagement.Domain.Models;
using InventoryManagement.WPF.Commands;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace InventoryManagement.WPF.ViewModels
{
    public class PersonSupplierViewModel : SupplierViewModel
    {
        private PersonSupplier _selectedPersonSupplier;
        private string _firstName;
        private string _lastName;
        private string _address { get; set; }

        public ObservableCollection<PersonSupplier> PersonSuppliers { get; set; } = new ObservableCollection<PersonSupplier>();

        public PersonSupplierViewModel()
        {
            LoadPersonSuppliersAsync();
        }

        public string FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                OnPropertyChanged(nameof(FirstName));
                ((RelayCommand)AddSupplierCommand).RaiseCanExecuteChanged();
                ((RelayCommand)UpdateSupplierCommand).RaiseCanExecuteChanged();
            }
        }

        public string LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                OnPropertyChanged(nameof(LastName));
                ((RelayCommand)AddSupplierCommand).RaiseCanExecuteChanged();
                ((RelayCommand)UpdateSupplierCommand).RaiseCanExecuteChanged();
            }
        }

        public PersonSupplier SelectedPersonSupplier
        {
            get => _selectedPersonSupplier;
            set
            {
                _selectedPersonSupplier = value;
                if (_selectedPersonSupplier != null)
                {
                    FirstName = _selectedPersonSupplier.FirstName;
                    LastName = _selectedPersonSupplier.LastName;
                    Address = _selectedPersonSupplier.Address;
                }
                OnPropertyChanged(nameof(SelectedPersonSupplier));
                ((RelayCommand)UpdateSupplierCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteSupplierCommand).RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(IsUpdateEnabled));
            }
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

        public bool IsUpdateEnabled => SelectedPersonSupplier != null;

        private async Task LoadPersonSuppliersAsync()
        {
            var suppliers = await _context.PersonSuppliers.ToListAsync();
            PersonSuppliers.Clear();
            foreach (var supplier in suppliers)
            {
                PersonSuppliers.Add(supplier);
            }
        }

        protected override bool CanAddSupplier()
        {
            return !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName) && !string.IsNullOrWhiteSpace(Address);
        }

        protected override async Task AddSupplierAsync()
        {
            if (PersonSuppliers.Any(p => p.FirstName == FirstName && p.LastName == LastName && p.Address == Address))
            {
                MessageBox.Show("A supplier with the same first name, last name and address already exists.", "Duplicate Supplier", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var personSupplier = new PersonSupplier
            {
                FirstName = FirstName,
                LastName = LastName,
                Address = Address
            };

            _context.PersonSuppliers.Add(personSupplier);
            await _context.SaveChangesAsync();
            PersonSuppliers.Add(personSupplier);

            ClearInputFields();
            MessageBox.Show("Supplier added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        protected override async Task TryAddSupplierAsync()
        {
            if (!CanAddSupplier())
            {
                MessageBox.Show("You can't add supplier. Check if the warehouse name is empty.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await AddSupplierAsync();
        }

        protected override bool CanUpdateSupplier()
        {
            return SelectedPersonSupplier != null;
        }

        protected override async Task UpdateSupplierAsync()
        {
            if (SelectedPersonSupplier == null)
            {
                return;
            }

            bool isSupplierInOrderItems = await _context.Orders.AnyAsync(o => o.SupplierId == SelectedPersonSupplier.Id);
            bool isSupplierInWarehouse = await _context.WarehouseProducts.AnyAsync(wp => wp.SupplierId == SelectedPersonSupplier.Id);

            if (isSupplierInOrderItems || isSupplierInWarehouse)
            {
                MessageBox.Show("You can update only suppliers which are not added to the order or warehouse.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure that you want to update this supplier?", "Confirm Update", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            SelectedPersonSupplier.FirstName = FirstName;
            SelectedPersonSupplier.LastName = LastName;
            SelectedPersonSupplier.Address = Address;

            _context.PersonSuppliers.Update(SelectedPersonSupplier);
            await _context.SaveChangesAsync();
            await LoadPersonSuppliersAsync();
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
            return SelectedPersonSupplier != null;
        }

        protected override async Task DeleteSupplierAsync()
        {
            if (SelectedPersonSupplier == null)
            {
                return;
            }

            bool isSupplierInOrderItems = await _context.Orders.AnyAsync(o => o.SupplierId == SelectedPersonSupplier.Id);
            bool isSupplierInWarehouse = await _context.WarehouseProducts.AnyAsync(wp => wp.SupplierId == SelectedPersonSupplier.Id);

            if (isSupplierInOrderItems || isSupplierInWarehouse)
            {
                MessageBox.Show("You can delete only suppliers which are not added to the order or warehouse.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this supplier?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            _context.PersonSuppliers.Remove(SelectedPersonSupplier);
            await _context.SaveChangesAsync();
            PersonSuppliers.Remove(SelectedPersonSupplier);

            ClearInputFields();
            SelectedPersonSupplier = null;

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
            FirstName = string.Empty;
            LastName = string.Empty;
            Address = string.Empty;
        }
    }
}
