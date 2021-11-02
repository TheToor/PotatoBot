namespace PotatoBot.Services
{
	internal interface IService
	{
		string Name { get; }
		bool Start();
		bool Stop();
	}
}
