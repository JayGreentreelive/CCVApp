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

                //Todo: This will actually exist in Rock.Client.Models
                public class GeneralData
                {
                    public GeneralData( )
                    {
                        //todo: we need to ship the app with updated versions of this data
                        Version = 0;

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
                        News.Add( new RockNews( "News Item 1", "News Flash! CCV Rocks!", "news_general_1.png" ) );
                        News.Add( new RockNews( "News Item 2", "News Flash! Jered is a super cool dude!", "news_general_2.png" ) );
                        News.Add( new RockNews( "News Item 3", "News Flash! I wear sneakers in the pool.", "news_general_3.png" ) );
                    }

                    [JsonConstructor]
                    public GeneralData( object obj )
                    {
                        Campuses = new List<string>( );
                        Titles = new List<string>( );
                        News = new List<RockNews>( );
                    }

                    /// <summary>
                    /// Current version of the General Data. If Rock tells us there's a version
                    /// with a greater value than this, we will update.
                    /// </summary>
                    /// <value>The version.</value>
                    public int Version { get; set; }

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
                }
                public GeneralData Data { get; set; }

                public RockGeneralData( )
                {
                    Data = new GeneralData( );
                }

                public void GetGeneralData( RockApi.RequestResult generalDataResult )
                {
                    Console.WriteLine( "Get GeneralData" );
                    RockApi.Instance.GetGeneralData(delegate(System.Net.HttpStatusCode statusCode, string statusDescription, GeneralData model)
                        {
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                Data = model;

                                // save!
                                SaveToDevice( );
                            }

                            Console.WriteLine( "GeneralData Received With Status {0}", statusCode );

                            // notify the caller
                            if( generalDataResult != null )
                            {
                                generalDataResult( statusCode, statusDescription );
                            }
                        });
                }

                public void SaveToDevice( )
                {
                    string filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), GENERIC_DATA_FILENAME);

                    // open a stream
                    using (StreamWriter writer = new StreamWriter(filePath, false))
                    {
                        string json = JsonConvert.SerializeObject( Data );
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
                            Data = JsonConvert.DeserializeObject<GeneralData>( json ) as GeneralData;
                        }
                    }
                }
            }
        }
    }
}
