using Application.Interfaces;
using Domain;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Serverless_Api
{
    public partial class RunGetInvites
    {
        private readonly Person _user;
        private readonly IPersonService _personService;

        public RunGetInvites(Person user, IPersonService personService)
        {
            _user = user;
            _personService = personService;
        }

        [Function(nameof(RunGetInvites))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "person/invites")] HttpRequestData req)
        {
            object? response = await _personService.GetInvitesAsync(_user.Id);

            return (response is null) ?
                req.CreateResponse(System.Net.HttpStatusCode.NoContent) :
                await req.CreateResponse(System.Net.HttpStatusCode.OK, response);
        }
    }
}
