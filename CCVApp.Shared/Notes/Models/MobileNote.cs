using System;
using System.Drawing;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Xml;
using RestSharp.Deserializers;

namespace CCVApp
{
    namespace Shared
    {
        namespace Notes
        {
            namespace Model
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
                        /// Url where the podcast can be found
                        /// </summary>
                        public string PodcastUrl { get; protected set; }
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


                /// <summary>
                /// The object that is saved / loaded and stores the user's state for a particular note.
                /// </summary>
                public class NoteState
                {
                    /// <summary>
                    /// Represents a user's custom note. Stores the last position, whether it was open or not,
                    /// and the note itself.
                    /// </summary>
                    public class UserNoteContent
                    {
                        /// <summary>
                        /// The percentage position of this note on screen. (So that should they
                        /// view this note on another device, it'll be in the appropriate spot)
                        /// </summary>
                        public float PositionPercX { get; set; }
                        public float PositionPercY { get; set; }

                        /// <summary>
                        /// The note's contents.
                        /// </summary>
                        public string Text { get; set; }

                        /// <summary>
                        /// Stores the last state of this note
                        /// </summary>
                        public bool WasOpen { get; set; }

                        public UserNoteContent( float positionPercX, float positionPercY, string text, bool wasOpen )
                        {
                            PositionPercX = positionPercX;
                            PositionPercY = positionPercY;

                            Text = text;
                            WasOpen = wasOpen;
                        }
                    }

                    /// <summary>
                    /// Represents the state of a reveal box within the notes. Tracked so
                    /// when loading the notes, we can restore any reveal box that was previously revealed.
                    /// </summary>
                    public class RevealBoxState
                    {
                        /// <summary>
                        /// The state of the reveal box. True if it was tapped and revealed, false otherwise.
                        /// </summary>
                        /// <value><c>true</c> if revealed; otherwise, <c>false</c>.</value>
                        public bool Revealed { get; set; }

                        public RevealBoxState( bool revealed )
                        {
                            Revealed = revealed;
                        }
                    }

                    /// <summary>
                    /// Represents the state of a text input within the noters. Tracked
                    /// so when loading the notes, we can restore the text that was previously in the
                    /// text input.
                    /// </summary>
                    public class TextInputState
                    {
                        /// <summary>
                        /// The text typed in the text input box.
                        /// </summary>
                        /// <value>The text.</value>
                        public string Text { get; set; }

                        public TextInputState( string text )
                        {
                            Text = text;
                        }
                    }

                    /// <summary>
                    /// List of all the user's custom notes
                    /// </summary>
                    /// <value>The mobile notes.</value>
                    public List<UserNoteContent> UserNoteContentList { get; set; }

                    /// <summary>
                    /// List of all the reveal box's current states
                    /// </summary>
                    /// <value>The reveal box states.</value>
                    public List<RevealBoxState> RevealBoxStateList { get; set; }

                    /// <summary>
                    /// List of all the text input's current text
                    /// </summary>
                    /// <value>The text input state list.</value>
                    public List<TextInputState> TextInputStateList { get; set; }
                }
            }
        }
    }
}
