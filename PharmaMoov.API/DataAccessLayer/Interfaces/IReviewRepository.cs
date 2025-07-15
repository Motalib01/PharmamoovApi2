using PharmaMoov.Models;
using PharmaMoov.Models.Review;
using System;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface IReviewRepository
    {
        APIResponse AddShopReview(string _auth, ShopReviewRatingDTO _review);
        APIResponse GetShopReviews(Guid _shop, int _review);
        APIResponse SetShopFavorite(string _auth, ShopFavorite _favorite);
    }
}
