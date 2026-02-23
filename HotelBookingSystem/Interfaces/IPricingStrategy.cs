namespace HotelBookingSystem.Interfaces
{
     public interface IPricingStrategy
     {
          decimal CalculateTotalPrice(decimal roomPrice, int nights);
          string GetPricingDescription();
     }
}