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

namespace InventoryManagement.WPF.ViewModels
{
    public abstract class SupplierViewModel : ViewModelBase
    {
        protected readonly InventoryManagementDbContext _context;
        public ICommand AddSupplierCommand { get; }
        public ICommand TryAddSupplierCommand { get; }
        public ICommand TryDeleteSupplierCommand { get; }
        public ICommand DeleteSupplierCommand { get; }
        public ICommand UpdateSupplierCommand { get; }
        public ICommand TryUpdateSupplierCommand { get; }

        protected SupplierViewModel()
        {
            _context = new InventoryManagementDbContext();
            AddSupplierCommand = new RelayCommand(async () => await AddSupplierAsync(), CanAddSupplier);
            UpdateSupplierCommand = new RelayCommand(async () => await UpdateSupplierAsync(), CanUpdateSupplier);
            DeleteSupplierCommand = new RelayCommand(async () => await DeleteSupplierAsync(), CanDeleteSupplier);
            TryAddSupplierCommand = new RelayCommand(async () => await TryAddSupplierAsync());
            TryDeleteSupplierCommand = new RelayCommand(async () => await TryDeleteSupplierAsync());
            TryUpdateSupplierCommand = new RelayCommand(async () => await TryUpdateSupplierAsync());
        }

        protected abstract Task AddSupplierAsync();
        protected abstract bool CanAddSupplier();
        protected abstract Task TryAddSupplierAsync();
        protected abstract Task TryDeleteSupplierAsync();
        protected abstract Task DeleteSupplierAsync();
        protected abstract bool CanDeleteSupplier();
        protected abstract bool CanUpdateSupplier();
        protected abstract Task TryUpdateSupplierAsync();
        protected abstract Task UpdateSupplierAsync();
    }

}
