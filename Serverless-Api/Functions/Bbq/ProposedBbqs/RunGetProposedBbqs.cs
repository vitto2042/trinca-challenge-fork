using System.Net;
using Application.Interfaces;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Serverless_Api
{
    public partial class RunGetProposedBbqs
    {
        private readonly IBbqService _bbqService;
        public RunGetProposedBbqs(IBbqService bbqService)
        {
            _bbqService = bbqService;
        }

        [Function(nameof(RunGetProposedBbqs))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "churras")] HttpRequestData req)
        {
            object? response = await _bbqService.GetProposedAsync();

            return await req.CreateResponse(HttpStatusCode.Created, response);
        }
    }
}
