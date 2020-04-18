namespace PotatoBot.Modals.API.Requests
{
    public class RequestAlbum : RequestBase
    {
        public ulong ArtistId { get; set; }

        public override string ToGet()
        {
            return $"artistId={ArtistId}";
        }
    }
}
