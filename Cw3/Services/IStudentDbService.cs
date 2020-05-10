using Microsoft.AspNetCore.Mvc;

namespace APBD
{
    public interface IStudentDbService
    {
        EnrollStudentResponse EnrollStudent(EnrollStudentRequest request);
        Enrollment PromoteStudents(PromoteStudentsRequest request);
        Student GetStudent(string index);
        bool IsAuthStudent(LoginRequest request);
    }
}