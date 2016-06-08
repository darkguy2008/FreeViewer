﻿using FreeRDC.Common.Hardware;
using FreeRDC.Network;
using System;
using System.Net;

namespace FreeRDC.Services
{
    public class Host
    {
        public string AssignedTag { get; set; }
        public string Fingerprint { get; set; }

        private CommandConnection _master;
        private static CommandSerializer _cs = new CommandSerializer();

        public void ConnectToMaster(string address, int port)
        {
            Fingerprint = HWID.GenerateFingerprint();
            _master = new CommandConnection();
            _master.OnConnected += OnConnected;
            _master.OnCommandReceived += OnCommandReceived;
            _master.Client(address, port);
        }

        private void OnConnected(IPEndPoint ep)
        {
            _master.RemoteEndPoint = ep;
            _master.SendCommand(_master.RemoteEndPoint, null, null, new Commands.AUTH() { AuthType = (int)Commands.AUTH.AuthTypes.Host, Fingerprint = Fingerprint }, () =>
            {
                Console.WriteLine("Identifying HOST with fingerprint {0}...", Fingerprint);
            });
        }

        private void OnCommandReceived(IPEndPoint ep, CommandContainer command)
        {
            switch ((ECommandType)command.Type)
            {
                case ECommandType.AUTH_OK:
                    var cmd = _cs.DeserializeAs<Commands.AUTH_OK>(command.Command);
                    if (command.TagFrom == "MASTER")
                    {
                        AssignedTag = cmd.AssignedTag;
                        Console.WriteLine("Assigned tag: " + cmd.AssignedTag);
                        Console.WriteLine("Endpoint address: " + cmd.EndpointAddress);
                    }
                    break;
            }
        }
    }
}
