﻿using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Specifications;
using Microsoft.eShopWeb.Web.ViewModels;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ApplicationCore.Interfaces.ICurrencyService;
using ApplicationCore.Interfaces;
using System.Threading;
using Infrastructure.Services;
using System.Globalization;
using Microsoft.Extensions.Configuration;

namespace Microsoft.eShopWeb.Web.Services
{
    /// <summary>
    /// This is a UI-specific service so belongs in UI project. It does not contain any business logic and works
    /// with UI-specific types (view models and SelectListItem types).
    /// </summary>
    public class CatalogViewModelService : ICatalogViewModelService
    {
        private readonly ILogger<CatalogViewModelService> _logger;
        private readonly IAsyncRepository<CatalogItem> _itemRepository;
        private readonly IAsyncRepository<CatalogBrand> _brandRepository;
        private readonly IAsyncRepository<CatalogType> _typeRepository;
        private readonly IUriComposer _uriComposer;
        private readonly ICurrencyService _currencyService;
        private readonly IConfiguration _configuration;

        private Currency default_price_unit = Currency.USD; // TODO: Get from configuration
        private Currency user_price_unit;// TODO: Get FROM user Culture

        public CatalogViewModelService(
            ILoggerFactory loggerFactory,
            IAsyncRepository<CatalogItem> itemRepository,
            IAsyncRepository<CatalogBrand> brandRepository,
            IAsyncRepository<CatalogType> typeRepository,
            IUriComposer uriComposer,
            ICurrencyService currencyService,
            IConfiguration configuration)
        {
            _logger = loggerFactory.CreateLogger<CatalogViewModelService>();
            _itemRepository = itemRepository;
            _brandRepository = brandRepository;
            _typeRepository = typeRepository;
            _uriComposer = uriComposer;
            _currencyService = currencyService;
            _configuration = configuration;

            Enum.TryParse(_configuration["DefaultCulture"], true, out default_price_unit);
            user_price_unit = CultureServiceUser.FindCurrency(Currency default_price_unit);
        }

        private async Task<CatalogItemViewModel> CreateCatalogItemViewModel(CatalogItem catalogItem, CancellationToken cancellationToken = default(CancellationToken)){
            return new CatalogItemViewModel()
                {
                    Id = catalogItem.Id,
                    Name = catalogItem.Name,
                    PictureUri = catalogItem.PictureUri,
                    Price = await _currencyService.Convert(catalogItem.Price, default_price_unit, user_price_unit, cancellationToken),
                    ShowPrice = catalogItem.ShowPrice,
                    PriceUnit = user_price_unit,
                    PriceSymbol = CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol
                };
        }

        public async Task<CatalogIndexViewModel> GetCatalogItems(int pageIndex, int itemsPage, int? brandId, int? typeId, CancellationToken cancellationToken = default(CancellationToken))
        {
            _logger.LogInformation("GetCatalogItems called.");

            var filterSpecification = new CatalogFilterSpecification(brandId, typeId);
            var filterPaginatedSpecification =
                new CatalogFilterPaginatedSpecification(itemsPage * pageIndex, itemsPage, brandId, typeId);

            // the implementation below using ForEach and Count. We need a List.
            var itemsOnPage = await _itemRepository.ListAsync(filterPaginatedSpecification);
            var totalItems = await _itemRepository.CountAsync(filterSpecification);

            foreach (var itemOnPage in itemsOnPage)
            {
                itemOnPage.PictureUri = _uriComposer.ComposePicUri(itemOnPage.PictureUri);
            }

            var catalogItemsTask = await Task.WhenAll(itemsOnPage.Select(catalogItem => CreateCatalogItemViewModel(catalogItem, cancellationToken)));
            // em caso de algures haver um cancelamento o código para aqui e devolve um erro. Escusa de proceguir no processamento
            cancellationToken.ThrowIfCancellationRequested();

            var vm = new CatalogIndexViewModel()
            {
                CatalogItems = catalogItemsTask,
                Brands = await GetBrands(cancellationToken),
                Types = await GetTypes(cancellationToken),
                BrandFilterApplied = brandId ?? 0,
                TypesFilterApplied = typeId ?? 0,
                PaginationInfo = new PaginationInfoViewModel()
                {
                    ActualPage = pageIndex,
                    ItemsPerPage = itemsOnPage.Count,
                    TotalItems = totalItems,
                    TotalPages = int.Parse(Math.Ceiling(((decimal)totalItems / itemsPage)).ToString())
                }
            };

            vm.PaginationInfo.Next = (vm.PaginationInfo.ActualPage == vm.PaginationInfo.TotalPages - 1) ? "is-disabled" : "";
            vm.PaginationInfo.Previous = (vm.PaginationInfo.ActualPage == 0) ? "is-disabled" : "";

            return vm;
        }

        public async Task<IEnumerable<SelectListItem>> GetBrands(CancellationToken cancelationToken = default(CancellationToken))
        {
            _logger.LogInformation("GetBrands called.");
            var brands = await _brandRepository.ListAllAsync();
            cancelationToken.ThrowIfCancellationRequested();
            
            var items = new List<SelectListItem>
            {
                new SelectListItem() { Value = null, Text = "All", Selected = true }
            };
            foreach (CatalogBrand brand in brands)
            {
                items.Add(new SelectListItem() { Value = brand.Id.ToString(), Text = brand.Brand });
            }

            return items;
        }

        public async Task<IEnumerable<SelectListItem>> GetTypes(CancellationToken cancelationToken = default(CancellationToken))
        {
            _logger.LogInformation("GetTypes called.");
            var types = await _typeRepository.ListAllAsync();
            cancelationToken.ThrowIfCancellationRequested();

            var items = new List<SelectListItem>
            {
                new SelectListItem() { Value = null, Text = "All", Selected = true }
            };
            foreach (CatalogType type in types)
            {
                items.Add(new SelectListItem() { Value = type.Id.ToString(), Text = type.Type });
            }

            return items;
        }
    }
}
