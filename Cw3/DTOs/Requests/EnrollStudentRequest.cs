using System;
using System.ComponentModel.DataAnnotations;

namespace APBD
{
    public class EnrollStudentRequest
    {
        [Required, RegularExpression("^s[0-9]+$")]
        public string IndexNumber { get; set; }
        
        [Required, MaxLength(100)]
        public string FirstName { get; set; }
        
        [Required, MaxLength(100)]
        public string LastName { get; set; }
        
        public DateTime Birthdate { get; set; } = new DateTime(1993, 03, 30);
        
        [Required]
        public string Studies { get; set; }

        public Student MapToStudent()
        {
            Student student = new Student
            {
                IndexNumber = IndexNumber,
                FirstName = FirstName,
                LastName = LastName,
                Birthdate = Birthdate,
                Studies = Studies
            };
            return student;
        }
    }
}