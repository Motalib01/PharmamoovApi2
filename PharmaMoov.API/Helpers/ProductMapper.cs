using PharmaMoov.Models.Product;
using System;

public static class ProductMapper
{
	public static Product MapFromExternal(ExternalProduct externalProduct, Guid shopId)
	{
		if (externalProduct == null)
			throw new ArgumentNullException(nameof(externalProduct));

		return new Product
		{
			ProductRecordId = 0,
			ProductId = externalProduct.ProductId != Guid.Empty ? externalProduct.ProductId : Guid.NewGuid(),
			ShopId = shopId,
			ProductDesc = externalProduct.ProductDesc,
			ProductIcon = externalProduct.ProductIcon,
			ProductName = externalProduct.ProductName,
			ProductCategoryId = externalProduct.ProductCategoryId,
			ProductPrice = externalProduct.ProductPrice,
			ProductPricePerKG = externalProduct.ProductPricePerKG,
			ProductTaxValue = externalProduct.ProductTaxValue,
			ProductSKUCode = externalProduct.ProductSKUCode,
			ProductSAPCode = externalProduct.ProductSAPCode,
			IsSale = externalProduct.IsSale,
			IsUnsold = externalProduct.IsUnsold,
			IsFragile = externalProduct.IsFragile,
			ProductStatus = externalProduct.ProductStatus,
			IsProductFeature = externalProduct.IsProductFeature,
			SalePrice = externalProduct.SalePrice,
			PriceHolder = externalProduct.PriceHolder
		};
	}
}