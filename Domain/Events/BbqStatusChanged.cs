using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Events
{
    public class BbqStatusChanged : IEvent
    {
        public BbqStatusChanged(BbqStatus bbqStatus)
        {
            BbqStatus = bbqStatus;
        }

        public BbqStatus BbqStatus { get; set; }
    }
}
