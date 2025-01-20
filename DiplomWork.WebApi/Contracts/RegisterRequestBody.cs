namespace DiplomWork.WebApi.Contracts
{
    public record RegisterRequestBody
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
    }
}
