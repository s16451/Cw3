using Microsoft.AspNetCore.Mvc;

namespace APBD
{
    public interface IStudentDbService
    {
        EnrollStudentResponse EnrollStudent(EnrollStudentRequest request);
        Enrollment PromoteStudents(PromoteStudentsRequest request);
        Student GetStudent(string index);
        EncryptedCredentials GetCredentials(LoginRequest request);
        bool IsRefTokenAuth(string refToken);
        void SaveRefToken(SaveRefTokenRequest request);
        void SaveRefToken(string prevToken, string token);
    }
}