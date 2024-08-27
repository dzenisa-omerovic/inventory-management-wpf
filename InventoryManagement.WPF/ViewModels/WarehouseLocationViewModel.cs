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
    public class WarehouseLocationViewModel : ViewModelBase
    {
        private readonly InventoryManagementDbContext _context;
        private WarehouseLocation _selectedWarehouseLocation;
        private string _warehouseLocationName;
        private Warehouse _selectedWarehouse;
        public ICommand AddWarehouseLocationCommand { get; }
        public ICommand UpdateWarehouseLocationCommand { get; }
        public ICommand DeleteWarehouseLocationCommand { get; }
        public ICommand TryAddWarehouseLocationCommand { get; }
        public ICommand TryUpdateWarehouseLocationCommand { get; }
        public ICommand TryDeleteWarehouseLocationCommand { get; }
        public ObservableCollection<WarehouseLocation> WarehouseLocations { get; set; } = new ObservableCollection<WarehouseLocation>();
        private ObservableCollection<Warehouse> _warehouses { get; set; }

        public WarehouseLocationViewModel()
        {
            _context = new InventoryManagementDbContext();
            Load();

            AddWarehouseLocationCommand = new RelayCommand(async () => await AddWarehouseLocationAsync(), CanAddWarehouseLocation);
            UpdateWarehouseLocationCommand = new RelayCommand(async () => await UpdateWarehouseLocationAsync(), CanUpdateWarehouseLocation);
            DeleteWarehouseLocationCommand = new RelayCommand(async () => await DeleteWarehouseLocationAsync(), CanDeleteWarehouseLocation);

            TryAddWarehouseLocationCommand = new RelayCommand(async () => await TryAddWarehouseLocationAsync());
            TryUpdateWarehouseLocationCommand = new RelayCommand(async () => await TryUpdateWarehouseLocationAsync());
            TryDeleteWarehouseLocationCommand = new RelayCommand(async () => await TryDeleteWarehouseLocationAsync());

        }

        public async void Load()
        {
            await LoadWarehouseLocationsAsync();
        }
        public ObservableCollection<Warehouse> Warehouses
        {
            get
            {
                var warehouses = _context.Warehouses.ToList();
                return new ObservableCollection<Warehouse>(warehouses);
            }
        }
        public WarehouseLocation SelectedWarehouseLocation
        {
            get => _selectedWarehouseLocation;
            set
            {
                _selectedWarehouseLocation = value;
                if (_selectedWarehouseLocation != null)
                {
                    WarehouseLocationName = _selectedWarehouseLocation.Name;
                    SelectedWarehouse = _selectedWarehouseLocation.Warehouse;
                }
                OnPropertyChanged(nameof(SelectedWarehouseLocation));
                ((RelayCommand)UpdateWarehouseLocationCommand).RaiseCanExecuteChanged();
                ((RelayCommand)DeleteWarehouseLocationCommand).RaiseCanExecuteChanged();
                OnPropertyChanged(nameof(IsUpdateEnabled));
            }
        }

        public string WarehouseLocationName
        {
            get => _warehouseLocationName;
            set
            {
                _warehouseLocationName = value;
                OnPropertyChanged(nameof(WarehouseLocationName));
                ((RelayCommand)AddWarehouseLocationCommand).RaiseCanExecuteChanged();
                ((RelayCommand)UpdateWarehouseLocationCommand).RaiseCanExecuteChanged();
            }
        }

        public Warehouse SelectedWarehouse
        {
            get => _selectedWarehouse;
            set
            {
                _selectedWarehouse = value;
                OnPropertyChanged(nameof(SelectedWarehouse));
            }
        }

        public bool IsUpdateEnabled => SelectedWarehouseLocation != null;

        private async Task LoadWarehouseLocationsAsync()
        {
            var warehouseLocations = await _context.WarehouseLocations.Include(wl => wl.Warehouse).ToListAsync();
            WarehouseLocations.Clear();
            foreach (var warehouseLocation in warehouseLocations)
            {
                WarehouseLocations.Add(warehouseLocation);
            }
        }

        private bool CanAddWarehouseLocation()
        {
            return !string.IsNullOrWhiteSpace(WarehouseLocationName) && SelectedWarehouse != null;
        }

        private async Task AddWarehouseLocationAsync()
        {
            if (WarehouseLocations.Any(p => p.Name == WarehouseLocationName))
            {
                MessageBox.Show("A warehouse location with the same name already exists.", "Duplicate Warehouse Location", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var warehouseLocation = new WarehouseLocation
            {
                Name = WarehouseLocationName,
                WarehouseId = SelectedWarehouse.Id
            };

            _context.WarehouseLocations.Add(warehouseLocation);
            await _context.SaveChangesAsync();
            WarehouseLocations.Add(warehouseLocation);

            WarehouseLocationName = string.Empty;
            MessageBox.Show("Warehouse location added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task TryAddWarehouseLocationAsync()
        {
            if (!CanAddWarehouseLocation())
            {
                MessageBox.Show("You can't add warehouse location. Check if the warehouse location name is empty or a warehouse is selected.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await AddWarehouseLocationAsync();
        }

        private bool CanUpdateWarehouseLocation()
        {
            return SelectedWarehouseLocation != null;
        }

        private async Task UpdateWarehouseLocationAsync()
        {
            if (SelectedWarehouseLocation == null)
            {
                return;
            }

            bool isWarehouseLocationInWarehouse = await _context.WarehouseProducts.AnyAsync(wp => wp.WarehouseLocationId == SelectedWarehouseLocation.Id);

            if (isWarehouseLocationInWarehouse)
            {
                MessageBox.Show("You can update only warehouse locations which are not added to warehouse.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure that you want to update this warehouse location?", "Confirm Update", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            SelectedWarehouseLocation.Name = WarehouseLocationName;
            SelectedWarehouseLocation.WarehouseId = SelectedWarehouse.Id;

            _context.WarehouseLocations.Update(SelectedWarehouseLocation);
            await _context.SaveChangesAsync();
            await LoadWarehouseLocationsAsync();
            MessageBox.Show("Warehouse location updated successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task TryUpdateWarehouseLocationAsync()
        {
            if (!CanUpdateWarehouseLocation())
            {
                MessageBox.Show("You can't update warehouse location. Check if the warehouse location is selected from the list.", "No Warehouse Location Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await UpdateWarehouseLocationAsync();
        }

        private bool CanDeleteWarehouseLocation()
        {
            return SelectedWarehouseLocation != null;
        }

        private async Task DeleteWarehouseLocationAsync()
        {
            if (SelectedWarehouseLocation == null)
            {
                return;
            }

            bool isWarehouseLocationInWarehouse = await _context.WarehouseProducts.AnyAsync(wp => wp.WarehouseLocationId == SelectedWarehouseLocation.Id);

            if (isWarehouseLocationInWarehouse)
            {
                MessageBox.Show("You can delete only warehouse locations which are not added to warehouse.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Are you sure you want to delete this warehouse location?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.No)
            {
                return;
            }

            _context.WarehouseLocations.Remove(SelectedWarehouseLocation);
            await _context.SaveChangesAsync();
            WarehouseLocations.Remove(SelectedWarehouseLocation);

            WarehouseLocationName = string.Empty;
            MessageBox.Show("Warehouse location deleted successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task TryDeleteWarehouseLocationAsync()
        {
            if (!CanDeleteWarehouseLocation())
            {
                MessageBox.Show("You can't delete warehouse location. Check if the warehouse location is selected from the list.", "No Warehouse Location Selected", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await DeleteWarehouseLocationAsync();
        }
    }
}