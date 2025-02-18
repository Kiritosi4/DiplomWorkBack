using DiplomWork.Models;

namespace DiplomWork.DTO
{
    public record ExpensesDashboardDTO
    {
        public decimal TotalAmount { get; set; }
        public decimal LastTotalAmount { get; set; }
        public int TotalOperations { get; set; }
        public int LastTotalOperations { get; set; }

        public IEnumerable<ChartSegment> ChartData { get; set; }
        public Dictionary<Guid, decimal> PieChartData { get; set; }
        public IEnumerable<ExpenseCategory> Categories { get; set; }
    }
}
