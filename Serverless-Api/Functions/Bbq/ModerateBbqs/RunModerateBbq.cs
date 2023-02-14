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

        private List<string> GetPeopleMinusModerators(Lookups lookups)
        {
            List<string> peopleMinusMods = new List<string>();
            peopleMinusMods.AddRange(lookups.PeopleIds);
            peopleMinusMods.RemoveAll(x => lookups.ModeratorIds.Contains(x));

            return peopleMinusMods;
        }

        private async Task SendInvites(Bbq bbq, List<string> invitedPeople)
        {
            foreach (var personId in invitedPeople)
            {
                Person person = await _persons.GetAsync(personId);
                PersonHasBeenInvitedToBbq @event = new PersonHasBeenInvitedToBbq(bbq.Id, bbq.Date, bbq.Reason);
                person.Apply(@event);
                await _persons.SaveAsync(person);
            }
        }

        private async Task RejectAllPendingInvites(string bbqId, List<string> people)
        {
            foreach (var personId in people)
            {
                Person person = await _persons.GetAsync(personId);
                List<InviteWasDeclined> @events = person.Invites
                    .Where(x => x.Id == bbqId
                            && x.Status == InviteStatus.Pending)
                    .Select(x => new InviteWasDeclined { InviteId = x.Id, PersonId = person.Id })
                    .ToList();
                @events.ForEach(x => person.Apply(x));
                await _persons.SaveAsync(person);
            }
        }

        [Function(nameof(RunModerateBbq))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "churras/{id}/moderar")] HttpRequestData req, string id)
        {
            ModerateBbqRequest moderationRequest = await req.Body<ModerateBbqRequest>();

            Bbq bbq = await _repository.GetAsync(id);

            bbq.Apply(new BbqStatusUpdated(moderationRequest.GonnaHappen, moderationRequest.TrincaWillPay));

            var lookups = await _snapshots.AsQueryable<Lookups>("Lookups").SingleOrDefaultAsync();

            if (moderationRequest.GonnaHappen)
            {
                await SendInvites(bbq, GetPeopleMinusModerators(lookups));
            }
            else
            {
                await RejectAllPendingInvites(bbq.Id, lookups.ModeratorIds);
            }

            await _repository.SaveAsync(bbq);

            return await req.CreateResponse(System.Net.HttpStatusCode.OK, bbq.TakeSnapshot());
        }
    }
}
