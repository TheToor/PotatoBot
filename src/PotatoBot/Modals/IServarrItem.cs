namespace PotatoBot.Modals
{
    public interface IServarrItem
	{
		string Title { get; set; }
		string Path { get; set; }
		ushort Year { get; set; }

		string PageTitle { get; }

		string GetPosterUrl();
	}
}
