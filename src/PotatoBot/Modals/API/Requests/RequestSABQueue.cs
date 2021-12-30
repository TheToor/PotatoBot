namespace PotatoBot.Modals.API.Requests
{
    public class RequestSABQueue : RequestBase
	{
		public uint Start { get; set; }
		public uint Limit { get; set; }

		public override string ToGet()
		{
			return $"start={Start}&limit={Limit}";
		}
	}
}
