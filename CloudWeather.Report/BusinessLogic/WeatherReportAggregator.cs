using System.Text.Json;
using CloudWeather.Report.Config;
using CloudWeather.Report.DataAccess;
using CloudWeather.Report.Models;
using Microsoft.Extensions.Options;

namespace CloudWeather.Report.BusinessLogic
{
    public class WeatherReportAggregator : IWeatherReportAggregator 
    {
        private readonly IHttpClientFactory _http;
        private readonly ILogger<WeatherReportAggregator> _logger;
        private readonly WeatherDataConfig _weatherDataConfig;
        private readonly WeatherReportDbContext _db; 

        public WeatherReportAggregator(IHttpClientFactory http
            , ILogger<WeatherReportAggregator> logger
            , IOptions<WeatherDataConfig> weatherDataConfig
            , WeatherReportDbContext db) 
        {
            _http = http;
            _logger = logger;
            _weatherDataConfig = weatherDataConfig.Value;
            _db = db;
        }

        public async Task<WeatherReport> BuildReport(string zip, int days)
        {
            var httpClient = _http.CreateClient();

            var precipData = await FetchPrecipitationData(httpClient, zip, days);
            var totalSnow = GetTotalSnow(precipData);
            var totalRain = GetTotalRain(precipData);
            _logger.LogInformation(
                $"zip: {zip} over last {days} days: " +
                $"total snow: {totalSnow}, rain: {totalRain}"
            );

            var tempData = await FetchTemperatureData(httpClient, zip, days);
            var averageHighTemp = tempData.Average(t => t.TempHighF);
            var averageLowTemp = tempData.Average(t => t.TempLowF);
            _logger.LogInformation(
                $"zip: {zip} over last {days} days: " +
                $"lo temp: {averageLowTemp}, hi temp: {averageHighTemp}"
            );

            var weatherReport = new WeatherReport {
                AverageHighF = Math.Round(averageHighTemp, 1),
                AverageLowF = Math.Round(averageLowTemp),
                RainfallTotalInches = totalRain,
                SnowTotalInches = totalSnow,
                ZipCode = zip,
                CreatedOn = DateTime.UtcNow
            };

            //TODO: User 'cached' weather reports instead of making round trips wen possible?
            await _db.AddAsync(weatherReport);
            await _db.SaveChangesAsync();

            return weatherReport;
        }

        private async Task<List<PrecipitationModel>> FetchPrecipitationData(HttpClient httpClient, string zip, int days)
        {
            var endpoint = BuildPrecipitationServiceEndpoint(zip, days);
            var precipitationRecords = await httpClient.GetAsync(endpoint);            
            var precipitationData = await precipitationRecords
                .Content
                .ReadFromJsonAsync<List<PrecipitationModel>>(GetJsonSerializerOptions());
            return precipitationData ?? new List<PrecipitationModel>();
        }

        private async Task<List<TemperatureModel>> FetchTemperatureData(HttpClient httpClient, string zip, int days)
        {
            var endpoint = BuildTemperatureServiceEndpoint(zip, days);
            var temperatureRecords = await httpClient.GetAsync(endpoint);
            var temperatureData = await temperatureRecords
                .Content
                .ReadFromJsonAsync<List<TemperatureModel>>(GetJsonSerializerOptions());
            return temperatureData ?? new List<TemperatureModel>();
        }

        private static decimal GetTotalSnow(IEnumerable<PrecipitationModel> precipData)
        {
            var totalSnow = precipData
                .Where(p => p.WeatherType == "snow")
                .Sum(p => p.AmountInches);
            return Math.Round(totalSnow, 1);
        }

        private static decimal GetTotalRain(IEnumerable<PrecipitationModel> precipData)
        {
            var totalRain = precipData
                .Where(p => p.WeatherType == "rain")
                .Sum(p => p.AmountInches);
            return Math.Round(totalRain, 1);
        }

        private JsonSerializerOptions GetJsonSerializerOptions() 
        {
            return new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        private string BuildTemperatureServiceEndpoint(string zip, int days)
        {
            var tempServiceProtocol = _weatherDataConfig.TempDataProtocol;
            var tempServiceHost = _weatherDataConfig.TempDataHost;
            var tempServicePort = _weatherDataConfig.TempDataPort;
            return $"{tempServiceProtocol}://{tempServiceHost}:{tempServicePort}/observation/{zip}?days={days}";
        }

        private string BuildPrecipitationServiceEndpoint(string zip, int days)
        {
            var precipServiceProtocol = _weatherDataConfig.PrecipDataProtocol;
            var precipServiceHost = _weatherDataConfig.PrecipDataHost;
            var precipServicePort = _weatherDataConfig.PrecipDataPort;
            return $"{precipServiceProtocol}://{precipServiceHost}:{precipServicePort}/observation/{zip}?days={days}";
        }
    }
}