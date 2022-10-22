namespace CloudWeather.Precipitation.DataAccess
{
    public class Precipitation
    {
        public Guid Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public decimal AmountIncher { get; set; }
        public string WeatherType { get; set; }
        public string ZipCode { get; set; }
    }
}