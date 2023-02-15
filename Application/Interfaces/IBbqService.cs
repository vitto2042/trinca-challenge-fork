using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IBbqService
    {
        Task<object?> CreateNewAsync(DateTime date, string reason, bool isTrincasPaying);
        Task<object?> ModerateAsync(string bbqId, bool gonnaHappen, bool trincaWillPay);
        Task<object?> GetProposedAsync();
        Task<object?> GetShoppingListAsync(string personId, string bbqId);
    }
}
