﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace PotatoBot.Tests
{
    public class TestSetup : IDisposable
    {
        public TestSetup()
        {
            Task.Factory.StartNew(() => Program.TestMain(null), TaskCreationOptions.LongRunning);

            while(Program.ServiceManager == null)
            {
                Thread.Sleep(10);
            }
        }

        public void Dispose()
        {
            Program.ProcessExit(null, null);
            GC.SuppressFinalize(this);
        }
    }

    [CollectionDefinition("PotatoBot")]
    public class TestSetupCollection : IClassFixture<TestSetup>
    {

    }
}