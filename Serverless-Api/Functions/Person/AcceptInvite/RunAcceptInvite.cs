using Domain.Events;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using CrossCutting;

namespace Serverless_Api
{
    public partial class RunAcceptInvite
    {
        private readonly Person _user;
        private readonly SnapshotStore _snapshots;
        private readonly IPersonRepository _repository;
        private readonly IBbqRepository _bbqs;
        public RunAcceptInvite(IPersonRepository repository, Person user, IBbqRepository bbqs, SnapshotStore snapshots)
        {
            _user = user;
            _repository = repository;
            _bbqs = bbqs;
            _snapshots = snapshots;
        }

        private async Task<int> GetNumberOfAcceptedInvites(string bbqId)
        {
            int accpetedInvites = 0;
            var Lookups = await _snapshots.AsQueryable<Lookups>("Lookups").SingleOrDefaultAsync();

            foreach (var personId in Lookups.PeopleIds)
            {
                Person person = await _repository.GetAsync(personId);
                if(person.Invites
                    .Any(x => x.Id == bbqId
                    && x.Status == InviteStatus.Accepted))
                {
                    accpetedInvites++;
                }
            }

            return accpetedInvites;
        }

        [Function(nameof(RunAcceptInvite))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "person/invites/{inviteId}/accept")] HttpRequestData req, string inviteId)
        {
            InviteAnswer answer = await req.Body<InviteAnswer>();

            Person person = await _repository.GetAsync(_user.Id);

            if(person.Invites
                .Any(x => x.Id == inviteId
                    && x.Status == InviteStatus.Accepted))
            {
                return await req.CreateResponse(System.Net.HttpStatusCode.UnprocessableEntity, "Invite Already accepted");
            }

            InviteWasAccepted @acceptInviteEvent = new InviteWasAccepted { InviteId = inviteId, IsVeg = answer.IsVeg, PersonId = person.Id };

            person.Apply(@acceptInviteEvent);

            await _repository.SaveAsync(person);

            //implementar efeito do aceite do convite no churrasco
            Bbq bbq = await _bbqs.GetAsync(inviteId);
            bbq.Apply(@acceptInviteEvent);

            //quando tiver 7 pessoas ele está confirmado
            if(await GetNumberOfAcceptedInvites(inviteId) == 7)
            {
                BbqStatusChanged @chagedBbqStatusEvent = new BbqStatusChanged(BbqStatus.Confirmed);
                bbq.Apply(@chagedBbqStatusEvent);
            }

            await _bbqs.SaveAsync(bbq);

            return await req.CreateResponse(System.Net.HttpStatusCode.OK, person.TakeSnapshot());
        }
    }
}
