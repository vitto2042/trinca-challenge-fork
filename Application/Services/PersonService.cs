using Application.Interfaces;
using Domain;
using Domain.Entities;
using Domain.Events;
using Domain.Repositories;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class PersonService : IPersonService
    {
        private readonly IPersonRepository _persons;
        private readonly IBbqRepository _bbqs;
        public PersonService(IPersonRepository persons, IBbqRepository bbqs)
        {
            _persons = persons;
            _bbqs = bbqs;
        }

        private async Task<bool> LastAcceptedInviteWasVeg(string personId)
        {
            List<IEvent> events = await _persons.GetEventsAsync(personId);
            InviteWasAccepted lastAcceptedInvite = events
                .LastOrDefault(x => x.GetType() == typeof(InviteWasAccepted)) as InviteWasAccepted;

            return lastAcceptedInvite.IsVeg;
        }

        public async Task<object?> AcceptInviteAsync(string personId, string inviteId, bool isVeg)
        {
            Person person = await _persons.GetAsync(personId);

            if (person.Invites
                .Any(x => x.Id == inviteId
                    && x.Status == InviteStatus.Accepted))
            {
                return person.TakeSnapshot();
            }

            InviteWasAccepted @acceptInviteEvent = new InviteWasAccepted { InviteId = inviteId, IsVeg = isVeg, PersonId = person.Id };

            person.Apply(@acceptInviteEvent);

            await _persons.SaveAsync(person);

            //implementar efeito do aceite do convite no churrasco
            Bbq bbq = await _bbqs.GetAsync(inviteId);
            bbq.Apply(@acceptInviteEvent);

            //quando tiver 7 pessoas ele está confirmado
            if (bbq.ConfirmedPeople.Count == 7)
            {
                BbqStatusChanged @chagedBbqStatusEvent = new BbqStatusChanged(BbqStatus.Confirmed);
                bbq.Apply(@chagedBbqStatusEvent);
            }

            await _bbqs.SaveAsync(bbq);

            return person.TakeSnapshot();
        }

        public async Task<object?> DeclineInviteAsync(string personId, string inviteId)
        {
            Person person = await _persons.GetAsync(personId);

            if (person == null)
                return null;

            if (person.Invites.Any(x => x.Id == inviteId && x.Status == InviteStatus.Declined))
            {
                return person.TakeSnapshot();
            }

            bool inviteWasAcceptedPreviously = person.Invites.Any(x => x.Id == inviteId && x.Status == InviteStatus.Accepted);

            person.Apply(new InviteWasDeclined { InviteId = inviteId, PersonId = person.Id });

            await _persons.SaveAsync(person);

            //Implementar impacto da recusa do convite no churrasco caso ele já tivesse sido aceito antes
            if (inviteWasAcceptedPreviously)
            {
                Bbq bbq = await _bbqs.GetAsync(inviteId);
                bbq.Apply(new InviteWasDeclined
                {
                    InviteId = inviteId,
                    PersonId = person.Id,
                    IsVeg = await LastAcceptedInviteWasVeg(person.Id)
                });

                await _bbqs.SaveAsync(bbq);
            }

            return person.TakeSnapshot();
        }

        public async Task<object?> GetInvitesAsync(string personId)
        {
            var person = await _persons.GetAsync(personId);

            if (person == null)
                return null;

            return person.TakeSnapshot();
        }
    }
}
