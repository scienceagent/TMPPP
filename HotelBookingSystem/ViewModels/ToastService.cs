using System;
using System.Windows.Threading;

namespace HotelBookingSystem.ViewModels
{
     public enum ToastKind { Success, Error, Info, Warning }

     public class ToastNotification : BaseViewModel
     {
          public string Message { get; }
          public string Title { get; }
          public ToastKind Kind { get; }

          private bool _isVisible;
          public bool IsVisible
          {
               get => _isVisible;
               set => SetProperty(ref _isVisible, value);
          }

          public ToastNotification(string title, string message, ToastKind kind)
          {
               Title = title;
               Message = message;
               Kind = kind;
               IsVisible = true;
          }
     }

     // Singleton service — MainViewModel holds one instance, all controllers fire into it
     public class ToastService : BaseViewModel
     {
          private static readonly Lazy<ToastService> _instance =
              new Lazy<ToastService>(() => new ToastService());

          public static ToastService Instance => _instance.Value;

          private ToastNotification? _current;
          public ToastNotification? Current
          {
               get => _current;
               private set => SetProperty(ref _current, value);
          }

          private bool _isShowing;
          public bool IsShowing
          {
               get => _isShowing;
               set => SetProperty(ref _isShowing, value);
          }

          private DispatcherTimer? _timer;

          private ToastService() { }

          public void Show(string title, string message, ToastKind kind = ToastKind.Success,
                           int durationMs = 3200)
          {
               // Cancel any in-flight timer
               _timer?.Stop();

               Current = new ToastNotification(title, message, kind);
               IsShowing = true;

               _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(durationMs) };
               _timer.Tick += (_, _) =>
               {
                    _timer.Stop();
                    IsShowing = false;
               };
               _timer.Start();
          }

          public void Dismiss()
          {
               _timer?.Stop();
               IsShowing = false;
          }
     }
}