using System.ComponentModel.DataAnnotations;

namespace WebBlog.Models
{
    public class SignIn
    {
        [Required]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Username length must be from 6 to 50")]
        public string Username { get; set; }

        [Required]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Password length must be from 6 to 50")]
        public string Password { get; set; }
    }
}
