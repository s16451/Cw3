using System;

namespace APBD
{
    public class EnrollmentResponse
    {
        public int IdEnrollment { get; set; }
        public int Semester { get; set; }
        public int IdStudy { get; set; }
        public DateTime StartDate { get; set; }

        public EnrollmentResponse(Enrollment enrollment)
        {
            IdEnrollment = enrollment.IdEnrollment;
            Semester = enrollment.Semester;
            IdStudy = enrollment.IdStudy;
            StartDate = enrollment.StartDate;
        }
    }
}