using CloudWeather.Report.DataAccess;

namespace CloudWeather.Report.BusinessLogic
{
    /// <summary>
    /// Aggregates data from multiple external sources to bvuild a weather report
    /// </summary>
    public interface IWeatherReportAggregator
    {
        /// <summary>
        /// Builds and returns a Weather Report.
        /// Persists WeeklyWeatherReport data.
        /// </summary>
        /// <param nome="zip"></param>
        /// <param nome="days"></param>
        ///<returns></returns>
        Task<WeatherReport> BuildReport(string zip, int days);
    }
}