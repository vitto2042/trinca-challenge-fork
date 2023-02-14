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
        private readonly SnapshotStore _snapshots;
        private readonly Person _user;
        private readonly IPersonRepository _repository;
        private readonly IBbqRepository _bbqs;

        public RunGetShoppingList(Person user, IPersonRepository repository, SnapshotStore snapshots, IBbqRepository bbqs)
        {
            _user = user;
            _repository = repository;
            _snapshots = snapshots;
            _bbqs = bbqs;
        }

        [Function(nameof(RunGetShoppingList))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "churras/{id}/shoppingList")] HttpRequestData req, string id)
        {
            var person = await _repository.GetAsync(_user.Id);

            if (person == null)
                return req.CreateResponse(System.Net.HttpStatusCode.NoContent);

            if (!person.IsCoOwner)
            {
                return req.CreateResponse(System.Net.HttpStatusCode.NoContent);
            }

            Bbq bbq = await _bbqs.GetAsync(id);

            return await req.CreateResponse(System.Net.HttpStatusCode.OK, bbq.ShoppingList.TakeSnapShot());
        }
    }
}