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
            /// On FIRST RUN, the constructor data will be used and saved into the .dat file.
            /// On subsequent runs, it will use whatever data is loaded from the .dat file, which
            /// can include updated data we download.
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

                        Genders = new List<string>( );
                        Genders.Add( "Unknown" );
                        Genders.Add( "Male" );
                        Genders.Add( "Female" );

                        PrayerCategories = new List<string>( );
                        PrayerCategories.Add( "Legendary Gems" );
                        PrayerCategories.Add( "Trophies" );
                        PrayerCategories.Add( "Treasure Goblins" );
                        PrayerCategories.Add( "Treasure Vault" );
                        PrayerCategories.Add( "Ponies" );
                        PrayerCategories.Add( "Whimseydale" );
                    }

                    [JsonConstructor]
                    public GeneralData( object obj )
                    {
                        Campuses = new List<string>( );
                        Genders = new List<string>( );
                        PrayerCategories = new List<string>( );
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
                    /// List of genders
                    /// </summary>
                    /// <value>The genders.</value>
                    public List<string> Genders { get; set; }

                    /// <summary>
                    /// Default list of prayer categories supported
                    /// </summary>
                    /// <value>The prayer categories.</value>
                    public List<string> PrayerCategories { get; set; }
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
