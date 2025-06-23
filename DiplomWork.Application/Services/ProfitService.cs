using DiplomWork.Models;
using DiplomWork.Persistance;
using Microsoft.EntityFrameworkCore;
using DiplomWork.DTO;
using System.Linq.Expressions;

namespace DiplomWork.Application.Services
{
    public class ProfitService
    {
        readonly DiplomWorkDbContext _db;

        public ProfitService(DiplomWorkDbContext db)
        {
            _db = db;
        }

        public async Task<Profit> AddNewProfit(CreateProfitDTO createProfitDTO, Guid userId)
        {
            var newProfit = new Profit
            {
                Id = Guid.NewGuid(),
                Amount = createProfitDTO.Amount,
                CreatedAt = createProfitDTO.Timestamp,
                CategoryId = createProfitDTO.CategoryId,
                OwnerId = userId
            };
            await _db.Profits.AddAsync(newProfit);
            await _db.SaveChangesAsync();   

            return newProfit;
        }

        public async Task DeleteProfitById(Guid id)
        {
            await _db.Profits.Where(x => x.Id == id).ExecuteDeleteAsync();
        }

        public async Task EditProfit(Guid id, CreateProfitDTO editedProfit, Guid userId)
        {
            await _db.Profits
                .Where(x => x.Id == id && x.OwnerId == userId)
                .ExecuteUpdateAsync(x => x
                .SetProperty(x => x.Amount, editedProfit.Amount)
                .SetProperty(x => x.CreatedAt, editedProfit.Timestamp)
                .SetProperty(x => x.CategoryId, editedProfit.CategoryId));
        }

        public async Task<ProfitListDTO> GetUserProfits(Guid userId, int offset = 0, int limit = 10, string orderBy = "date", string order = "desc", long? minTimestamp = 0, long? maxTimestamp = 0, Guid?[] categories = null)
        {
            var query = _db.Profits
                .AsNoTracking()
                .Where(x => x.OwnerId == userId);

            if(categories != null && categories.Length > 0)
            {
                if (categories.Any(x => x.Value == Guid.Empty))
                {
                    query = query.Where(x => categories.Contains(x.CategoryId) || x.CategoryId == null);
                }
                else
                {
                    query = query.Where(x => categories.Contains(x.CategoryId));
                }
            }

            if(minTimestamp > 0 || maxTimestamp > 0)
            {
                query = query.Where(x => x.CreatedAt > minTimestamp && x.CreatedAt < maxTimestamp);
            }

            var total = await query.CountAsync();

            query = query
                .Skip(offset)
                .Take(limit);

            // Создание выражения для сортировки
            var parameter = Expression.Parameter(typeof(Profit), orderBy);
            var property = Expression.Property(parameter, orderBy);
            var orderByExpression = Expression.Lambda<Func<Profit, object>>(Expression.Convert(property, typeof(object)), parameter);

            if (order == "desc")
            {
                query = query.OrderByDescending(orderByExpression);
            }
            else
            {
                query = query.OrderBy(orderByExpression);
            }

            var profits = await query.ToArrayAsync();
            
            var categoryIds = profits.Select(x => x.CategoryId).ToArray();
            var includedCategories = await _db.ProfitCategories.Where(x => categoryIds.Contains(x.Id)).ToArrayAsync();

            return new ProfitListDTO
            {
                Data = profits,
                Categories = includedCategories,
                Total = total
            };
        }

        public async Task<ProfitDashboardDTO> GetDashboardData(Guid userId, long minTimestamp, long maxTimestamp, Guid?[] categories = null, int timezoneOffset = 0)
        {
            // Распределение расходов по категориям
            var sumByCategoryId = new Dictionary<Guid, decimal>();
            var includedCategories = new List<ProfitCategory>();

            // Общая статистика
            decimal totalAmount = 0M;

            var query = _db.Profits.AsNoTracking().Where(x => x.OwnerId == userId);
            if (categories != null && categories.Length > 0)
            {
                if (categories.Any(x => x.Value == Guid.Empty))
                {
                    query = query.Where(x => categories.Contains(x.CategoryId) || x.CategoryId == null);
                }
                else
                {
                    query = query.Where(x => categories.Contains(x.CategoryId));
                }
            }

            // Обработка наличия периода
            if (minTimestamp != 0 && maxTimestamp != 0 && maxTimestamp > minTimestamp)
            {
                query = query.Where(x => x.CreatedAt > minTimestamp && x.CreatedAt < maxTimestamp);
            }

            query = query
                .Include(x => x.Category)
                .OrderBy(x => x.CreatedAt);

            var profits = await query.ToArrayAsync();
            if (profits.Length == 0)
            {
                return new ProfitDashboardDTO
                {
                    ChartData = [],
                    PieChartData = sumByCategoryId,
                    Categories = includedCategories
                };
            }

            
            long realMinTimestamp = profits.First().CreatedAt;
            long realMaxTimestamp = profits.Last().CreatedAt;

            long timeSegment = realMaxTimestamp - realMinTimestamp;
            var chartLabelFormat = "dd.MM.yyyy";

            if (timeSegment > 5184000 && timeSegment <= 61758000)
            {
                chartLabelFormat = "MM.yyyy";
            }
            else if (timeSegment > 61758000)
            {
                chartLabelFormat = "yyyy";
            }

            var chartSeriesDict = new Dictionary<DateTime, decimal>();
            foreach (var profit in profits)
            {
                // Заполнение по временным отрезкам для гарфика
                var createdAt = DateTimeOffset
                .FromUnixTimeSeconds(profit.CreatedAt)
                .ToOffset(TimeSpan.FromHours(timezoneOffset))
                .DateTime;
                
                DateTime segmentKey;
                if (chartLabelFormat == "yyyy")
                {
                    segmentKey = new DateTime(createdAt.Year, 1, 1);
                } 
                else if (chartLabelFormat == "MM.yyyy")
                {
                    segmentKey = new DateTime(createdAt.Year, createdAt.Month, 1);
                }
                else
                {
                    segmentKey = new DateTime(createdAt.Year, createdAt.Month, createdAt.Day);
                }

                if (!chartSeriesDict.ContainsKey(segmentKey))
                {
                    chartSeriesDict[segmentKey] = 0;
                } 

                chartSeriesDict[segmentKey] += profit.Amount;
                //=

                // Заполнение распределения по категориям (для круговой диаграммы)
                var myCategoryId = profit.CategoryId == null ? Guid.Empty : profit.CategoryId;
                if (sumByCategoryId.ContainsKey(myCategoryId.Value))
                {
                    try
                    {
                        sumByCategoryId[myCategoryId.Value] += profit.Amount;
                    }
                    catch (OverflowException)
                    {
                        sumByCategoryId[myCategoryId.Value] = decimal.MaxValue;
                    }
                }
                else
                {
                    sumByCategoryId.Add(myCategoryId.Value, profit.Amount);
                }
                //=

                // Добавление в ответ информации о категории расхода
                if (profit.Category != null)
                {
                    includedCategories.Add(profit.Category);
                }

                try
                {
                    totalAmount += profit.Amount;
                }
                catch (OverflowException)
                {
                    totalAmount = decimal.MaxValue;
                }

            }

            var chartSegments = chartSeriesDict.Select(kv => new ChartSegment
            {
                x = kv.Key.ToString(chartLabelFormat),
                y = kv.Value
            }).ToArray();

            return new ProfitDashboardDTO
            {
                TotalOperations = profits.Length,
                TotalAmount = totalAmount,
                ChartData = chartSegments,
                PieChartData = sumByCategoryId,
                Categories = includedCategories
            };
        }
    }
}
