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
        
        public string Birthdate { get; set; }
        
        [Required]
        public string Studies { get; set; }
    }
}