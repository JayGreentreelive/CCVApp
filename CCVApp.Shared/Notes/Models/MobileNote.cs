using System;
using System.Drawing;
using Newtonsoft.Json;

namespace CCVApp
{
    namespace Shared
    {
        namespace Notes
        {
            namespace Model
            {
                /// <summary>
                /// Represents a user's custom note. Stores the last position, whether it was open or not,
                /// and the note itself.
                /// </summary>
                public class MobileNote
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

                    public static string Serialize( PointF position, string text, bool wasOpen )
                    {
                        return JsonConvert.SerializeObject( 
                            new MobileNote()
                            {
                                Position = position,
                                Text = text,
                                WasOpen = wasOpen
                            });
                    }

                    public static MobileNote Deserialize( string json )
                    {
                        return JsonConvert.DeserializeObject<MobileNote>( json ) as MobileNote;
                    }
                }
            }
        }
    }
}

