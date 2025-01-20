

using DiplomWork.DTO;

namespace DiplomWork.Models
{
    public class Target
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public decimal Limit { get; set; }
        public Guid OwnerId { get; set; }
        public List<Profit>? Profits { get; set; }

        public TargetDTO ConvertToDTO()
        {
            return new TargetDTO
            {
                Id = Id,
                Name = Name,
                Limit = Limit,
                Amount = Profits == null ? 0 : Profits.Sum(f => f.Amount),
            };
        }
    }
}
