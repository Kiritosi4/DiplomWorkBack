

namespace DiplomWork.DTO
{
    public class CreateProfitDTO
    {
        public Guid? CategoryId { get; set; }
        public decimal Amount { get; set; }
        public long Timestamp { get; set; }
    }
}
