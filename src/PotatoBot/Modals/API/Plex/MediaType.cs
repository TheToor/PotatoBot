namespace PotatoBot.Modals.API.Plex
{
    public enum MediaType
    {
        Unknown,

        // Should be correct
        Movie,
        Show,
        Season,

        // Should be correct
        Episode,
        Trailer,
        Comic,
        Person,
        Artist,

        // Should be correct
        Track = 10,
        PhotoAlbum,
        Picture,
        Photo,
        Clip,
        PlaylistItem,

        OtherUnknown = 100
    }
}
