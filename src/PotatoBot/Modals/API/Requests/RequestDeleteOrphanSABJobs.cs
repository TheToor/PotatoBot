namespace PotatoBot.Modals.API.Requests
{
	public class RequestDeleteOrphanSABJobs : RequestBase
	{
		public override string ToGet()
		{
			return $"name=delete_all_orphan";
		}
	}
}
