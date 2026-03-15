using System;
using System.Collections.ObjectModel;
using System.Text;
using HotelBookingSystem.Composite;

namespace HotelBookingSystem.ViewModels
{
     public class RoomServiceController : BaseViewModel
     {
          private readonly RoomServiceCatalog _catalog;

          private RoomServiceComponent? _selectedCatalogItem;
          private RoomServiceComponent? _selectedOrderItem;
          private string _orderSummary = "No services ordered yet.";
          private decimal _orderTotal;

          public ObservableCollection<RoomServiceComponent> CatalogItems { get; } = new();
          public ObservableCollection<RoomServiceComponent> OrderedItems { get; } = new();

          public RoomServiceComponent? SelectedCatalogItem
          {
               get => _selectedCatalogItem;
               set => SetProperty(ref _selectedCatalogItem, value);
          }

          public RoomServiceComponent? SelectedOrderItem
          {
               get => _selectedOrderItem;
               set => SetProperty(ref _selectedOrderItem, value);
          }

          public string OrderSummary
          {
               get => _orderSummary;
               set => SetProperty(ref _orderSummary, value);
          }

          public decimal OrderTotal
          {
               get => _orderTotal;
               set => SetProperty(ref _orderTotal, value);
          }

          public event Action<string>? OnLog;

          public RoomServiceController()
          {
               _catalog = new RoomServiceCatalog();
               foreach (var item in _catalog.GetAll())
                    CatalogItems.Add(item);
          }

          public void AddToOrder()
          {
               if (SelectedCatalogItem == null) return;

               OrderedItems.Add(SelectedCatalogItem);
               string typeLabel = SelectedCatalogItem is RoomServicePackage ? "[Composite]" : "[Leaf]";
               OnLog?.Invoke($"[Composite] {typeLabel} '{SelectedCatalogItem.Name}' added → ${SelectedCatalogItem.GetPrice():F2}");

               RecalcOrder();
          }

          public void RemoveSelected()
          {
               if (SelectedOrderItem == null) return;
               OrderedItems.Remove(SelectedOrderItem);
               SelectedOrderItem = null;
               RecalcOrder();
          }

          public void ClearOrder()
          {
               OrderedItems.Clear();
               SelectedOrderItem = null;
               RecalcOrder();
               OnLog?.Invoke("[Composite] Order cleared.\n");
          }

          private void RecalcOrder()
          {
               if (OrderedItems.Count == 0)
               {
                    OrderTotal = 0;
                    OrderSummary = "No services ordered yet.";
                    return;
               }

               decimal total = 0;
               var sb = new StringBuilder();
               foreach (var item in OrderedItems)
               {
                    total += item.GetPrice();
                    sb.AppendLine(item.GetDescription());
                    sb.AppendLine();
               }

               OrderTotal = total;
               OrderSummary = sb.ToString().TrimEnd();
          }
     }
}