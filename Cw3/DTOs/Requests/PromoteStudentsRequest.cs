using System.ComponentModel.DataAnnotations;

namespace APBD
{
    public class PromoteStudentsRequest
    {
        [Required, MaxLength(100)]
        public string Studies { get; set; }
        
        [Required]
        public int Semester { get; set; }
    }
}