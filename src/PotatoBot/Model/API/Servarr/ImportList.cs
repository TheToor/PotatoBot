using PotatoBot.Model.API.Requests;

namespace PotatoBot.Model.API.Servarr
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
