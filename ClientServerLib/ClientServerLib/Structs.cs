﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClientServerLib
{
    public delegate void ResponseCallback(byte[] Response);
    public delegate byte[] HandleRequest(int ConnectionID, byte[] Message);

    public struct SRequest
    {
        public byte[] Command;
        public byte[] Response;
        public ResponseCallback Callback;
    }
}
