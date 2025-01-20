using DiplomWork.DTO;
using System.ComponentModel.DataAnnotations.Schema;

namespace DiplomWork.Models
{
    public class Profit
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public decimal Amount { get; set; }
        public long CreatedAt { get; set; }
        public Guid? CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public ProfitCategory? Category { get; set; }
        public Guid? TargetId { get; set; }
        public Target? Target { get; set; }

    }

    public class ProfitListDTO : EntityListDTO<Profit>
    {
        public ProfitCategory[] Categories { get; set; }
    }
}
