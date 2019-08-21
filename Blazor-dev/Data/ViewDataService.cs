using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authorization;

namespace WebApplication1.Data
{
    [Authorize]
    public class ViewDataService : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _caption;

        public string Caption
        {
            get => _caption;
            set
            {
                _caption = value;
                OnPropertyChanged();
            }
        }
    }
}