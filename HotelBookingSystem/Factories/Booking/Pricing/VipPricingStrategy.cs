using HotelBookingSystem.Interfaces;

namespace HotelBookingSystem.Factories.Pricing
{
     public class VipPricingStrategy : IPricingStrategy
     {
          private const decimal DiscountRate = 0.20m;
          private const decimal FreeNightEvery = 5m;

          public decimal CalculateTotalPrice(decimal roomPrice, int nights)
          {
               int freeNights = (int)(nights / FreeNightEvery);
               int chargeableNights = nights - freeNights;
               decimal subtotal = roomPrice * chargeableNights;
               decimal discount = subtotal * DiscountRate;
               return subtotal - discount;
          }

          public string GetPricingDescription() =>
               "VIP pricing: 20% discount + 1 free night every 5 nights";
     }
}