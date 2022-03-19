using PotatoBot.Extensions;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace PotatoBot.Modals.API.Plex.Library
{
    [XmlRoot(ElementName = "Video")]
    public class Video
    {

        [XmlElement(ElementName = "Director")]
        public List<Director> Director { get; set; }

        [XmlElement(ElementName = "Writer")]
        public Writer Writer { get; set; }

        [XmlElement(ElementName = "Producer")]
        public Producer Producer { get; set; }

        [XmlElement(ElementName = "Guid")]
        public List<Guid> Guid { get; set; }

        [XmlElement(ElementName = "Role")]
        public List<RolePerson> Role { get; set; }

        [XmlAttribute(AttributeName = "ratingKey")]
        public int RatingKey { get; set; }

        [XmlAttribute(AttributeName = "key")]
        public string Key { get; set; }

        [XmlAttribute(AttributeName = "parentRatingKey")]
        public int ParentRatingKey { get; set; }

        [XmlAttribute(AttributeName = "grandparentRatingKey")]
        public int GrandparentRatingKey { get; set; }

        [XmlAttribute(AttributeName = "guid")]
        public string VideoGuid { get; set; }

        [XmlAttribute(AttributeName = "parentGuid")]
        public string ParentGuid { get; set; }

        [XmlAttribute(AttributeName = "grandparentGuid")]
        public string GrandparentGuid { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }

        [XmlAttribute(AttributeName = "grandparentKey")]
        public string GrandparentKey { get; set; }

        [XmlAttribute(AttributeName = "parentKey")]
        public string ParentKey { get; set; }

        [XmlAttribute(AttributeName = "librarySectionTitle")]
        public string LibrarySectionTitle { get; set; }

        [XmlAttribute(AttributeName = "librarySectionID")]
        public int LibrarySectionID { get; set; }

        [XmlAttribute(AttributeName = "librarySectionKey")]
        public string LibrarySectionKey { get; set; }

        [XmlAttribute(AttributeName = "grandparentTitle")]
        public string GrandparentTitle { get; set; }

        [XmlAttribute(AttributeName = "parentTitle")]
        public string ParentTitle { get; set; }

        [XmlAttribute(AttributeName = "contentRating")]
        public string ContentRating { get; set; }

        [XmlAttribute(AttributeName = "summary")]
        public string Summary { get; set; }

        [XmlAttribute(AttributeName = "index")]
        public int Index { get; set; }

        [XmlAttribute(AttributeName = "parentIndex")]
        public int ParentIndex { get; set; }

        [XmlAttribute(AttributeName = "audienceRating")]
        public string AudienceRating { get; set; }

        [XmlAttribute(AttributeName = "thumb")]
        public string Thumb { get; set; }

        [XmlAttribute(AttributeName = "art")]
        public string Art { get; set; }

        [XmlAttribute(AttributeName = "parentThumb")]
        public string ParentThumb { get; set; }

        [XmlAttribute(AttributeName = "grandparentThumb")]
        public string GrandparentThumb { get; set; }

        [XmlAttribute(AttributeName = "grandparentArt")]
        public string GrandparentArt { get; set; }

        [XmlAttribute(AttributeName = "duration")]
        public int Duration { get; set; }

        [XmlAttribute(AttributeName = "originallyAvailableAt")]
        public string OriginallyAvailableAt { get; set; }

        [XmlAttribute(AttributeName = "addedAt")]
        public int AddedAt { get; set; }
        [XmlIgnore()]
        public DateTime AddedAtDate => DateTimeExtensions.UnixTimeStampToDateTime(UpdatedAt);

        [XmlAttribute(AttributeName = "updatedAt")]
        public int UpdatedAt { get; set; }
        [XmlIgnore()]
        public DateTime UpdatedAtDate => DateTimeExtensions.UnixTimeStampToDateTime(UpdatedAt);

        [XmlAttribute(AttributeName = "audienceRatingImage")]
        public string AudienceRatingImage { get; set; }

        [XmlElement(ElementName = "Media")]
        public Media Media { get; set; }

        public override bool Equals(object other)
        {
            if(this == null || other == null)
            {
                return false;
            }
            if(other is not Video video)
            {
                return false;
            }
            if(video.Key != Key)
            {
                return false;
            }
            return true;
        }
    }
}
