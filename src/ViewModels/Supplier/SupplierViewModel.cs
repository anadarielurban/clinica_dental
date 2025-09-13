using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using DentalClinicWPF.Data;
using DentalClinicWPF.Models;
using DentalClinicWPF.ViewModels.Base;
using DentalClinicWPF.Views.Supplier;

namespace DentalClinicWPF.ViewModels.Supplier
{
    public class SupplierViewModel : BaseViewModel
    {
        private readonly DentalClinicContext _context;

        public ObservableCollection<Models.Supplier> Suppliers { get; private set; } = new();

        private Models.Supplier? _selectedSupplier;
        public Models.Supplier? SelectedSupplier
        {
            get => _selectedSupplier;
            set
            {
                _selectedSupplier = value;
                OnPropertyChanged();
                (EditSupplierCommand as RelayCommand)?.NotifyCanExecuteChanged();
            }
        }

        public string SearchText { get; set; } = string.Empty;

        // Właściwości do sortowania
        public ObservableCollection<string> SortOptions { get; } = new ObservableCollection<string>
        {
            "Company Name",
            "Contact Name",
            "Phone Number"
        };
        public string SelectedSortOption { get; set; }
        public ICommand SortCommand { get; }

        public ICommand AddSupplierCommand { get; }
        public ICommand EditSupplierCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand SearchCommand { get; }

        public SupplierViewModel(DentalClinicContext context)
        {
            _context = context;

            LoadSuppliers();

            AddSupplierCommand = new RelayCommand(OpenAddSupplier);
            EditSupplierCommand = new RelayCommand(OpenEditSupplier, CanEditSupplier);
            ReloadCommand = new RelayCommand(LoadSuppliers);
            SearchCommand = new RelayCommand(SearchSuppliers);
            SortCommand = new RelayCommand(SortSuppliers); // Komenda sortowania
        }

        private void LoadSuppliers()
        {
            Suppliers = new ObservableCollection<Models.Supplier>(_context.Supplier.ToList());
            OnPropertyChanged(nameof(Suppliers));
        }

        private bool CanEditSupplier() => SelectedSupplier != null;

        private void OpenAddSupplier()
        {
            var addSupplierView = new AddSupplierView
            {
                DataContext = new AddSupplierViewModel(_context)
            };

            if (addSupplierView.ShowDialog() == true)
            {
                LoadSuppliers();
            }
        }

        private void OpenEditSupplier()
        {
            if (SelectedSupplier == null) return;

            var editSupplierView = new EditSupplierView
            {
                DataContext = new EditSupplierViewModel(_context, SelectedSupplier)
            };

            if (editSupplierView.ShowDialog() == true)
            {
                LoadSuppliers();
            }
        }

        private void SearchSuppliers()
        {
            var filtered = _context.Supplier
                .Where(s => s.CompanyName.Contains(SearchText) || s.ContactName.Contains(SearchText))
                .ToList();

            Suppliers = new ObservableCollection<Models.Supplier>(filtered);
            OnPropertyChanged(nameof(Suppliers));
        }

        private void SortSuppliers()
        {
            if (string.IsNullOrEmpty(SelectedSortOption) || Suppliers == null)
                return;

            IEnumerable<Models.Supplier> sortedSuppliers = SelectedSortOption switch
            {
                "Company Name" => Suppliers.OrderBy(s => s.CompanyName),
                "Contact Name" => Suppliers.OrderBy(s => s.ContactName),
                "Phone Number" => Suppliers.OrderBy(s => s.PhoneNumber),
                _ => Suppliers
            };

            Suppliers = new ObservableCollection<Models.Supplier>(sortedSuppliers);
            OnPropertyChanged(nameof(Suppliers));
        }
    }
}
