namespace ProductWeb.Models
{
    public class LoginFormRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string? LoginVerificationCode { get; set; }
    }

}
