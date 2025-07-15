using PharmaMoov.Models;
using PharmaMoov.Models.Attachment;
using PharmaMoov.API.DataAccessLayer.Interfaces;
using ImageMagick;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net.Http.Headers;
using PharmaMoov.Models.Shop;

namespace PharmaMoov.API.Controllers
{
    [Authorize]
    [Produces("application/json")]
    [Route("api/Attachment")]
    public class AttachmentController : Controller
    {
        IAttachmentRepository AttachmentRepo { get; }
        public AttachmentController(IAttachmentRepository _att)
        {
            AttachmentRepo = _att;
        }

        [HttpPost("UploadProfileImage"), DisableRequestSizeLimit]
        public IActionResult UploadProfileImage([FromHeader] string Authorization)
        {
            return UploadImage(Authorization, UploadTypes.UploadProfileImage);
        }

        [HttpPost("UploadPrescription"), DisableRequestSizeLimit]
        public IActionResult UploadPrescription([FromHeader] string Authorization)
        {
            return UploadImage(Authorization, UploadTypes.UploadPrescription);
        }

        private IActionResult UploadImage(string Authorization, UploadTypes Type)
        {
            string fileFolder = "";
            switch (Type)
            {
                case UploadTypes.UploadBanner:
                    fileFolder = "Banners";
                    break;
                case UploadTypes.UploadIcon:
                    fileFolder = "Icons";
                    break;
                case UploadTypes.UploadBackgroundImage:
                    fileFolder = "BackgroundImages";
                    break;
                case UploadTypes.UploadProfileImage:
                    fileFolder = "ProfileImages";
                    break;
                case UploadTypes.UploadPrescription:
                    fileFolder = "Prescription";
                    break;
                default:
                    fileFolder = "";
                    break;
            }

            if (Authorization != null)
            {
                string token = Authorization.Split(' ')[1];
                try
                {
                    var UrlRoot = Request.Scheme + "://" + Request.Host + Request.PathBase;
                    var file = Request.Form.Files[0];
                    var DestinationFolderName = Path.Combine("Resources", fileFolder);

                    if (!Directory.Exists(Path.Combine(Directory.GetCurrentDirectory(), DestinationFolderName)))
                    {
                        Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), DestinationFolderName));
                    }

                    var ServerPhysicalPath = Path.Combine(Directory.GetCurrentDirectory(), DestinationFolderName);

