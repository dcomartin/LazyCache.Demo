using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LazyCache.Demo
{
    class Program
    {
        private static IAppCache _cache;
        private static HttpClient _httpClient;

        static void Main(string[] args)
        {
            _cache = new CachingService();

            _httpClient = new HttpClient();

            while (true)
            {
                Console.WriteLine("Please enter a Date [yyyy-mm-dd]: ");
                var dateStr = Console.ReadLine();
                DateTime date;

                if (DateTime.TryParse(dateStr, out date) == false)
                {
                    Console.WriteLine($"{DateTime.UtcNow}: Invalid Date.");
                    continue;
                }

                if (date.Date > DateTime.UtcNow.Date)
                {
                    Console.WriteLine($"{DateTime.UtcNow}: Cannot specify a date in the future.");
                    continue;
                }

                var exchange = GetCurrencyRate(date);
                decimal canadian;
                exchange.Rates.TryGetValue("CAD", out canadian);
                Console.WriteLine($"{DateTime.UtcNow}: USD to CAD = {canadian}");

            }
        }

        private static CurrencyExchange GetCurrencyRate(DateTime date)
        {
            var key = date.Date.ToString("yyyy-MM-dd");

            return _cache.GetOrAdd(key, () =>
            {
                Console.WriteLine($"{DateTime.UtcNow}: Fetching from service");

                var response = _httpClient.GetAsync("http://api.fixer.io/" + key + "?base=USD").Result;
                var json = response.Content.ReadAsStringAsync().Result;

                return JsonConvert.DeserializeObject<CurrencyExchange>(json);
            }, new TimeSpan(0, 0, 0, 30));
        }
    }

    public class CurrencyExchange
    {
        public string Base { get; set; }
        public DateTime Date { get; set; }
        public Dictionary<string, decimal> Rates { get; set; }
    }
}
