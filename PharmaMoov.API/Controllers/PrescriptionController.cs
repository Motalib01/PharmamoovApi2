using PharmaMoov.Models;
using PharmaMoov.Models.Prescription;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaMoov.Models.Admin;
using System;
using System.Collections.Generic;

namespace PharmaMoov.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Prescription")]
    public class PrescriptionController : APIBaseController
    {
        IPrescriptionRepository PrescriptionRepo { get; }

        public PrescriptionController(IPrescriptionRepository _prescriptionRepo)
        {
            PrescriptionRepo = _prescriptionRepo;
        }

        [HttpGet("PopulatePrescriptions/{_prescriptionRecordId}/{_isActive}/{_shopId}")]
        public IActionResult PopulatePrescriptions(int _prescriptionRecordId, int _isActive, Guid _shopId)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PrescriptionRepo.PopulatePrescriptions(_prescriptionRecordId, _isActive, _shopId);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                return BadRequest(apiResp);              
            }
            return BadRequest(new APIResponse
            {
                Message = "Model Objet Invalid",
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Status = "Object level error.",
                ModelError = ModelState.Errors()
            });
        }

        [HttpPost("AddPrescription")]
        public IActionResult AddPrescription([FromHeader] string Authorization, [FromBody] PrescriptionDetail _prescription)
        {
            APIResponse aResp = new APIResponse();
            
            if (ModelState.IsValid)
            {
                aResp = PrescriptionRepo.AddPrescription(Authorization.Split(' ')[1], _prescription);
                if (aResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(aResp);
                }
                return BadRequest(aResp);
            }

            aResp = new APIResponse
            {
                Message = "Model Objet Invalid",
                Status = "Model Erreur!",
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                ModelError = ModelState.Errors()
            };
            return BadRequest(aResp);
        }

        [HttpPut("EditPrescription")]
        public IActionResult EditPrescription([FromHeader] string Authorization, [FromBody] PrescriptionDetail _prescription)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PrescriptionRepo.EditPrescription(Authorization.Split(' ')[1], _prescription);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                return BadRequest(apiResp);
            }
            return BadRequest(new APIResponse
            {
                Message = "Model Objet Invalid",
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Status = "Object level error.",
                ModelError = ModelState.Errors()
            });
        }

        [HttpPut("ChangePrescriptionStatus")]
        public IActionResult ChangePrescriptionStatus([FromHeader] string Authorization, [FromBody] ChangeRecordStatus _prescription)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PrescriptionRepo.ChangePrescriptionStatus(Authorization.Split(' ')[1], _prescription);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                return BadRequest(apiResp);
            }
            return BadRequest(new APIResponse
            {
                Message = "Model Objet Invalid",
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Status = "Object level error.",
                ModelError = ModelState.Errors()
            });
        }
        
        [HttpGet("GetPrescription/{prescriptionRecordId}")]
        public IActionResult GetPrescription(int prescriptionRecordId)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PrescriptionRepo.GetPrescription(prescriptionRecordId);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                return BadRequest(apiResp);
            }

            return BadRequest(new APIResponse
            {
                Message = "Model Objet Invalid",
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Status = "Object level error.",
                ModelError = ModelState.Errors()
            });
        }

        [HttpGet("GetPrescriptionDetails/{prescriptionRecordId}")]
        public IActionResult GetPrescriptionDetails(int prescriptionRecordId)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PrescriptionRepo.GetPrescriptionDetails(prescriptionRecordId);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                return BadRequest(apiResp);
            }

            return BadRequest(new APIResponse
            {
                Message = "Model Objet Invalid",
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Status = "Object level error.",
                ModelError = ModelState.Errors()
            });
        }

        [HttpPost("AddPrescriptionProducts")]
        public IActionResult AddPrescriptionProducts([FromHeader] string Authorization, [FromBody]List<PrescriptionProductsParamModel> pModel)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PrescriptionRepo.AddPrescriptionProducts(Authorization.Split(' ')[1], pModel);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                return BadRequest(apiResp);
            }

            return BadRequest(new APIResponse
            {
                Message = "Model Objet Invalid",
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Status = "Object level error.",
                ModelError = ModelState.Errors()
            });
        }
        
        [HttpGet("GetInvoicePrescriptionCount/{userId}")]
        public IActionResult GetInvoicePrescriptionCount(Guid userId)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PrescriptionRepo.GetInvoicePrescriptionCount(userId);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                return BadRequest(apiResp);
            }

            return BadRequest(new APIResponse
            {
                Message = "Model Objet Invalid",
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Status = "Object level error.",
                ModelError = ModelState.Errors()
            });
        }

        [HttpGet("GetInvoicePrescriptionList/{userId}")]
        public IActionResult GetInvoicePrescriptionList([FromHeader] string Authorization, Guid userId)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PrescriptionRepo.GetInvoicePrescriptionList(Authorization.Split(' ')[1], userId);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                return BadRequest(apiResp);
            }

            return BadRequest(new APIResponse
            {
                Message = "Model Objet Invalid",
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Status = "Object level error.",
                ModelError = ModelState.Errors()
            });
        }

        [HttpGet("GetPrescriptionProductList")]
        public IActionResult GetPrescriptionProductList([FromHeader] string Authorization, [FromQuery]PrescriptionProductListParamModel model)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PrescriptionRepo.GetPrescriptionProductList(Authorization.Split(' ')[1], model);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                return BadRequest(apiResp);
            }

            return BadRequest(new APIResponse
            {
                Message = "Model Objet Invalid",
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Status = "Object level error.",
                ModelError = ModelState.Errors()
            });
        }

        [HttpPost("CancelInvoice")]
        public IActionResult CancelInvoice([FromBody] InvoiceParamModel model)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PrescriptionRepo.CancelInvoice(model);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                return BadRequest(apiResp);
            }

            return BadRequest(new APIResponse
            {
                Message = "Model Objet Invalid",
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Status = "Object level error.",
                ModelError = ModelState.Errors()
            });
        }

        [HttpGet("GetPrescriptionRecordId")]
        public IActionResult GetPrescriptionRecordId([FromQuery] PrescriptionParamModel model)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PrescriptionRepo.GetPrescriptionRecordId(model);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                return BadRequest(apiResp);
            }

            return BadRequest(new APIResponse
            {
                Message = "Model Objet Invalid",
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Status = "Object level error.",
                ModelError = ModelState.Errors()
            });
        }
    }
}
