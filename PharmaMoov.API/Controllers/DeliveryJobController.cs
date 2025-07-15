using PharmaMoov.Models;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaMoov.API.Helpers;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PharmaMoov.API.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/DeliveryJob")]
    public class DeliveryJobController : APIBaseController
    {
        IDeliveryJobRepository DeliveryJobRepo { get; }
        ILoggerManager LogManager { get;}
        IMainHttpClient MainHttpClient { get; }
        APIConfigurationManager MConf { get; }

        public DeliveryJobController(IDeliveryJobRepository _DeliveryJobRepo, IMainHttpClient _mhttpc, APIConfigurationManager _conf, ILoggerManager _logManager)
        {
            DeliveryJobRepo = _DeliveryJobRepo;
            MainHttpClient = _mhttpc;
            MConf = _conf;
            LogManager = _logManager;
        }

        //[AllowAnonymous]
        //[HttpPost("Hook")]
        //public IActionResult DeliveryJobWebhook([FromBody] WebhookResponse model)
        //{
        //    LogManager.LogInfo("DeliveryJobWebhook");
        //    if (model.Type == "update")
        //    {
        //        LogManager.LogInfo("Object from Delivery Service WEBHOOK:");
        //        LogManager.LogDebugObject(model);
        //    }
        //    try
        //    {
        //        if (model.Event == "delivery" && model.Type == "update")
        //        {
        //            DeliveryJobRepo.UpdateDeliveryJob(model.Data.JobReference, model.Data.Status);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        LogManager.LogDebugObject(e);
        //    }

        //    return Ok();
        //}

        //[HttpGet("ValidateShopAddress/{address}/{phone}")]
        //public IActionResult ValidateShopAddress(string address, string phone)
        //{
        //    var apiResponse = new APIResponse();
        //    try
        //    {
        //        var token = _GetDeliveryJobToken();

        //        var headers = new List<KeyValuePair<string, string>>();
        //        var parameters = new List<KeyValuePair<string, string>>();

        //        headers.Add(new KeyValuePair<string, string>("authorization", "Bearer " + token));
        //        parameters.Add(new KeyValuePair<string, string>("address", address));
        //        parameters.Add(new KeyValuePair<string, string>("phone", phone));

        //        var response = MainHttpClient.DeliveryJobPostRequest(MConf.DeliveryJobConfig.AddressValidationEndpoint + "picking", RestSharp.Method.GET, headers, iQueryParameters: parameters);
        //        apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
        //        apiResponse.Payload = JsonConvert.DeserializeObject<AddressValidationModel>(response.Content);

        //    }
        //    catch (Exception e)
        //    {
        //        LogManager.LogError("Erreur: ValidateShopAddress");
        //        LogManager.LogDebugObject(e);
        //        apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
        //        apiResponse.Message = "La nouvelle adresse a été ajoutée avec succès.";
        //    }

        //    return Ok(apiResponse);
        //}

        //[HttpGet("ValidateShopAddressById/{shopId}")]
        //public IActionResult ValidateShopAddressById(int shopId)
        //{
        //    var apiResponse = new APIResponse();
        //    try
        //    {
        //        var token = _GetDeliveryJobToken();

        //        var headers = new List<KeyValuePair<string, string>>();
        //        var parameters = new List<KeyValuePair<string, string>>();

        //        var address = DeliveryJobRepo.GetShopAddressById(shopId);

        //        headers.Add(new KeyValuePair<string, string>("authorization", "Bearer " + token));
        //        parameters.Add(new KeyValuePair<string, string>("address", address));

        //        var response = MainHttpClient.DeliveryJobPostRequest(MConf.DeliveryJobConfig.AddressValidationEndpoint + "picking", RestSharp.Method.GET, headers, iQueryParameters: parameters);
        //        apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
        //        apiResponse.Payload = JsonConvert.DeserializeObject<AddressValidationModel>(response.Content);
        //    }
        //    catch (Exception e) {
        //        LogManager.LogError("Erreur: ValidateShopAddressById");
        //        LogManager.LogDebugObject(e);
        //        apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
        //        apiResponse.Message = "La nouvelle adresse a été ajoutée avec succès.";
        //    }

        //    return Ok(apiResponse);
        //}

        //[HttpGet("ValidateCustomerAddress/{address}/{phone}")]
        //public IActionResult ValidateCustomerAddress(string address, string phone)
        //{
        //    var apiResponse = new APIResponse();
        //    try
        //    {
        //        var token = _GetDeliveryJobToken();

        //        var headers = new List<KeyValuePair<string, string>>();
        //        var parameters = new List<KeyValuePair<string, string>>();

        //        headers.Add(new KeyValuePair<string, string>("authorization", "Bearer " + token));
        //        parameters.Add(new KeyValuePair<string, string>("address", address));
        //        parameters.Add(new KeyValuePair<string, string>("phone", phone));

        //        var response = MainHttpClient.DeliveryJobPostRequest(MConf.DeliveryJobConfig.AddressValidationEndpoint + "delivering", RestSharp.Method.GET, headers, iQueryParameters: parameters);
        //        AddressValidationModel validationResult = JsonConvert.DeserializeObject<AddressValidationModel>(response.Content);
        //        apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
        //        apiResponse.Message = "Address is valid for Stuart Delivery.";
        //        if (validationResult.Succès == false) { apiResponse.Message = "Address is NOT valid for Stuart Delivery."; }
        //        apiResponse.Payload = validationResult;
        //    }
        //    catch (Exception e)
        //    {
        //        LogManager.LogError("Erreur: ValidateShopAddress");
        //        LogManager.LogDebugObject(e);
        //        apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
        //        apiResponse.Message = "La nouvelle adresse a été ajoutée avec succès.";
        //    }

        //    return Ok(apiResponse);
        //}

        //[HttpGet("ValidateCustomerAddressById/{customerAddressId}")]
        //public IActionResult ValidateCustomerAddressById(int customerAddressId)
        //{
        //    var apiResponse = new APIResponse();
        //    try
        //    {
        //        var token = _GetDeliveryJobToken();

        //        var headers = new List<KeyValuePair<string, string>>();
        //        var parameters = new List<KeyValuePair<string, string>>();

        //        var address = DeliveryJobRepo.GetCustomerAddressById(customerAddressId);

        //        headers.Add(new KeyValuePair<string, string>("authorization", "Bearer " + token));
        //        parameters.Add(new KeyValuePair<string, string>("address", address));

        //        var response = MainHttpClient.DeliveryJobPostRequest(MConf.DeliveryJobConfig.AddressValidationEndpoint + "delivering", RestSharp.Method.GET, headers, iQueryParameters: parameters);
        //        apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
        //        apiResponse.Payload = JsonConvert.DeserializeObject<AddressValidationModel>(response.Content);
        //    }
        //    catch (Exception e)
        //    {
        //        LogManager.LogError("Erreur: ValidateCustomerAddressById");
        //        LogManager.LogDebugObject(e);
        //        apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
        //        apiResponse.Message = "La nouvelle adresse a été ajoutée avec succès.";
        //    }

        //    return Ok(apiResponse);
        //}

        //[HttpPost("GetJobPricing")]
        //public IActionResult GetJobPricing([FromBody] JobModelForMobile model)
        //{
        //    var apiResponse = new APIResponse();

        //    try
        //    {
        //        var jobParameterModel = DeliveryJobRepo.GetJobModelForMobile(model);
        //        var jobParameterObj = JsonConvert.SerializeObject(jobParameterModel);

        //        var token = _GetDeliveryJobToken();

        //        var headers = new List<KeyValuePair<string, string>>();
        //        var parameters = new List<KeyValuePair<string, string>>();

        //        headers.Add(new KeyValuePair<string, string>("authorization", "Bearer " + token));
        //        parameters.Add(new KeyValuePair<string, string>("application/json", "{\"job\":" + jobParameterObj + "}"));

        //        var response = MainHttpClient.DeliveryJobPostRequest(MConf.DeliveryJobConfig.PricingEndpoint, RestSharp.Method.POST, headers, iBodyParameters: parameters);
        //        apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
        //        apiResponse.Payload = JsonConvert.DeserializeObject<PricingModel>(response.Content);
        //    }
        //    catch (Exception e)
        //    {
        //        LogManager.LogError("Erreur: GetJobPricing");
        //        LogManager.LogDebugObject(e);
        //        apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
        //        apiResponse.Message = "La nouvelle adresse a été ajoutée avec succès.";
        //    }

        //    return Ok(apiResponse);
        //}

        //[HttpPost("GetJobPricingByOrder")]
        //public IActionResult GetJobPricingByOrder([FromBody] JobModel model)
        //{
        //    var apiResponse = new APIResponse();
        //    try
        //    {
        //        var jobParameterModel = DeliveryJobRepo.GetJobModel(model);
        //        var jobParameterObj = JsonConvert.SerializeObject(jobParameterModel);

        //        var token = _GetDeliveryJobToken();

        //        var headers = new List<KeyValuePair<string, string>>();
        //        var parameters = new List<KeyValuePair<string, string>>();

        //        headers.Add(new KeyValuePair<string, string>("authorization", "Bearer " + token));
        //        parameters.Add(new KeyValuePair<string, string>("application/json", "{\"job\":" + jobParameterObj + "}"));

        //        var response = MainHttpClient.DeliveryJobPostRequest(MConf.DeliveryJobConfig.PricingEndpoint, RestSharp.Method.POST, headers, iBodyParameters: parameters);
        //        apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
        //        apiResponse.Payload = JsonConvert.DeserializeObject<PricingModel>(response.Content);
        //    }
        //    catch (Exception e)
        //    {
        //        LogManager.LogError("Erreur: GetJobPricingByOrder");
        //        LogManager.LogDebugObject(e);
        //        apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
        //        apiResponse.Message = "La nouvelle adresse a été ajoutée avec succès.";
        //    }

        //    return Ok(apiResponse);
        //}

        //[HttpPost("ValidateJob")]
        //public IActionResult ValidateJob([FromBody] JobInput model)
        //{
        //    var apiResponse = new APIResponse();
        //    try
        //    {
        //        var jobParameterObj = JsonConvert.SerializeObject(model);

        //        var token = _GetDeliveryJobToken();

        //        var headers = new List<KeyValuePair<string, string>>();
        //        var parameters = new List<KeyValuePair<string, string>>();

        //        headers.Add(new KeyValuePair<string, string>("authorization", "Bearer " + token));
        //        parameters.Add(new KeyValuePair<string, string>("application/json", "{\"job\":" + jobParameterObj + "}"));

        //        var response = MainHttpClient.DeliveryJobPostRequest(MConf.DeliveryJobConfig.JobValidationEndpoint, RestSharp.Method.POST, headers, iBodyParameters: parameters);
        //        apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
        //        apiResponse.Payload = JsonConvert.DeserializeObject<JobValidationModel>(response.Content);
        //    }
        //    catch (Exception e)
        //    {
        //        LogManager.LogError("Erreur: ValidateJob");
        //        LogManager.LogDebugObject(e);
        //        apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
        //        apiResponse.Message = "La nouvelle adresse a été ajoutée avec succès.";
        //    }

        //    return Ok(apiResponse);
        //}

        //[HttpPost("ValidateJobByOrder")]
        //public IActionResult ValidateJobByOrder([FromBody] JobModel model)
        //{
        //    var apiResponse = new APIResponse();
        //    try
        //    {
        //        var jobParameterModel = DeliveryJobRepo.GetJobModel(model);
        //        var jobParameterObj = JsonConvert.SerializeObject(jobParameterModel);

        //        var token = _GetDeliveryJobToken();

        //        var headers = new List<KeyValuePair<string, string>>();
        //        var parameters = new List<KeyValuePair<string, string>>();

        //        headers.Add(new KeyValuePair<string, string>("authorization", "Bearer " + token));
        //        parameters.Add(new KeyValuePair<string, string>("application/json", "{\"job\":" + jobParameterObj + "}"));

        //        var response = MainHttpClient.DeliveryJobPostRequest(MConf.DeliveryJobConfig.JobValidationEndpoint, RestSharp.Method.POST, headers, iBodyParameters: parameters);
        //        apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
        //        apiResponse.Payload = JsonConvert.DeserializeObject<JobValidationModel>(response.Content);
        //    }
        //    catch (Exception e)
        //    {
        //        LogManager.LogError("Erreur: ValidateJobByOrder");
        //        LogManager.LogDebugObject(e);
        //        apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
        //        apiResponse.Message = "La nouvelle adresse a été ajoutée avec succès.";
        //    }

        //    return Ok(apiResponse);
        //}

        //[HttpPost("CreateJob")]
        //public IActionResult CreateJob([FromHeader] string Authorization, [FromBody] JobModel model)
        //{
        //    LogManager.LogInfo("CreateJob");
        //    LogManager.LogInfo("Request Model: ");
        //    LogManager.LogDebugObject(model);
        //    var apiResponse = new APIResponse();
        //    try
        //    {
        //        var jobParameterModel = DeliveryJobRepo.GetJobModel(model);
        //        var jobParameterObj = JsonConvert.SerializeObject(jobParameterModel);
        //        var jobParameterObjStr = "{\"job\":" + jobParameterObj + "}";

        //        var token = _GetDeliveryJobToken();

        //        var headers = new List<KeyValuePair<string, string>>();
        //        var parameters = new List<KeyValuePair<string, string>>();

        //        headers.Add(new KeyValuePair<string, string>("authorization", "Bearer " + token));
        //        parameters.Add(new KeyValuePair<string, string>("application/json", jobParameterObjStr));

        //        var response = MainHttpClient.DeliveryJobPostRequest(MConf.DeliveryJobConfig.JobCreationEndpoint, RestSharp.Method.POST, headers, iBodyParameters: parameters);
        //        LogManager.LogInfo("Response Model: ");
        //        var responseModel = JsonConvert.DeserializeObject<JobOutput>(response.Content);
        //        LogManager.LogDebugObject(responseModel);
        //        if (string.IsNullOrEmpty(responseModel.Erreur))
        //        {
        //            var deliveryJobModel = DeliveryJobRepo.CreateDeliveryJob(responseModel.AssignmentCode, model, jobParameterObjStr, Authorization != null ? Authorization.Split(' ')[1] : string.Empty);
        //            apiResponse.StatusCode = System.Net.HttpStatusCode.OK;
        //            apiResponse.Message = "Delivery job created successfully.";
        //            apiResponse.Payload = responseModel;
        //        }
        //        else
        //        {
        //            LogManager.LogInfo("CreateJob ERROR:");
        //            LogManager.LogDebugObject(response);
        //            apiResponse.Message = "Delivery job failed to create.";
        //            apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
        //            apiResponse.Payload = response;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        LogManager.LogError("Erreur: CreateJob");
        //        LogManager.LogDebugObject(e);
        //        apiResponse.StatusCode = System.Net.HttpStatusCode.BadRequest;
        //        apiResponse.Message = "La nouvelle adresse a été ajoutée avec succès.";
        //    }

        //    return Ok(apiResponse);
        //}

        //private string _GetDeliveryJobToken()
        //{
        //    var retToken = string.Empty;

        //    try
        //    {
        //        var headers = new List<KeyValuePair<string, string>>();
        //        var parameters = new List<KeyValuePair<string, string>>();

        //        headers.Add(new KeyValuePair<string, string>("content-type", "application/x-www-form-urlencoded"));
        //        parameters.Add(new KeyValuePair<string, string>("application/x-www-form-urlencoded", "client_id=" + MConf.DeliveryJobConfig.ClientId + "&client_secret=" + MConf.DeliveryJobConfig.ClientSecret + "&scope =api&grant_type=client_credentials"));

        //        var tokenResult = MainHttpClient.DeliveryJobPostRequest(MConf.DeliveryJobConfig.TokenEndpoint, RestSharp.Method.POST, headers, iBodyParameters: parameters);
        //        var tokenModel = JsonConvert.DeserializeObject<TokenModel>(tokenResult.Content);

        //        if(string.IsNullOrEmpty(tokenModel.Erreur))
        //        {
        //            retToken = tokenModel.AccessToken;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        LogManager.LogError("Erreur: _GetDeliveryJobToken");
        //        LogManager.LogDebugObject(e);
        //    }

        //    return retToken;
        //}
    }
}