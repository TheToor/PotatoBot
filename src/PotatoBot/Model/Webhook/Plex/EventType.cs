﻿namespace PotatoBot.Model.Webhook.Plex
{
    public enum EventType
    {
        // Content Events
        NewOnDeck,
        NewInLibrary,

        // Playback Events
        Play,
        Pause,
        Resume,
        Stop,
        Scrobble,
        Rate,

        // Server Events
        DatabaseBackup,
        DatabaseCorrupted,
        NewDevice,
        PlaybackStarted
    }
}
