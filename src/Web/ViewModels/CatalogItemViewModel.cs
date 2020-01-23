﻿using static ApplicationCore.Interfaces.ICurrencyService;

namespace Microsoft.eShopWeb.Web.ViewModels
{
    public class CatalogItemViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PictureUri { get; set; }
        public decimal Price { get; set; }
        public bool ShowPrice { get; set; }
        public Currency PriceUnit { get; set; }
        public string PriceSymbol { get; set; }
    }
}
