using DiplomWork.DTO;

namespace DiplomWork.Models
{
    public class Target
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Limit { get; set; }
        public decimal Amount { get; set; }
        public Guid OwnerId { get; set; }
        public bool Closed { get; set; }

        public TargetDTO ConvertToDTO()
        {
            return new TargetDTO
            {
                Id = Id,
                Name = Name,
                Limit = Limit,
                Amount = Amount,
                Closed = Closed
            };
        }
    }
}
