

namespace DiplomWork.DTO
{
    public record AddTargetDTO
    {
        public string Name { get; set; }
        public decimal Amount { get; set; }
        public decimal Limit { get; set; }
        public bool Closed { get; set; }
    }
}
