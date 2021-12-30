using System;
using System.Collections.Generic;

namespace PotatoBot.Modals.API.Plex
{
    public class PlexUser
	{
		public int Id { get; set; }
		public string Uuid { get; set; }
		public string Email { get; set; }
		public DateTime Joined_at { get; set; }
		public string Username { get; set; }
		public string Title { get; set; }
		public string Thumb { get; set; }
		public bool HasPassword { get; set; }
		public string AuthToken { get; set; }
		public string Authentication_token { get; set; }
		public Subscription Subscription { get; set; }
		public Roles Roles { get; set; }
		public List<object> Entitlements { get; set; }
		public DateTime ConfirmedAt { get; set; }
		public object ForumId { get; set; }
		public bool RememberMe { get; set; }
	}
}
