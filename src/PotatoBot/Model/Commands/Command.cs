using System;

namespace PotatoBot.Model.Commands
{
    [AttributeUsage(AttributeTargets.Class)]
    public class Command : Attribute
    {
        public string Name { get; private set; }
        public string Description { get; set; }

        public Command(string name)
        {
            Name = name;
        }
    }
}
