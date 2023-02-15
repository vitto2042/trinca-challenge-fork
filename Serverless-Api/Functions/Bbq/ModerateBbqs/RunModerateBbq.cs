using Application.Interfaces;
using CrossCutting;
using Domain.Entities;
using Domain.Events;
using Domain.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace Serverless_Api
{
    public partial class RunModerateBbq
    {
        private readonly IBbqService _bbqService;

        public RunModerateBbq(IBbqService bbqService)
        {
            _bbqService = bbqService;
        }

        [Function(nameof(RunModerateBbq))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "churras/{id}/moderar")] HttpRequestData req, string id)
        {
            ModerateBbqRequest input = await req.Body<ModerateBbqRequest>();

            if (input == null)
            {
                return await req.CreateResponse(HttpStatusCode.BadRequest, "input is required.");
            }

            object? response = await _bbqService.ModerateAsync(id, input.GonnaHappen, input.TrincaWillPay);

            return await req.CreateResponse(System.Net.HttpStatusCode.OK, response);
        }
    }
}
