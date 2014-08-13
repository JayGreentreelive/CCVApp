using System;
using System.Drawing;
using Newtonsoft.Json;

namespace Notes
{
    namespace Model
    {
        public class MobileNote
        {
            public PointF Position { get; set; }
            public string Text { get; set; }
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

