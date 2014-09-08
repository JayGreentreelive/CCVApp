using System;
using System.Drawing;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace CCVApp
{
    namespace Shared
    {
        namespace Notes
        {
            namespace Model
            {
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
                        /// The position of this note on screen.
                        /// </summary>
                        public PointF Position { get; set; }

                        /// <summary>
                        /// The note's contents.
                        /// </summary>
                        public string Text { get; set; }

                        /// <summary>
                        /// Stores the last state of this note
                        /// </summary>
                        public bool WasOpen { get; set; }

                        public UserNoteContent( PointF position, string text, bool wasOpen )
                        {
                            Position = position;
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
                        /// The text of the reveal box so we can match it
                        /// to the appropriate reveal box in the Note
                        /// </summary>
                        /// <value>The text.</value>
                        public string Text { get; set; }

                        /// <summary>
                        /// The state of the reveal box. True if it was tapped and revealed, false otherwise.
                        /// </summary>
                        /// <value><c>true</c> if revealed; otherwise, <c>false</c>.</value>
                        public bool Revealed { get; set; }

                        public RevealBoxState( string text, bool revealed )
                        {
                            Text = text;
                            Revealed = revealed;
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
                }
            }
        }
    }
}

