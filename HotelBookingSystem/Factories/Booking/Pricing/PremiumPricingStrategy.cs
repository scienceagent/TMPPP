using HotelBookingSystem.Interfaces;

namespace HotelBookingSystem.Factories.Pricing
{
     public class PremiumPricingStrategy : IPricingStrategy
     {
          private const decimal DiscountRate = 0.10m;

          public decimal CalculateTotalPrice(decimal roomPrice, int nights)
          {
               decimal subtotal = roomPrice * nights;
               decimal discount = subtotal * DiscountRate;
               return subtotal - discount;
          }

          public string GetPricingDescription() => "Premium pricing: 10% discount applied";
     }
}