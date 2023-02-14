using CrossCutting;
using Domain.Entities;
using Domain.Events;
using Domain.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Serverless_Api
{
    public partial class RunModerateBbq
    {
        private readonly SnapshotStore _snapshots;
        private readonly IPersonRepository _persons;
        private readonly IBbqRepository _repository;

        public RunModerateBbq(IBbqRepository repository, SnapshotStore snapshots, IPersonRepository persons)
        {
            _persons = persons;
            _snapshots = snapshots;
            _repository = repository;
        }

        [Function(nameof(RunModerateBbq))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "churras/{id}/moderar")] HttpRequestData req, string id)
        {
            var moderationRequest = await req.Body<ModerateBbqRequest>();

            var bbq = await _repository.GetAsync(id);

            bbq.Apply(new BbqStatusUpdated(moderationRequest.GonnaHappen, moderationRequest.TrincaWillPay));

            var lookups = await _snapshots.AsQueryable<Lookups>("Lookups").SingleOrDefaultAsync();

            var peopleMinusMods = lookups.PeopleIds;
            peopleMinusMods.RemoveAll(x => lookups.ModeratorIds.Contains(x));

            foreach (var personId in peopleMinusMods)
            {
                var person = await _persons.GetAsync(personId);
                var @event = new PersonHasBeenInvitedToBbq(bbq.Id, bbq.Date, bbq.Reason);
                person.Apply(@event);
                await _persons.SaveAsync(person);
            }

            await _repository.SaveAsync(bbq);

            return await req.CreateResponse(System.Net.HttpStatusCode.OK, bbq.TakeSnapshot());
        }
    }
}
