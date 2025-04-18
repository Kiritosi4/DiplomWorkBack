

namespace DiplomWork.DTO
{
    public record TargetDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public decimal Limit { get; set; }
        public bool Closed { get; set; }
    }
}
