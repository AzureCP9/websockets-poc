﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebSocketServer.Domain.Messaging.Interfaces;
public interface ITrackedMessage
{
    Guid? FrontendId { get; set; }
}
