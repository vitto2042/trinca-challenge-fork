using Eveneum;
using System.Net;
using CrossCutting;
using Domain.Events;
using Domain.Entities;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Application.Interfaces;

namespace Serverless_Api
{
    public partial class RunCreateNewBbq
    {
        private readonly IBbqService _bbqService;
        public RunCreateNewBbq(IBbqService bbqService)
        {
            _bbqService = bbqService;
        }

        [Function(nameof(RunCreateNewBbq))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "churras")] HttpRequestData req)
        {
            NewBbqRequest input = await req.Body<NewBbqRequest>();

            if (input == null)
            {
                return await req.CreateResponse(HttpStatusCode.BadRequest, "input is required.");
            }

            object? response = await _bbqService.CreateNewAsync(input.Date, input.Reason, input.IsTrincasPaying);

            return await req.CreateResponse(HttpStatusCode.Created, response);
        }
    }
}
