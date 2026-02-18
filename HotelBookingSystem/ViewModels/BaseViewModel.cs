using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace HotelBookingSystem.ViewModels
{
     /// <summary>
     /// Base ViewModel class implementing INotifyPropertyChanged
     /// All ViewModels inherit from this for data binding support
     /// </summary>
     public abstract class BaseViewModel : INotifyPropertyChanged
     {
          public event PropertyChangedEventHandler PropertyChanged;

          protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
          {
               PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
          }

          protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
          {
               if (Equals(field, value))
                    return false;

               field = value;
               OnPropertyChanged(propertyName);
               return true;
          }
     }
}