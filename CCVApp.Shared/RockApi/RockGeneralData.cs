using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace CCVApp
{
    namespace Shared
    {
        namespace Network
        {
            /// <summary>
            /// Stores data that can safely be used as placeholders for
            /// areas of the app that normally require a network connection.
            /// </summary>
            public sealed class RockGeneralData
            {
                private static RockGeneralData _Instance = new RockGeneralData( );
                public static RockGeneralData Instance { get { return _Instance; } }

                const string GENERIC_DATA_FILENAME = "mobilegenericdata.dat";

                /// <summary>
                /// List of all available campuses to choose from.
                /// </summary>
                /// <value>The campuses.</value>
                public List<string> Campuses { get; set; }

                /// <summary>
                /// List of sirnames to use.
                /// </summary>
                /// <value>The titles.</value>
                public List<string> Titles { get; set; }

                /// <summary>
                /// Default news to display when there's no connection available
                /// </summary>
                /// <value>The news.</value>
                public List<RockNews> News { get; set; }

                public RockGeneralData( )
                {
                    //todo: we need to ship the app with updated versions of this data

                    // default values if there's no connection
                    // and this is never updated.
                    Campuses = new List<string>( );
                    Campuses.Add( "Peoria" );
                    Campuses.Add( "Surprise" );
                    Campuses.Add( "Scottsdale" );
                    Campuses.Add( "East Valley" );
                    Campuses.Add( "Anthem" );

                    Titles = new List<string>( );
                    Titles.Add( "Mr." );
                    Titles.Add( "Ms." );
                    Titles.Add( "Mrs." );
                    Titles.Add(" Dr." );

                    News = new List<RockNews>( );
                    News.Add( new RockNews( "News Item 1", "News Flash! Jered is a super cool dude!" ) );
                    News.Add( new RockNews( "News Item 2", "News Flash! CCV Rocks!" ) );
                    News.Add( new RockNews( "News Item 3", "News Flash! I wear sneakers in the pool." ) );
                }

                [JsonConstructor]
                public RockGeneralData( object obj )
                {
                    Campuses = new List<string>( );
                    Titles = new List<string>( );
                    News = new List<RockNews>( );
                }

                public void SaveToDevice( )
                {
                    string filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), GENERIC_DATA_FILENAME);

                    // open a stream
                    using (StreamWriter writer = new StreamWriter(filePath, false))
                    {
                        string json = JsonConvert.SerializeObject( this );
                        writer.WriteLine( json );
                    }
                }

                public void LoadFromDevice(  )
                {
                    // at startup, this should be called to allow current objects to be restored.
                    string filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), GENERIC_DATA_FILENAME);

                    // if the file exists
                    if(System.IO.File.Exists(filePath) == true)
                    {
                        // read it
                        using (StreamReader reader = new StreamReader(filePath))
                        {
                            string json = reader.ReadLine();
                            _Instance = JsonConvert.DeserializeObject<RockGeneralData>( json ) as RockGeneralData;
                        }
                    }
                }
            }
        }
    }
}
