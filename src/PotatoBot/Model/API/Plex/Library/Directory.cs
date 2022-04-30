using System.Collections.Generic;
using System.Xml.Serialization;

namespace PotatoBot.Model.API.Plex.Library
{
    [XmlRoot(ElementName = "Directory")]
    public class Directory
    {
        [XmlElement(ElementName = "Guid")]
        public List<Guid> Guid { get; set; }

        [XmlElement(ElementName = "Role")]
        public List<RolePerson> Role { get; set; }

        [XmlElement(ElementName = "Location")]
        public Location Location { get; set; }

        [XmlAttribute(AttributeName = "ratingKey")]
        public int RatingKey { get; set; }

        [XmlAttribute(AttributeName = "key")]
        public string Key { get; set; }

        [XmlAttribute(AttributeName = "guid")]
        public string DirectoryGuid { get; set; }

        [XmlAttribute(AttributeName = "studio")]
        public string Studio { get; set; }

        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "title")]
        public string Title { get; set; }

        [XmlAttribute(AttributeName = "librarySectionTitle")]
        public string LibrarySectionTitle { get; set; }

        [XmlAttribute(AttributeName = "librarySectionID")]
        public int LibrarySectionID { get; set; }

        [XmlAttribute(AttributeName = "librarySectionKey")]
        public string LibrarySectionKey { get; set; }

        [XmlAttribute(AttributeName = "contentRating")]
        public string ContentRating { get; set; }

        [XmlAttribute(AttributeName = "summary")]
        public string Summary { get; set; }

        [XmlAttribute(AttributeName = "index")]
        public int Index { get; set; }

        [XmlAttribute(AttributeName = "audienceRating")]
        public string AudienceRating { get; set; }

        [XmlAttribute(AttributeName = "year")]
        public int Year { get; set; }

        [XmlAttribute(AttributeName = "thumb")]
        public string Thumb { get; set; }

        [XmlAttribute(AttributeName = "art")]
        public string Art { get; set; }

        [XmlAttribute(AttributeName = "theme")]
        public string Theme { get; set; }

        [XmlAttribute(AttributeName = "duration")]
        public int Duration { get; set; }

        [XmlAttribute(AttributeName = "originallyAvailableAt")]
        public string OriginallyAvailableAt { get; set; }

        [XmlAttribute(AttributeName = "leafCount")]
        public int LeafCount { get; set; }

        [XmlAttribute(AttributeName = "viewedLeafCount")]
        public int ViewedLeafCount { get; set; }

        [XmlAttribute(AttributeName = "childCount")]
        public int ChildCount { get; set; }

        [XmlAttribute(AttributeName = "addedAt")]
        public int AddedAt { get; set; }

        [XmlAttribute(AttributeName = "updatedAt")]
        public int UpdatedAt { get; set; }

        [XmlAttribute(AttributeName = "audienceRatingImage")]
        public string AudienceRatingImage { get; set; }

        [XmlAttribute(AttributeName = "primaryExtraKey")]
        public string PrimaryExtraKey { get; set; }

        [XmlAttribute(AttributeName = "parentRatingKey")]
        public int ParentRatingKey { get; set; }

        [XmlAttribute(AttributeName = "parentGuid")]
        public string ParentGuid { get; set; }

        [XmlAttribute(AttributeName = "parentStudio")]
        public string ParentStudio { get; set; }

        [XmlAttribute(AttributeName = "parentKey")]
        public string ParentKey { get; set; }

        [XmlAttribute(AttributeName = "parentTitle")]
        public string ParentTitle { get; set; }

        [XmlAttribute(AttributeName = "parentIndex")]
        public int ParentIndex { get; set; }

        [XmlAttribute(AttributeName = "parentYear")]
        public int ParentYear { get; set; }

        [XmlAttribute(AttributeName = "parentThumb")]
        public string ParentThumb { get; set; }

        [XmlAttribute(AttributeName = "parentTheme")]
        public string ParentTheme { get; set; }

        [XmlElement(ElementName = "Genre")]
        public List<Genre> Genre { get; set; }

        [XmlAttribute(AttributeName = "titleSort")]
        public string TitleSort { get; set; }

        [XmlAttribute(AttributeName = "tagline")]
        public string Tagline { get; set; }
    }
}
