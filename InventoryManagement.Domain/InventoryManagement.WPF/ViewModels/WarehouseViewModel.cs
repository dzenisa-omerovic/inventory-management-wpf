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
    public class WarehouseViewModel : ViewModelBase
    {
        private readonly InventoryManagementDbContext _context;
        private Warehouse _selectedWarehouse;
        private string _warehouseName;
        private ObservableCollection<WarehouseLocation> _selectedWarehouseLocations;
        public ICommand AddWarehouseCommand { get; }
        public ICommand UpdateWarehouseCommand { get; }
        public ICommand DeleteWarehouseCommand { get; }
        public ICommand TryAddWarehouseCommand { get; }
        public ICommand TryUpdateWarehouseCommand { get; }
        public ICommand TryDeleteWarehouseCommand { get; }
        public ICommand ShowLocationsCommand { get; }
        public ObservableCollection<Warehouse> Warehouses { get; set; } = new ObservableCollection<Warehouse>();

        public WarehouseViewModel()
        {
            _context = new InventoryManagementDbContext();
            LoadWarehousesAsync();

            AddWarehouseCommand = new RelayCommand(async () => await AddWarehouseAsync(), CanAddWarehouse);
            UpdateWarehouseCommand = new RelayCommand(async () => await UpdateWarehouseAsync(), CanUpdateWarehouse);
            DeleteWarehouseCommand = new RelayCommand(async () => await DeleteWarehouseAsync(), CanDeleteWarehouse);

            TryAddWarehouseCommand = new RelayCommand(async () => await TryAddWarehouseAsync());
            TryUpdateWarehouseCommand = new RelayCommand(async () => await TryUpdateWarehouseAsync());
            TryDeleteWarehouseCommand = new RelayCommand(async () => await TryDeleteWarehouseAsync());
            ShowLocationsCommand = new RelayCommand(async () => await ShowLocationsAsync());
        }

        public ObservableCollection<WarehouseLocation> SelectedWarehouseLocations
        {
            get => _selectedWarehouseLocations;
            set
            {
                _selectedWarehouseLocations = value;
                OnPropertyChanged(nameof(SelectedWarehouseLocations));
            }
        }

        public Warehouse SelectedWarehouse
        {
            get => _selectedWarehouse;
            set
            {
                _selectedWarehouse = value;
                if (_selectedWarehouse != null)
                {
                    WarehouseName = _selectedWarehouse.Name;
                }
                OnPropertyChanged(nameof(SelectedWarehouse));
                ((RelayCommand)UpdateWarehouseCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteWarehouseCommand).RaiseCanExecuteChanged();
                ((RelayCommand)ShowLocationsCommand).RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(IsUpdateEnabled));
            }
        }

        public string WarehouseName
        {
            get => _warehouseName;
            set
            {
                _warehouseName = value;
                OnPropertyChanged(nameof(WarehouseName));
                ((RelayCommand)AddWarehouseCommand).RaiseCanExecuteChanged();
                ((RelayCommand)UpdateWarehouseCommand).RaiseCanExecuteChanged();
            }
        }

        public bool IsUpdateEnabled => SelectedWarehouse != null;

        private async Task LoadWarehousesAsync()
        {
            var warehouses = await _context.Warehouses.ToListAsync();
            Warehouses.Clear();
            foreach (var warehouse in warehouses)
            {
                Warehouses.Add(warehouse);
            }
        }

        private bool CanAddWarehouse()
        {
            return !string.IsNullOrWhiteSpace(WarehouseName);
        }

        private async Task AddWarehouseAsync()
        {
            if (Warehouses.Any(p => p.Name == WarehouseName))
            {
                MessageBox.Show("A warehouse with the same name already exists.", "Duplicate Warehouse", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var warehouse = new Warehouse
            {
                Name = WarehouseName
            };

            _context.Warehouses.Add(warehouse);
            await _context.SaveChangesAsync();
            Warehouses.Add(warehouse);

            WarehouseName = string.Empty;
            MessageBox.Show("Warehouse added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task TryAddWarehouseAsync()
        {
            if (!CanAddWarehouse())
            {
                MessageBox.Show("You can't add warehouse. Check if the warehouse name is empty.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await AddWarehouseAsync();
        }

        private bool CanUpdateWarehouse()
        {
            return SelectedWarehouse != null;
        }

        private async Task UpdateWarehouseAsync()
        {
            if (SelectedWarehouse == null)
            {
                return;
            }

            if (MessageBox.Show("Are you sure that you want to update this warehouse?", "Confirm Update", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            SelectedWarehouse.Name = WarehouseName;

            _context.Warehouses.Update(SelectedWarehouse);
            await _context.SaveChangesAsync();
            await LoadWarehousesAsync();
            MessageBox.Show("Warehouse updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task TryUpdateWarehouseAsync()
        {
            if (!CanUpdateWarehouse())
            {
                MessageBox.Show("You can't update warehouse. Check if the warehouse is selected from the list.", "No Warehouse Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await UpdateWarehouseAsync();
        }

        private bool CanDeleteWarehouse()
        {
            return SelectedWarehouse != null;
        }

        private async Task DeleteWarehouseAsync()
        {
            if (SelectedWarehouse == null)
            {
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this warehouse?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            _context.Warehouses.Remove(SelectedWarehouse);
            await _context.SaveChangesAsync();
            Warehouses.Remove(SelectedWarehouse);

            SelectedWarehouse = null;
            WarehouseName = string.Empty;

            MessageBox.Show("Warehouse deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task TryDeleteWarehouseAsync()
        {
            if (!CanDeleteWarehouse())
            {
                MessageBox.Show("You can't delete warehouse. Check if the warehouse is selected from the list.", "No Warehouse Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await DeleteWarehouseAsync();
        }

        private async Task ShowLocationsAsync()
        {
            if (SelectedWarehouse == null)
            {
                MessageBox.Show("Please select a warehouse to view its locations.", "No Warehouse Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var locations = await _context.WarehouseLocations.Where(location => location.WarehouseId == SelectedWarehouse.Id).ToListAsync();

            if (locations.Count == 0)
            {
                MessageBox.Show("The selected warehouse has no locations.", "No Locations Found", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            SelectedWarehouseLocations = new ObservableCollection<WarehouseLocation>(locations);
        }
    }
}
