using HotelBookingSystem.Interfaces;

namespace HotelBookingSystem.Factories.Pricing
{
     public class StandardPricingStrategy : IPricingStrategy
     {
          public decimal CalculateTotalPrice(decimal roomPrice, int nights)
          {
               return roomPrice * nights;
          }

          public string GetPricingDescription() => "Standard pricing: base rate per night";
     }
}