using PharmaMoov.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface ICardPaymentRepository
    {
        APIResponse NewCardRegistration(string token);
        APIResponse GetAvailableCards(string token);
        APIResponse UpdateCardRegistration(int RegistrationRecordID, string data);
        APIResponse DeactivateCard(string auth, string CardID);
    }
}
