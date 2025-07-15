using PharmaMoov.Models;
using PharmaMoov.Models.Patient;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PharmaMoov.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/Patient")]
    public class PatientController : ControllerBase
    {
        IPatientRepository PatientRepo { get; }
        public PatientController(IPatientRepository _patientRepo)
        {
            PatientRepo = _patientRepo;
        }

        [HttpGet("GetAllPatients")]
        public IActionResult GetAllPatients([FromHeader] string Authorization, [FromQuery]FilterPatientModel filterPatientModel)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PatientRepo.GetAllPatients(Authorization.Split(' ')[1], filterPatientModel._pageNo, filterPatientModel._searchKey);
                if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    return Ok(apiResp);
                }
                else
                {
                    return BadRequest(apiResp);
                }
            }
            return BadRequest(new APIResponse
            {
                Message = "Model Objet Invalid",
                StatusCode = System.Net.HttpStatusCode.BadRequest,
                Status = "Object level error.",
                ModelError = ModelState.Errors()
            });
        }

        [HttpPost("AddPatient")]
        public IActionResult AddPatient([FromHeader] string Authorization, [FromBody] PatientDTO _patient)
        {
            APIResponse aResp = new APIResponse();

            if (ModelState.IsValid)
            {
                aResp = PatientRepo.AddPatient(Authorization.Split(' ')[1], _patient);
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

        [HttpGet("ViewPatient/{_patientRecordId}")]
        public IActionResult ViewPatient([FromHeader] string Authorization, int _patientRecordId)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PatientRepo.ViewPatient(Authorization.Split(' ')[1], _patientRecordId);
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

        [HttpGet("PatientDetail/{_patientRecordId}/{_patientDetailType}")]
        public IActionResult PatientDetail([FromHeader] string Authorization, int _patientRecordId, int _patientDetailType)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PatientRepo.PatientDetail(Authorization.Split(' ')[1], _patientRecordId, _patientDetailType);
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

        [HttpPut("UpdatePatientProfile")]
        public IActionResult UpdatePatientProfile([FromHeader] string Authorization, [FromBody] PatientProfileModel _patientProfile)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PatientRepo.UpdatePatientProfile(Authorization.Split(' ')[1], _patientProfile);
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

        [HttpPut("UpdatePatientAddress")]
        public IActionResult UpdatePatientAddress([FromHeader] string Authorization, [FromBody] PatientAddressModel _patientAddressModel)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PatientRepo.UpdatePatientAddress(Authorization.Split(' ')[1], _patientAddressModel);
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

        [HttpPost("UpdatePatientDetails")]
        public IActionResult UpdatePatientDetails([FromHeader] string Authorization, [FromBody] PatientDTO _patient)
        {
            if (ModelState.IsValid)
            {
                APIResponse apiResp = PatientRepo.UpdatePatientDetails(Authorization.Split(' ')[1], _patient);
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
