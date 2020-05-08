using Microsoft.AspNetCore.Mvc;

namespace APBD
{
    public interface IStudentDbService
    {
        EnrollStudentResponse EnrollStudent(EnrollStudentRequest request);
        EnrollmentResponse PromoteStudents(PromoteStudentsRequest request);
    }
}