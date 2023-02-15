using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IPersonService
    {
        Task<object?> AcceptInviteAsync(string personId, string inviteId, bool isVeg);
        Task<object?> DeclineInviteAsync(string personId, string inviteId);
        Task<object?> GetInvitesAsync(string personId);
    }
}
