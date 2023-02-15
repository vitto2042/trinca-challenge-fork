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

namespace Serverless_Api
{
    public partial class RunDeclineInvite
    {
        private readonly Person _user;
        private readonly IPersonRepository _repository;
        private readonly IBbqRepository _bbqs;

        public RunDeclineInvite(Person user, IPersonRepository repository, IBbqRepository bbqs)
        {
            _user = user;
            _repository = repository;
            _bbqs = bbqs;
        }

        private bool LastAcceptedInviteWasVeg(List<IEvent> events)
        {
            InviteWasAccepted lastAcceptedInvite = events
                .LastOrDefault(x => x.GetType() == typeof(InviteWasAccepted)) as InviteWasAccepted;

            return lastAcceptedInvite.IsVeg;
        }

        [Function(nameof(RunDeclineInvite))]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "put", Route = "person/invites/{inviteId}/decline")] HttpRequestData req, string inviteId)
        {
            Person person = await _repository.GetAsync(_user.Id);

            if (person == null)
                return req.CreateResponse(System.Net.HttpStatusCode.NoContent);

            if (person.Invites.Any(x => x.Id == inviteId && x.Status == InviteStatus.Declined))
            {
                return await req.CreateResponse(System.Net.HttpStatusCode.UnprocessableEntity, "Invite already declined");
            }

            bool inviteWasAcceptedPreviously = person.Invites.Any(x => x.Id == inviteId && x.Status == InviteStatus.Accepted);

            person.Apply(new InviteWasDeclined { InviteId = inviteId, PersonId = person.Id });

            await _repository.SaveAsync(person);

            //Implementar impacto da recusa do convite no churrasco caso ele já tivesse sido aceito antes
            if(inviteWasAcceptedPreviously)
            {
                Bbq bbq = await _bbqs.GetAsync(inviteId);
                bbq.Apply(new InviteWasDeclined {
                    InviteId = inviteId,
                    PersonId = person.Id,
                    IsVeg = LastAcceptedInviteWasVeg(await _repository.GetEventsAsync(person.Id))
                });

                await _bbqs.SaveAsync(bbq);
            }

            return await req.CreateResponse(System.Net.HttpStatusCode.OK, person.TakeSnapshot());
        }
    }
}
