using System.Collections.Generic;
using System.Threading.Tasks;

namespace ERPBackend.Services.Interfaces
{
    public interface IIDCardService
    {
        Task<byte[]> GenerateIDCardsAsync(List<int> employeeIds, string design);
    }
}
