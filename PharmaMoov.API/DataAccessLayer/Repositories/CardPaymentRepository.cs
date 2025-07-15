using MangoPay.SDK;
using MangoPay.SDK.Core.Enumerations;
using MangoPay.SDK.Entities;
using MangoPay.SDK.Entities.GET;
using MangoPay.SDK.Entities.POST;
using MangoPay.SDK.Entities.PUT;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using PharmaMoov.API.Helpers;
using PharmaMoov.Models;
using PharmaMoov.Models.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PharmaMoov.API.DataAccessLayer.Repositories
{
    public class CardPaymentRepository : APIBaseRepo, ICardPaymentRepository
    {
        readonly APIDBContext DbContext;
        private APIConfigurationManager APIConfig { get; set; }
        ILoggerManager LogManager { get; }
        IPaymentRepository PaymentRepo { get; }
        private MangoPayApi MangoPayAPI = new MangoPayApi();

        public CardPaymentRepository(APIDBContext _dbCtxt, IPaymentRepository _payRepo, ILoggerManager _logManager, APIConfigurationManager _apiCon, IPromoRepository _promoRepo)
        {
            DbContext = _dbCtxt;
            LogManager = _logManager;
            APIConfig = _apiCon;
            PaymentRepo = _payRepo;

            // configure MangoPayApi
            MangoPayAPI.Config.ClientId = APIConfig.PaymentConfig.ClientId;
            MangoPayAPI.Config.ClientPassword = APIConfig.PaymentConfig.ApiKey;
            MangoPayAPI.Config.BaseUrl = APIConfig.PaymentConfig.BaseUrl;
        }

        public APIResponse NewCardRegistration(string token)
        {
            try
            {
                UserLoginTransaction ULT = DbContext.UserLoginTransactions.FirstOrDefault(ult => ult.Token == token);
                if (ULT == null)
                {
                    return new APIResponse
                    {
                        Message = "Erreur, invalid Login!",
                        Status = "Erreur",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
                else
                {
                    User cardOwner = DbContext.Users.FirstOrDefault(u => u.UserId == ULT.UserId);
                    if (cardOwner == null)
                    {
                        return new APIResponse
                        {
                            Message = "Erreur, invalid Login!",
                            Status = "Erreur",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                    else
                    {
                        MangoPayUser existingMangoPayUser = DbContext.MangoPayUsers.FirstOrDefault(mpu => mpu.PharmaMUserId == cardOwner.UserId);
                        if (existingMangoPayUser == null)
                        {
                            APIResponse GetMangoPayUser = PaymentRepo.CreateUserNaturalUserInMangoPay(cardOwner);
                            if (GetMangoPayUser.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                existingMangoPayUser = (MangoPayUser)GetMangoPayUser.Payload;
                            }
                            else
                            {
                                return GetMangoPayUser;
                            }
                        }

                        return new APIResponse
                        {
                            Message = "Succès initiating card registration",
                            Payload = InitiateCardRegistration(cardOwner.FirstName + " " + cardOwner.LastName,
                            existingMangoPayUser.ID,
                            existingMangoPayUser.PharmaMUserId),
                            Status = "Succès",
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo(ex.Message);
                LogManager.LogDebug(ex.StackTrace);
                return new APIResponse
                {
                    Message = "Erreur Payment! Try again later!",
                    ModelError = null,
                    Payload = ex.Data,
                    ResponseCode = null,
                    Status = "Erreur",
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }
        }

        public APIResponse GetAvailableCards(string token)
        {
            try
            {
                UserLoginTransaction ULT = DbContext.UserLoginTransactions.FirstOrDefault(ult => ult.Token == token);
                if (ULT == null)
                {
                    return new APIResponse
                    {
                        Message = "Erreur, invalid Login!",
                        Status = "Erreur",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
                else
                {
                    User cardOwner = DbContext.Users.FirstOrDefault(u => u.UserId == ULT.UserId);
                    if (cardOwner == null)
                    {
                        return new APIResponse
                        {
                            Message = "Erreur, invalid Login!",
                            Status = "Erreur",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                    else
                    {
                        MangoPayUser existingMangoPayUser = DbContext.MangoPayUsers.FirstOrDefault(mpu => mpu.PharmaMUserId == cardOwner.UserId);
                        if (existingMangoPayUser == null)
                        {
                            APIResponse GetMangoPayUser = PaymentRepo.CreateUserNaturalUserInMangoPay(cardOwner);
                            if (GetMangoPayUser.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                existingMangoPayUser = (MangoPayUser)GetMangoPayUser.Payload;
                            }
                            else
                            {
                                return GetMangoPayUser;
                            }
                        }

                        return new APIResponse
                        {
                            Message = "Récupération des cartes réussie !",
                            Payload = GetAllCardsFromMangoPay(existingMangoPayUser.ID),
                            Status = "Succès",
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo(ex.Message);
                LogManager.LogDebug(ex.StackTrace);
                return new APIResponse
                {
                    Message = "Erreur Payment! Try again later!",
                    ModelError = null,
                    Payload = ex.Data,
                    ResponseCode = null,
                    Status = "Erreur",
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }
        }

        public APIResponse UpdateCardRegistration(int RegistrationRecordID, string data)
        {
            try
            {
                LogManager.LogInfo("UpdateCardRegistration RegistrationRecordID:" + RegistrationRecordID.ToString() + " with data: " + data);
                //Get card registration details
                CardRegistration cardReg = DbContext.CardRegistrations.FirstOrDefault(cr => cr.RegistrationRecordID == RegistrationRecordID);
                if (cardReg != null)
                {
                    cardReg.RegistrationData = data;
                    CardRegistrationPutDTO newPutDTO = new CardRegistrationPutDTO
                    {
                        RegistrationData = "data=" + data,
                        Tag = "DataUpdate for cardId:" + cardReg.CardId
                    };
                    CardRegistrationDTO returnCardUpdateData = MangoPayAPI.CardRegistrations.Update(newPutDTO, cardReg.Id);
                    cardReg.UserId = returnCardUpdateData.UserId;
                    cardReg.AccessKey = returnCardUpdateData.AccessKey;
                    cardReg.PreregistrationData = returnCardUpdateData.PreregistrationData;
                    cardReg.CardRegistrationURL = returnCardUpdateData.CardRegistrationURL;
                    cardReg.CardId = returnCardUpdateData.CardId;
                    cardReg.RegistrationData = returnCardUpdateData.RegistrationData;
                    cardReg.ResultCode = returnCardUpdateData.ResultCode;
                    cardReg.Currency = returnCardUpdateData.Currency;
                    cardReg.Status = returnCardUpdateData.Status;
                    cardReg.CardType = returnCardUpdateData.CardType;
                    cardReg.Tag = "CardRead:" + cardReg.CardId;

                    DbContext.CardRegistrations.Update(cardReg);
                    DbContext.SaveChanges();
                    return new APIResponse
                    {
                        Message = "L'enregistrement de la carte a été mis à jour avec succès !",
                        Payload = returnCardUpdateData,
                        Status = "Succès",
                        StatusCode = System.Net.HttpStatusCode.OK
                    };
                }
                else
                {
                    return new APIResponse
                    {
                        Message = "L'ID d'enregistrement de la carte n'existe pas",
                        Payload = null,
                        Status = "failed",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo(ex.Message);
                LogManager.LogDebug(ex.StackTrace);
                return new APIResponse
                {
                    Message = "Erreur Payment! Try again later!",
                    ModelError = null,
                    Payload = ex.Message,
                    ResponseCode = null,
                    Status = "Erreur",
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }
        }

        public APIResponse DeactivateCard(string auth, string CardID)
        {
            try
            {
                UserLoginTransaction ULT = DbContext.UserLoginTransactions.FirstOrDefault(ult => ult.Token == auth);
                if (ULT == null)
                {
                    return new APIResponse
                    {
                        Message = "Erreur, invalid auth token!",
                        Status = "Erreur",
                        StatusCode = System.Net.HttpStatusCode.BadRequest
                    };
                }
                else
                {
                    User cardOwner = DbContext.Users.FirstOrDefault(u => u.UserId == ULT.UserId);
                    if (cardOwner == null)
                    {
                        return new APIResponse
                        {
                            Message = "Erreur, invalid auth token!",
                            Status = "Erreur",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        };
                    }
                    else
                    {
                        MangoPayUser existingMangoPayUser = DbContext.MangoPayUsers.FirstOrDefault(mpu => mpu.PharmaMUserId == cardOwner.UserId);
                        if (existingMangoPayUser == null)
                        {
                            APIResponse GetMangoPayUser = PaymentRepo.CreateUserNaturalUserInMangoPay(cardOwner);
                            if (GetMangoPayUser.StatusCode == System.Net.HttpStatusCode.OK)
                            {
                                existingMangoPayUser = (MangoPayUser)GetMangoPayUser.Payload;
                            }
                            else
                            {
                                return GetMangoPayUser;
                            }
                        }

                        return new APIResponse
                        {
                            Message = "Deactivate Card Succès Full!",
                            Payload = DeactivateCard(CardID, existingMangoPayUser),
                            Status = "Succès",
                            StatusCode = System.Net.HttpStatusCode.OK
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.LogInfo(ex.Message);
                LogManager.LogDebug(ex.StackTrace);
                return new APIResponse
                {
                    Message = "Erreur Payment! Try again later!",
                    ModelError = null,
                    Payload = ex.Data,
                    ResponseCode = null,
                    Status = "Erreur",
                    StatusCode = System.Net.HttpStatusCode.BadRequest
                };
            }
        }

        CardRegistration DeactivateCard(string CardID, MangoPayUser mPayUser)
        {
            try
            {
                LogManager.LogInfo("DeactivateCard for UserId:" + mPayUser.PharmaMUserId.ToString() +
                    " with MangoPayUserID " + mPayUser.ID +
                    " and cardID:" + CardID);
                CardPutDTO NewCardPut = new CardPutDTO
                {
                    Active = false
                };
                CardDTO returnDeactivateDate = MangoPayAPI.Cards.Update(NewCardPut, CardID);
                CardRegistration existingRegistration = DbContext.CardRegistrations.FirstOrDefault(cr => cr.CardId == CardID && cr.UserId == mPayUser.ID);
                DbContext.CardRegistrations.Remove(existingRegistration);
                DbContext.SaveChanges();
                return existingRegistration;
            }
            catch (Exception ex)
            {
                LogManager.LogInfo(ex.Message);
                LogManager.LogDebug(ex.StackTrace);
                return null;
            }
        }

        List<CardDTO> GetAllCardsFromMangoPay(string userID)
        {
            List<CardDTO> ReturnIngCardsList = new List<CardDTO>();
            Pagination paging = new Pagination
            {
                ItemsPerPage = 100,
                Page = 1,
            };
            ListPaginated<CardDTO> mpayUserCardsList = MangoPayAPI.Users.GetCards(userID, paging);
            for (int pnum = 1; pnum <= mpayUserCardsList.TotalPages; pnum++)
            {
                foreach (CardDTO card in mpayUserCardsList)
                {
                    if (card.Active == true)
                    {
                        ReturnIngCardsList.Add(card);
                    }
                }
                paging.Page = pnum + 1;
                mpayUserCardsList = MangoPayAPI.Users.GetCards(userID, paging);
            }

            return ReturnIngCardsList;
        }

        CardRegistration InitiateCardRegistration(string CustomerName, string MangoPayUserID, Guid UserID)
        {
            try
            {
                LogManager.LogInfo("New Card Registration for UserId:" + UserID.ToString() + " with MangoPayUserID " + MangoPayUserID);
                // build the registration request dto
                CardRegistrationPostDTO NewCardRegistrationPost = new CardRegistrationPostDTO(MangoPayUserID, CurrencyIso.EUR);
                NewCardRegistrationPost.Tag = CustomerName + "'s_card_" + UserID.ToString();
                // request card registration
                CardRegistrationDTO newCard = MangoPayAPI.CardRegistrations.Create(NewCardRegistrationPost);
                CardRegistration CardRegistrationRecord = new CardRegistration
                {
                    AccessKey = newCard.AccessKey,
                    CardId = newCard.CardId,
                    CardRegistrationURL = newCard.CardRegistrationURL,
                    CardType = newCard.CardType,
                    CreatedBy = UserID,
                    ApplicationUserID = UserID,
                    CreationDate = newCard.CreationDate,
                    Currency = newCard.Currency,
                    DateCreated = DateTime.Now,
                    PreregistrationData = newCard.PreregistrationData,
                    RegistrationData = newCard.RegistrationData,
                    Id = newCard.Id,
                    ResultCode = newCard.ResultCode,
                    Status = newCard.Status,
                    Tag = newCard.Tag,
                    UserId = newCard.UserId
                };
                DbContext.CardRegistrations.Add(CardRegistrationRecord);
                DbContext.SaveChanges();
                return CardRegistrationRecord;
            }
            catch (Exception ex)
            {
                LogManager.LogInfo(ex.Message);
                LogManager.LogDebug(ex.StackTrace);
                return null;
            }
        }
    }
}
