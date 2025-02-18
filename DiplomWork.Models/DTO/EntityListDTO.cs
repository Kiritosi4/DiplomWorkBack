

namespace DiplomWork.DTO
{
    public class EntityListDTO<T>
    {
        public IEnumerable<T> Data { get; set; }
        public int Total { get; set; }
    }
}
