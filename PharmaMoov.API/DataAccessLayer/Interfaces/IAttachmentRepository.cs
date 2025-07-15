using PharmaMoov.Models;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface IAttachmentRepository
    {
        APIResponse AddAttachmentRecord(string _authToken, string _uploadedFileName, string _serverPhyPath, string _urlRoot, UploadTypes _ut);
    }
}
