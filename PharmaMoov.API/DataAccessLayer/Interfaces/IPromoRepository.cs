using PharmaMoov.Models;
using PharmaMoov.Models.Admin;
using PharmaMoov.Models.Campaign;
using PharmaMoov.Models.Promo;
using PharmaMoov.Models.Shop;
using System;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface IPromoRepository
    {
        APIResponse AddPromo(string _auth, PromoDTO _promo);
        APIResponse EditPromo(string _auth, PromoDTO _promo);
        APIResponse GetPromoValue(string _auth, string _code);
        APIResponse GetPromoList(int _id);
        APIResponse ChangePromoStatus(ChangeRecordStatus _admin);
        APIResponse GetCampaignByShopId(string _auth, Guid _cId);
        APIResponse AddBanner(string _auth, Campaign _campaign);
        APIResponse GetShopBanners();
        APIResponse GetCampaignByBannerId(string _auth, int _id);
    }
}
