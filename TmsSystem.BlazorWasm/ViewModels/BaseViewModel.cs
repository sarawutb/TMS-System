using System.ComponentModel;
using System.Runtime.CompilerServices;
using TmsSystem.BlazorWasm.Services;

namespace TmsSystem.BlazorWasm.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        private readonly LoadingService? _loadingService;
        public BaseViewModel(LoadingService? loadingService)
        {
            _loadingService = loadingService;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private string? _loadingMessage;
        public string? LoadingMessage
        {
            get => _loadingMessage;
            set => SetProperty(ref _loadingMessage, value);
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        protected bool SetProperty<T>(
            ref T backingStore,
            T value,
            [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged(
            [CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(
                this,
                new PropertyChangedEventArgs(propertyName));
        }

        public virtual void ShowLoading(string message = "Loading...")
        {
            IsBusy = true;
            IsLoading = true;
            LoadingMessage = message;
            _loadingService?.Show(LoadingMessage);
        }

        public virtual void HideLoading()
        {
            IsBusy = false;
            IsLoading = false;
            LoadingMessage = null;
            _loadingService?.Hide();
        }

        public virtual void SetError(string message)
        {
            ErrorMessage = message;
        }

        public virtual void ClearError()
        {
            ErrorMessage = null;
        }
    }
}
