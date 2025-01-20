namespace DiplomWork.WebApi.Contracts
{
    public record LoginRequestBody
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
