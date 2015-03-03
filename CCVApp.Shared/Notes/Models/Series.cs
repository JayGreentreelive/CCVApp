using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace CCVApp.Shared
{
    namespace Notes.Model
    {
        /// <summary>
        /// Represents a "series" of weekly messages, like "At The Movies", "White Christmas", or "Wisdom"
        /// </summary>
        public class Series
        {
            public static char[] TrimChars = new char[] { ' ', '\n', '\t' };

            /// <summary>
            /// Represents the individual messages within each series
            /// </summary>
            public class Message
            {
                public Message( )
                {
                }

                [JsonConstructor]
                public Message( string name, string speaker, string date, string noteUrl, string audioUrl, string watchUrl, string shareUrl )
                {
                    Name = name;
                    Speaker = speaker;
                    Date = date;
                    NoteUrl = noteUrl;
                    AudioUrl = audioUrl;
                    WatchUrl = watchUrl;
                    ShareUrl = shareUrl;
                }

                /// <summary>
                /// Name of the message
                /// </summary>
                string _Name;
                public string Name
                {
                    get
                    {
                        return _Name;
                    }

                    protected set
                    {
                        _Name = value == null ? "" : value.Trim( Series.TrimChars );
                    }
                }


                /// <summary>
                /// Speaker for this message
                /// </summary>
                string _Speaker;
                public string Speaker
                {
                    get
                    {
                        return _Speaker;
                    }

                    protected set
                    {
                        _Speaker = value == null ? "" : value.Trim( Series.TrimChars );
                    }
                }

                /// <summary>
                /// The date this message was given.
                /// </summary>
                string _Date;
                public string Date
                {
                    get
                    {
                        return _Date;
                    }

                    protected set
                    {
                        _Date = value == null ? "" : value.Trim( Series.TrimChars );
                    }
                }

                /// <summary>
                /// Url of the note for this message
                /// </summary>
                string _NoteUrl;
                public string NoteUrl
                {
                    get
                    {
                        return _NoteUrl;
                    }

                    protected set
                    {
                        _NoteUrl = value == null ? "" : value.Trim( Series.TrimChars );
                    }
                }

                /// <summary>
                /// Url where the video can be found for listening
                /// </summary>
                string _AudioUrl;
                public string AudioUrl
                {
                    get
                    {
                        return _AudioUrl;
                    }

                    protected set
                    {
                        _AudioUrl = value == null ? "" : value.Trim( Series.TrimChars );
                    }
                }

                /// <summary>
                /// Url where the video can be found for watching in the browser
                /// </summary>
                string _WatchUrl;
                public string WatchUrl
                {
                    get
                    {
                        return _WatchUrl;
                    }

                    protected set
                    {
                        _WatchUrl = value == null ? "" : value.Trim( Series.TrimChars );
                    }
                }

                /// <summary>
                /// Url to use when sharing the video message with someone. (Differs from the WatchUrl
                /// because it may need to link to say the company website's embedded view page) 
                /// </summary>
                string _ShareUrl;
                public string ShareUrl
                {
                    get
                    {
                        return _ShareUrl;
                    }

                    protected set
                    {
                        _ShareUrl = value == null ? "" : value.Trim( Series.TrimChars );
                    }
                }
            }

            public Series( )
            {
            }

            [JsonConstructor]
            public Series( string name, string description, string billboardUrl, string thumbnailUrl, string dateRanges, List<Message> messages )
            {
                Name = name;
                Description = description;
                BillboardUrl = billboardUrl;
                ThumbnailUrl = thumbnailUrl;
                DateRanges = dateRanges;

                Messages = messages;
            }

            /// <summary>
            /// Name of the series
            /// </summary>
            string _Name;
            public string Name
            {
                get
                {
                    return _Name;
                }

                protected set
                {
                    _Name = value == null ? "" : value.Trim( TrimChars );
                }
            }

            /// <summary>
            /// Summary of what the messages in this series will cover
            /// </summary>
            string _Description;
            public string Description
            { 
                get
                {
                    return _Description;
                }

                protected set
                {
                    _Description = value == null ? "" : value.Trim( TrimChars );
                }
            }

            /// <summary>
            /// Url to the billboard graphic representing this series
            /// </summary>
            string _BillboardUrl;
            public string BillboardUrl
            { 
                get
                {
                    return _BillboardUrl;
                }

                protected set
                {
                    _BillboardUrl = value == null ? "" : value.Trim( TrimChars );
                }
            }

            /// <summary>
            /// Url to the thumbnail graphic representing this series
            /// </summary>
            string _ThumbnailUrl;
            public string ThumbnailUrl
            { 
                get
                {
                    return _ThumbnailUrl;
                }

                protected set
                {
                    _ThumbnailUrl = value == null ? "" : value.Trim( TrimChars );
                }
            }

            /// <summary>
            /// The range of dates this series covered.
            /// </summary>
            string _DateRanges;
            public string DateRanges
            {
                get
                {
                    return _DateRanges;
                }

                protected set
                {
                    _DateRanges = value == null ? "" : value.Trim( TrimChars );
                }
            }

            /// <summary>
            /// List of all the messages within this series
            /// </summary>
            public List<Message> Messages { get; protected set; }
        }
    }
}

