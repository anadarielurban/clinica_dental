// ViewModels/Common/TabItemViewModel.cs
using System;
using System.Windows.Input;
using DentalClinicWPF.ViewModels.Base;
using CommunityToolkit.Mvvm.Input;

namespace DentalClinicWPF.Helpers
{
    public class TabItemViewModel : BaseViewModel
    {
        private string _header = string.Empty;
        public string Header
        {
            get => _header;
            set { _header = value; OnPropertyChanged(); }
        }

        private BaseViewModel _content;
        public BaseViewModel Content
        {
            get => _content;
            set { _content = value; OnPropertyChanged(); }
        }

        private ICommand _closeCommand;
        public ICommand CloseCommand
        {
            get => _closeCommand;
            set { _closeCommand = value; OnPropertyChanged(); }
        }

        public TabItemViewModel(string header, BaseViewModel content, Action<TabItemViewModel> closeAction)
        {
            Header = header ?? throw new ArgumentNullException(nameof(header));
            Content = content ?? throw new ArgumentNullException(nameof(content));
            CloseCommand = new RelayCommand(() => closeAction(this));
        }
    }
}