using Domain;
using Eveneum;
using CrossCutting;
using Domain.Events;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using static Domain.ServiceCollectionExtensions;
using System;
using Application.Interfaces;

namespace Serverless_Api
{
    public partial class RunDeclineInvite
    {
        private readonly Person _user;
        private readonly IPersonService _personService;

        public RunDeclineInvite(Person user, IPersonService personService)
        {
            _user = user;
            _personService = personService;
        }

        [Function(nameof(RunDeclineInvite))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "person/invites/{inviteId}/decline")] HttpRequestData req, string inviteId)
        {
            object? response = await _personService.DeclineInviteAsync(_user.Id, inviteId);

            return (response is null) ?
                req.CreateResponse(System.Net.HttpStatusCode.NoContent) :
                await req.CreateResponse(System.Net.HttpStatusCode.OK, response);
        }
    }
}
