using System;
using System.Collections.Generic;

namespace CCVApp.Shared
{
    namespace Notes.Model
    {
        /// <summary>
        /// Represents a "series" of weekly messages, like "At The Movies", "White Christmas", or "Wisdom"
        /// </summary>
        public class Series
        {
            /// <summary>
            /// Represents the individual messages within each series
            /// </summary>
            public class Message
            {
                /// <summary>
                /// Name of the message
                /// </summary>
                public string Name { get; protected set; }

                /// <summary>
                /// Summary of what the message was about
                /// </summary>
                public string Description { get; protected set; }

                /// <summary>
                /// Url of the note for this message
                /// </summary>
                public string NoteUrl { get; protected set; }

                /// <summary>
                /// Url where the video can be found for watching in the browser
                /// </summary>
                public string WatchUrl { get; protected set; }

                /// <summary>
                /// Url to use when sharing the video message with someone. (Differs from the WatchUrl
                /// because it may need to link to say the company website's embedded view page) 
                /// </summary>
                /// <value>The share URL.</value>
                public string ShareUrl { get; protected set; }
            }

            /// <summary>
            /// Name of the series
            /// </summary>
            /// <value>The name.</value>
            public string Name { get; protected set; }

            /// <summary>
            /// Summary of what the messages in this series will cover
            /// </summary>
            /// <value>The description.</value>
            public string Description { get; protected set; }

            /// <summary>
            /// Url to the billboard graphic representing this series
            /// </summary>
            /// <value>The billboard.</value>
            public string BillboardUrl { get; protected set; }

            /// <summary>
            /// The range of dates this series covered.
            /// </summary>
            /// <value>The date range.</value>
            public string DateRanges { get; protected set; }

            /// <summary>
            /// List of all the messages within this series
            /// </summary>
            /// <value>The messages.</value>
            public List<Message> Messages { get; protected set; }
        }
    }
}