                    if (file.Length > 0)
                    {
                        var UploadedFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        var fullPath = Path.Combine(ServerPhysicalPath, UploadedFileName);
                        var dbPath = Path.Combine(DestinationFolderName, UploadedFileName);

                        APIResponse apiResp = AttachmentRepo.AddAttachmentRecord(token, UploadedFileName, ServerPhysicalPath, UrlRoot, Type);
                        if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            Attachment AddedAttachment = apiResp.Payload as Attachment;
                            using (var stream = new FileStream(AddedAttachment.AttachmentServerPhysicalPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }
                            //compress image a little bit
                            var image = new FileInfo(AddedAttachment.AttachmentServerPhysicalPath);
                            var optimizer = new ImageOptimizer();
                            optimizer.LosslessCompress(image);
                            image.Refresh();
                            return Ok(apiResp);
                        }
                        else
                        {
                            return BadRequest(apiResp);
                        }
                    }
                    else
                    {
                        return BadRequest(new APIResponse
                        {
                            Message = "No File Found!",
                            Status = "No File Found!",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        });
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(new APIResponse
                    {
                        Message = "Erreur de serveur interne!",
                        Status = ex.Message,
                        StatusCode = System.Net.HttpStatusCode.InternalServerError
                    });
                }
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "401 Unauthorized!",
                    Status = "Invalid Authorization",
                    StatusCode = System.Net.HttpStatusCode.Unauthorized
                });
            }
        }

        [HttpPost("UploadBanner"), DisableRequestSizeLimit]
        public IActionResult UploadBanner([FromHeader] string Authorization)
        {
            if (Authorization != null)
            {
                string token = Authorization.Split(' ')[1];
                try
                {
                    var UrlRoot = Request.Scheme + "://" + Request.Host + Request.PathBase;
                    var file = Request.Form.Files[0];
                    var DestinationFolderName = Path.Combine("Resources", "Banners");
                    var ServerPhysicalPath = Path.Combine(Directory.GetCurrentDirectory(), DestinationFolderName);

                    if (file.Length > 0)
                    {
                        var UploadedFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        var fullPath = Path.Combine(ServerPhysicalPath, UploadedFileName);
                        var dbPath = Path.Combine(DestinationFolderName, UploadedFileName);

                        APIResponse apiResp = AttachmentRepo.AddAttachmentRecord(token, UploadedFileName, ServerPhysicalPath, UrlRoot, UploadTypes.UploadBanner);
                        if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            Attachment AddedAttachment = apiResp.Payload as Attachment;
                            using (var stream = new FileStream(AddedAttachment.AttachmentServerPhysicalPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }
                            //compress image a little bit
                            var image = new FileInfo(AddedAttachment.AttachmentServerPhysicalPath);
                            var optimizer = new ImageOptimizer();
                            optimizer.LosslessCompress(image);
                            image.Refresh();
                            return Ok(apiResp);
                        }
                        else
                        {
                            return BadRequest(apiResp);
                        }
                    }
                    else
                    {
                        return BadRequest(new APIResponse
                        {
                            Message = "No File Found!",
                            Status = "No File Found!",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        });
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(new APIResponse
                    {
                        Message = "Erreur de serveur interne!",
                        Status = ex.Message,
                        StatusCode = System.Net.HttpStatusCode.InternalServerError
                    });
                }
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "401 Unauthorized!",
                    Status = "Invalid Authorization",
                    StatusCode = System.Net.HttpStatusCode.Unauthorized
                });
            }
        }

        [HttpPost("UploadIcon"), DisableRequestSizeLimit]
        public IActionResult UploadIcon([FromHeader] string Authorization)
        {

            if (Authorization != null)
            {
                string token = Authorization.Split(' ')[1];
                try
                {
                    var UrlRoot = Request.Scheme + "://" + Request.Host + Request.PathBase;
                    var file = Request.Form.Files[0];
                    var DestinationFolderName = Path.Combine("Resources", "Icons");
                    var ServerPhysicalPath = Path.Combine(Directory.GetCurrentDirectory(), DestinationFolderName);

                    if (file.Length > 0)
                    {
                        var UploadedFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        var fullPath = Path.Combine(ServerPhysicalPath, UploadedFileName);
                        var dbPath = Path.Combine(DestinationFolderName, UploadedFileName);

                        APIResponse apiResp = AttachmentRepo.AddAttachmentRecord(token, UploadedFileName, ServerPhysicalPath, UrlRoot, UploadTypes.UploadIcon);
                        if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            Attachment AddedAttachment = apiResp.Payload as Attachment;
                            using (var stream = new FileStream(AddedAttachment.AttachmentServerPhysicalPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }
                            //compress image a little bit
                            var image = new FileInfo(AddedAttachment.AttachmentServerPhysicalPath);
                            var optimizer = new ImageOptimizer();
                            optimizer.LosslessCompress(image);
                            image.Refresh();
                            return Ok(apiResp);
                        }
                        else
                        {
                            return BadRequest(apiResp);
                        }
                    }
                    else
                    {
                        return BadRequest(new APIResponse
                        {
                            Message = "No File Found!",
                            Status = "No File Found!",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        });
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(new APIResponse
                    {
                        Message = "Erreur de serveur interne!",
                        Status = ex.Message,
                        StatusCode = System.Net.HttpStatusCode.InternalServerError
                    });
                }
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "401 Unauthorized!",
                    Status = "Invalid Authorization",
                    StatusCode = System.Net.HttpStatusCode.Unauthorized
                });
            }
        }

        [HttpPost("UploadBackgroundImage"), DisableRequestSizeLimit]
        public IActionResult UploadBackgroundImage([FromHeader] string Authorization)
        {

            if (Authorization != null)
            {
                string token = Authorization.Split(' ')[1];
                try
                {
                    var UrlRoot = Request.Scheme + "://" + Request.Host + Request.PathBase;
                    var file = Request.Form.Files[0];
                    var DestinationFolderName = Path.Combine("Resources", "BackgroundImages");
                    var ServerPhysicalPath = Path.Combine(Directory.GetCurrentDirectory(), DestinationFolderName);

                    if (file.Length > 0)
                    {
                        var UploadedFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        var fullPath = Path.Combine(ServerPhysicalPath, UploadedFileName);
                        var dbPath = Path.Combine(DestinationFolderName, UploadedFileName);

                        APIResponse apiResp = AttachmentRepo.AddAttachmentRecord(token, UploadedFileName, ServerPhysicalPath, UrlRoot, UploadTypes.UploadBackgroundImage);
                        if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            Attachment AddedAttachment = apiResp.Payload as Attachment;
                            using (var stream = new FileStream(AddedAttachment.AttachmentServerPhysicalPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }
                            //compress image a little bit
                            var image = new FileInfo(AddedAttachment.AttachmentServerPhysicalPath);
                            var optimizer = new ImageOptimizer();
                            optimizer.LosslessCompress(image);
                            image.Refresh();
                            return Ok(apiResp);
                        }
                        else
                        {
                            return BadRequest(apiResp);
                        }
                    }
                    else
                    {
                        return BadRequest(new APIResponse
                        {
                            Message = "No File Found!",
                            Status = "No File Found!",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        });
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(new APIResponse
                    {
                        Message = "Erreur de serveur interne!",
                        Status = ex.Message,
                        StatusCode = System.Net.HttpStatusCode.InternalServerError
                    });
                }
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "401 Unauthorized!",
                    Status = "Invalid Authorization",
                    StatusCode = System.Net.HttpStatusCode.Unauthorized
                });
            }
        }

        [HttpPost("UploadDocument"), DisableRequestSizeLimit]
        public IActionResult UploadDocument([FromHeader] string Authorization)
        {

            if (Authorization != null)
            {
                string token = Authorization.Split(' ')[1];
                try
                {
                    var UrlRoot = Request.Scheme + "://" + Request.Host + Request.PathBase;
                    var file = Request.Form.Files[0];
                    var DestinationFolderName = Path.Combine("Resources", "Documents");
                    var ServerPhysicalPath = Path.Combine(Directory.GetCurrentDirectory(), DestinationFolderName);

                    if (file.Length > 0)
                    {
                        var UploadedFileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                        var fullPath = Path.Combine(ServerPhysicalPath, UploadedFileName);
                        var dbPath = Path.Combine(DestinationFolderName, UploadedFileName);
                        var FileSize = file.Length;

                        APIResponse apiResp = AttachmentRepo.AddAttachmentRecord(token, UploadedFileName, ServerPhysicalPath, UrlRoot, UploadTypes.UploadDocument);
                        if (apiResp.StatusCode == System.Net.HttpStatusCode.OK)
                        {
                            Attachment AddedAttachment = apiResp.Payload as Attachment;
                            using (var stream = new FileStream(AddedAttachment.AttachmentServerPhysicalPath, FileMode.Create))
                            {
                                file.CopyTo(stream);
                            }

                            return Ok(apiResp);
                        }
                        else
                        {
                            return BadRequest(apiResp);
                        }
                    }
                    else
                    {
                        return BadRequest(new APIResponse
                        {
                            Message = "No File Found!",
                            Status = "No File Found!",
                            StatusCode = System.Net.HttpStatusCode.BadRequest
                        });
                    }
                }
                catch (Exception ex)
                {
                    return BadRequest(new APIResponse
                    {
                        Message = "Erreur de serveur interne!",
                        Status = ex.Message,
                        StatusCode = System.Net.HttpStatusCode.InternalServerError
                    });
                }
            }
            else
            {
                return BadRequest(new APIResponse
                {
                    Message = "401 Unauthorized!",
                    Status = "Invalid Authorization",
                    StatusCode = System.Net.HttpStatusCode.Unauthorized
                });
            }
        }
    }
}