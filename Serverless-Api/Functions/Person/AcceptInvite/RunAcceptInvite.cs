using Domain.Events;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using CrossCutting;
using Application.Interfaces;

namespace Serverless_Api
{
    public partial class RunAcceptInvite
    {
        private readonly Person _user;
        private readonly IPersonService _personService;
        public RunAcceptInvite(Person user, IPersonService personService)
        {
            _user = user;
            _personService = personService;
        }

        [Function(nameof(RunAcceptInvite))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "person/invites/{inviteId}/accept")] HttpRequestData req, string inviteId)
        {
            InviteAnswer answer = await req.Body<InviteAnswer>();

            object? response = await _personService.AcceptInviteAsync(_user.Id, inviteId, answer.IsVeg);

            return await req.CreateResponse(System.Net.HttpStatusCode.OK, response);
        }
    }
}
