using PharmaMoov.Models;
using PharmaMoov.Models.Admin;
using PharmaMoov.Models.Shop;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface IFAQsRepository
    {
        APIResponse AddFAQuestion(string _auth, ShopFAQdto _fAQ);
        APIResponse EditFAQuestion(string _auth, ShopFAQdto _fAQ);
        APIResponse GetFAQs();
        APIResponse GetFAQsList(string _auth, int _id);
        APIResponse ChangeFAQStatus(ChangeRecordStatus _faqStat);
        APIResponse GetTermsAndConditions();
        APIResponse SaveTermsAndConditions(string Authorization, TermsAndCondition _termAndCondition);
        APIResponse GetPrivacyPolicy();
        APIResponse SavePrivacyPolicy(string Authorization, PrivacyPolicy _privPolicy);
        APIResponse ShopGetTermsAndConditions();
        APIResponse ShopSaveTermsAndConditions(string Authorization, ShopTermsAndCondition _termAndCondition);
    }
}
