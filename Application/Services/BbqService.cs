using Application.Interfaces;
using CrossCutting;
using Domain.Entities;
using Domain.Events;
using Domain.Repositories;
using Eveneum;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Services
{
    public class BbqService : IBbqService
    {
        private readonly IPersonRepository _persons;
        private readonly IBbqRepository _bbqs;
        private readonly SnapshotStore _snapshots;

        public BbqService(IPersonRepository persons, IBbqRepository bbqs, SnapshotStore snapshots)
        {
            _persons = persons;
            _bbqs = bbqs;
            _snapshots = snapshots;
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

        public async Task<object?> CreateNewAsync(DateTime date, string reason, bool isTrincasPaying)
        {
            var churras = new Bbq();
            churras.Apply(new ThereIsSomeoneElseInTheMood(Guid.NewGuid(), date, reason, isTrincasPaying));

            await _bbqs.SaveAsync(churras);

            var Lookups = await _snapshots.AsQueryable<Lookups>("Lookups").SingleOrDefaultAsync();

            foreach (var personId in Lookups.ModeratorIds)
            {
                Person person = await _persons.GetAsync(personId);
                person.Apply(new PersonHasBeenInvitedToBbq(churras.Id, churras.Date, churras.Reason));

                await _persons.SaveAsync(person);
            }

            return churras.TakeSnapshot();
        }

        public async Task<object?> GetProposedAsync()
        {
            var snapshots = new List<object>();
            string moderatorId = (await _snapshots.AsQueryable<Lookups>("Lookups").SingleOrDefaultAsync())
                .ModeratorIds
                .FirstOrDefault();
            Person moderator = await _persons.GetAsync(moderatorId);

            foreach (string bbqId in moderator.Invites
                .Where(i => i.Date > DateTime.Now)
                .Select(o => o.Id)
                .ToList())
            {
                Bbq bbq = await _bbqs.GetAsync(bbqId);

                if (bbq.Status != BbqStatus.ItsNotGonnaHappen)
                {
                    snapshots.Add(bbq.TakeSnapshot());
                }
            }

            return snapshots;
        }

        public async Task<object?> GetShoppingListAsync(string personId, string bbqId)
        {
            var person = await _persons.GetAsync(personId);

            if (person == null || !person.IsCoOwner)
                return null;

            Bbq bbq = await _bbqs.GetAsync(bbqId);

            return bbq.ShoppingList.TakeSnapShot();
        }

        public async Task<object?> ModerateAsync(string bbqId, bool gonnaHappen, bool trincaWillPay)
        {
            Bbq bbq = await _bbqs.GetAsync(bbqId);

            bbq.Apply(new BbqStatusUpdated(gonnaHappen, trincaWillPay));

            var lookups = await _snapshots.AsQueryable<Lookups>("Lookups").SingleOrDefaultAsync();

            if (gonnaHappen)
            {
                await SendInvites(bbq, GetPeopleMinusModerators(lookups));
            }
            else
            {
                await RejectAllPendingInvites(bbq.Id, lookups.ModeratorIds);
            }

            await _bbqs.SaveAsync(bbq);

            return bbq.TakeSnapshot();
        }
    }
}
