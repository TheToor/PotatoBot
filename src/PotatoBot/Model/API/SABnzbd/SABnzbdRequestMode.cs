namespace PotatoBot.Model.API.SABnzbd
{
    public enum SABnzbdRequestMode
    {
        // Gets full server status
        fullstatus,
        // Pauses the queue
        pause,
        // Resumes the queue
        resume,
        // Get queue status (& fullstats?)
        queue,
        // Server status
        status
    }
}
