using PotatoBot.Modals.API.Requests;

namespace PotatoBot.Modals.API.Servarr
{
    internal class ImportList : RequestBase
	{
		public bool IncludeRecommendations { get; set; } = true;
		public override string ToGet()
		{
			return $"includeRecommendations={IncludeRecommendations}";
		}
	}
}
