using Application.Interfaces;
using CrossCutting;
using Domain;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Serverless_Api
{
    public class RunGetShoppingList
    {
        private readonly Person _user;
        private readonly IBbqService _bbqService;

        public RunGetShoppingList(Person user, IBbqService bbqService)
        {
            _user = user;
            _bbqService = bbqService;
        }

        [Function(nameof(RunGetShoppingList))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "churras/{id}/shoppingList")] HttpRequestData req, string id)
        {
            object? response = await _bbqService.GetShoppingListAsync(_user.Id, id);

            return (response is null) ?
                req.CreateResponse(System.Net.HttpStatusCode.NoContent) :
                await req.CreateResponse(System.Net.HttpStatusCode.OK, response);
        }
    }
}