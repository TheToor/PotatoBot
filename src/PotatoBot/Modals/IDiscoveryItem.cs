namespace PotatoBot.Modals
{
    public interface IDiscoveryItem
	{
		public bool IsExcluded { get; set; }
		public bool IsExisting { get; set; }
		public bool IsRecommendation { get; set; }
	}
}
