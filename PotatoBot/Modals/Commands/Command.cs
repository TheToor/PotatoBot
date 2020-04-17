﻿using System;

namespace PotatoBot.Modals.Commands
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Command : Attribute
    {
        public string Name { get; private set; }

        public Command(string name)
        {
            this.Name = name;
        }
    }
}
