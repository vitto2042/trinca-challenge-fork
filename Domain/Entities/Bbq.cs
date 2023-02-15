using System;
using System.Collections.Generic;
using Domain.Events;

namespace Domain.Entities
{
    public class Bbq : AggregateRoot
    {
        public string Reason { get; set; }
        public BbqStatus Status { get; set; }
        public DateTime Date { get; set; }
        public bool IsTrincasPaying { get; set; }
        public ShoppingList? ShoppingList { get; set; }
        public List<string>? ConfirmedPeople { get; set; }
        public void When(ThereIsSomeoneElseInTheMood @event)
        {
            Id = @event.Id.ToString();
            Date = @event.Date;
            Reason = @event.Reason;
            Status = BbqStatus.New;
        }

        public void When(BbqStatusUpdated @event)
        {
            if (@event.GonnaHappen)
            {
                Status = BbqStatus.PendingConfirmations;
                ShoppingList = new ShoppingList();
                ConfirmedPeople= new List<string>();
            }
            else 
                Status = BbqStatus.ItsNotGonnaHappen;

            if (@event.TrincaWillPay)
                IsTrincasPaying = true;
        }

        public void When(BbqStatusChanged @event)
        {
            Status = @event.BbqStatus;
        }

        public void When(InviteWasAccepted @event)
        {
            float meatAdd = 0.0f;
            float vegsAdd = 0.0f;

            if (@event.IsVeg)
            {
                vegsAdd = 0.6f;
            }
            else
            {
                meatAdd = 0.3f;
                vegsAdd = 0.3f;
            }

            ShoppingList = ShoppingList.Add(vegsAdd, meatAdd);
            ConfirmedPeople.Add(@event.PersonId);
        }

        public void When(InviteWasDeclined @event)
        {
            //TODO:Deve ser possível rejeitar um convite já aceito antes.
            //Se este for o caso, a quantidade de comida calculada pelo aceite anterior do convite
            //deve ser retirado da lista de compras do churrasco.
            //Se ao rejeitar, o número de pessoas confirmadas no churrasco for menor que sete,
            //o churrasco deverá ter seu status atualizado para “Pendente de confirmações”. 
            
            float meatSub = 0.0f;
            float vegsSub = 0.0f;

            if (@event.IsVeg)
            {
                vegsSub = 0.6f;
            }
            else
            {
                meatSub = 0.3f;
                vegsSub = 0.3f;
            }

            ShoppingList = ShoppingList.Sub(vegsSub, meatSub);
            ConfirmedPeople.Remove(@event.PersonId);

            if(Status == BbqStatus.Confirmed
                && ConfirmedPeople.Count < 7)
            {
                Status = BbqStatus.PendingConfirmations;
            }
        }

        public object TakeSnapshot()
        {
            return new
            {
                Id,
                Date,
                IsTrincasPaying,
                Status = Status.ToString()
            };
        }
    }
}
