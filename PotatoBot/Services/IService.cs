namespace PotatoBot.Services
{
    interface IService
    {
        string Name { get; }
        bool Start();
        bool Stop();
    }
}
