using System.ComponentModel.DataAnnotations;

namespace APBD
{
    public class SaveRefTokenRequest
    {
        [Required]
        public string RefToken { get; set; }
        public string IndexNumber { get; set; }
    }
}