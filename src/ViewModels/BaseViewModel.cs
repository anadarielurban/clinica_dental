using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace DentalClinicWPF.ViewModels.Base
{
    public class BaseViewModel : INotifyPropertyChanged, INotifyDataErrorInfo
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private readonly Dictionary<string, List<string>> _errors = new();

        public bool HasErrors => _errors.Any();

        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            ValidateProperty(propertyName);

            return true;
        }

        protected virtual void ValidateProperty(string? propertyName)
        {
            ClearErrors(propertyName);
            // Derived classes override for validation logic.
        }

        protected void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName))
                _errors[propertyName] = new List<string>();

            _errors[propertyName].Add(error);
            OnErrorsChanged(propertyName);
        }

        protected void ClearErrors(string? propertyName)
        {
            if (!string.IsNullOrEmpty(propertyName) && _errors.Remove(propertyName))
                OnErrorsChanged(propertyName);
        }

        private void OnErrorsChanged(string propertyName)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public IEnumerable GetErrors(string? propertyName)
        {
            return propertyName == null ? _errors.Values : _errors.GetValueOrDefault(propertyName, new List<string>());
        }

        public ObservableCollection<T> ApplySearchFilter<T>(ObservableCollection<T> collection, Func<T, bool> predicate)
        {
            return new ObservableCollection<T>(collection.Where(predicate));
        }

        public ObservableCollection<T> ApplySort<T>(ObservableCollection<T> collection, Func<T, object> sortKeySelector)
        {
            return new ObservableCollection<T>(collection.OrderBy(sortKeySelector));
        }
    }
}
