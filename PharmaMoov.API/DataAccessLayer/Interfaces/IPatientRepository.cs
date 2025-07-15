using PharmaMoov.Models;
using PharmaMoov.Models.Patient;

namespace PharmaMoov.API.DataAccessLayer.Interfaces
{
    public interface IPatientRepository
    {
        APIResponse GetAllPatients(string _auth, int _pageNo, string _searchKey);
        APIResponse AddPatient(string _auth, PatientDTO _patient);
        APIResponse ViewPatient(string _auth, int _patientRecordId);
        APIResponse PatientDetail(string _auth, int _patientRecordId, int patientDetailType);
        APIResponse UpdatePatientProfile(string _auth, PatientProfileModel _patientProfile);
        APIResponse UpdatePatientAddress(string _auth, PatientAddressModel _patientAddress);
        APIResponse UpdatePatientDetails(string _auth, PatientDTO _patient);
    }
}
