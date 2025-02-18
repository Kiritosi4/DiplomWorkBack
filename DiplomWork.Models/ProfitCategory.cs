

namespace DiplomWork.Models
{
    public class ProfitCategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? OwnerID { get; set; } 
        public List<Profit>? Profits { get; set; }
    }
}
