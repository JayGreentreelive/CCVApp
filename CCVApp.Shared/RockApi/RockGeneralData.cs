using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Rock.Mobile.Network;

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
                        News.Add( new RockNews( "# WISDOM", "Each day we make decisions and each decision often has many options" + 
                                                            "that are morally permissible. It’s not so much which is the \"right\""  + 
                                                            "choice, it’s which is the “wise” choice. Wisdom goes beyond right and wrong," + 
                                                            "and illuminates which choice is best based on the complex realities of life."  + 
                                                            "So join us as we seek and gain wisdom that will benefit every area of our lives.",
                                                            
                                                            "http://www.ccvonline.com/Arena/default.aspx?page=18604&topic=114",

                                                            "news_general_1.png",
                        
                                                            "news_general_1_header.png" ) );

                        News.Add( new RockNews( "FATHER & SON CAMPOUT 2014", "Bring your baseball gloves, sleeping bag, and pillow & be prepared to have a great time!", 

                                                                             "http://www.ccvonline.com/Arena/default.aspx?page=17393&eventId=23819", 

                                                                             "news_general_2.png", 

                                                                              "news_general_2_header.png" ) );


                        News.Add( new RockNews( "2015 CAMPS", "Save the date! Camp Registration opens 1/10/2015", 

                                                              "https://www.ccvonline.com/Arena/default.aspx?page=19027", 

                                                              "news_general_3.png",
                                                              
                                                              "news_general_3_header.png" ) );
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

                public void GetGeneralData( HttpRequest.RequestResult generalDataResult )
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
