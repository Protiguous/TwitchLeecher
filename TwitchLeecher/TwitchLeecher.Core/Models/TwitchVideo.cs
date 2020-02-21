namespace TwitchLeecher.Core.Models {

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using TwitchLeecher.Shared.Extensions;

    public class TwitchVideo {

        private const String UNKNOWN_GAME = "Unknown";
        private const String UNTITLED_BROADCAST = "Untitled Broadcast";

        public String BestQuality {
            get {
                if ( this.Qualities == null || this.Qualities.Count == 0 ) {
                    return TwitchVideoQuality.UNKNOWN;
                }

                return this.Qualities.First().ResFpsString;
            }
        }

        public String Channel { get; }

        public String Game { get; }

        public Uri GameThumbnail { get; }

        public String Id { get; }

        public TimeSpan Length { get; }

        public String LengthStr {
            get {
                return this.Length.ToDaylessString();
            }
        }

        public List<TwitchVideoQuality> Qualities { get; }

        public DateTime RecordedDate { get; }

        public Uri Thumbnail { get; }

        public String Title { get; }

        public Uri Url { get; }

        public Int32 Views { get; }

        public TwitchVideo( String channel, String title, String id, String game, Int32 views, TimeSpan length,
                                                                                                                    List<TwitchVideoQuality> qualities, DateTime recordedDate, Uri thumbnail, Uri gameThumbnail, Uri url ) {
            if ( String.IsNullOrWhiteSpace( channel ) ) {
                throw new ArgumentNullException( nameof( channel ) );
            }

            if ( String.IsNullOrWhiteSpace( id ) ) {
                throw new ArgumentNullException( nameof( id ) );
            }

            if ( qualities == null || qualities.Count == 0 ) {
                throw new ArgumentNullException( nameof( qualities ) );
            }

            if ( String.IsNullOrWhiteSpace( title ) ) {
                title = UNTITLED_BROADCAST;
            }

            this.Channel = channel;
            this.Title = title;
            this.Id = id;

            if ( String.IsNullOrWhiteSpace( game ) ) {
                this.Game = UNKNOWN_GAME;
            }
            else {
                this.Game = game;
            }

            this.Views = views;
            this.Length = length;
            this.Qualities = qualities;
            this.RecordedDate = recordedDate;
            this.Thumbnail = thumbnail ?? throw new ArgumentNullException( nameof( thumbnail ) );
            this.GameThumbnail = gameThumbnail ?? throw new ArgumentNullException( nameof( gameThumbnail ) );
            this.Url = url ?? throw new ArgumentNullException( nameof( url ) );
        }
    }
}