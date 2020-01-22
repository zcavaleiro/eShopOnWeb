﻿using ApplicationCore.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class CurrencyServiceStatic : ICurrencyService
    {
        public Task<decimal> Convert(decimal value, ICurrencyService.Currency source, ICurrencyService.Currency target, CancellationToken cancellationToken = default(CancellationToken))
        {
            //throw new System.NotImplementedException();
            return Task.FromResult(value*1.03m);
        }
    }
}