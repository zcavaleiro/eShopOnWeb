﻿using ApplicationCore.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using static ApplicationCore.Interfaces.ICurrencyService;
using System.Json;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Infrastructure.Services
{
    public class ExchangeRates{
        public Dictionary<string, decimal> rates {get; set;}
        //public string base {get; set;}
        //public string date {get; set;}
    }

    public class CurrencyServiceExchangeRates : ICurrencyService
    {
        /*
        private readonly ICurrencyRatesService _currencyRatesService;

        public CurrencyServiceExchangeRates(ICurrencyRatesService currencyRatesService){
            _currencyRatesService = currencyRatesService;
        }
*/
        public async Task<decimal> Convert(decimal value, Currency source, Currency target, CancellationToken cancellationToken = default(CancellationToken))
        {
            // string jsonStr = await _currencyRatesService.Request(source);
            // Dictionary<string, decimal> rates = new Dictionary<string, decimal>();
            // rates.Add("USD", 1.00m);
            // rates.Add("EUR", 1.99m);
            // rates.Add("GBP", 1.55m);
            
            string jsonStr = "{\"rates\":{\"CAD\":1.3131853194,\"HKD\":7.7725419121,\"ISK\":124.5129134572,\"PHP\":50.8309922972,\"DKK\":6.7720888083,\"HUF\":304.4947893068,\"CZK\":22.8001812415,\"GBP\":0.7640507476,\"RON\":4.3315813321,\"SEK\":9.548074309,\"IDR\":13595.7498867241,\"INR\":71.2913457182,\"BRL\":4.1761667422,\"RUB\":61.7754417762,\"HRK\":6.7435432714,\"JPY\":109.6148618034,\"THB\":30.5600362483,\"CHF\":0.9707294971,\"EUR\":0.9062075215,\"MYR\":4.0649750793,\"BGN\":1.7723606706,\"TRY\":5.9391934753,\"CNY\":6.9333031264,\"NOK\":9.0054372451,\"NZD\":1.5119166289,\"ZAR\":14.3710919801,\"USD\":1.0,\"MXN\":18.7739012234,\"SGD\":1.3511554146,\"AUD\":1.46144087,\"ILS\":3.452650657,\"KRW\":1168.210240145,\"PLN\":3.8572723154},\"base\":\"USD\",\"date\":\"2020-01-24\"}";
            var jsonObject = new JsonObject();
            var obj = JsonConvert.DeserializeObject<dynamic>(jsonStr);
            var tt = obj["rates"];
            var exchangeRates = JsonConvert.DeserializeObject<ExchangeRates>(jsonStr);
            

            var rat = 1.03m;
            exchangeRates.rates.TryGetValue(target.ToString(), out rat);

            //throw new System.NotImplementedException();
            return await Task.FromResult(value * rat); // TODO: miss implementation
        }
    }
}
