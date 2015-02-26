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

                public class GeneralData
                {
                    public GeneralData( )
                    {
                        Version = 0;

                        // default values if there's no connection
                        // and this is never updated.
                        Campuses = new List<Rock.Client.Campus>( );
                        Campuses.Add( new Rock.Client.Campus( ) { Name = "Peoria", Id = 1 } );
                        Campuses.Add( new Rock.Client.Campus( ) { Name = "Surprise", Id = 5 } );
                        Campuses.Add( new Rock.Client.Campus( ) { Name = "Scottsdale", Id = 6 } );
                        Campuses.Add( new Rock.Client.Campus( ) { Name = "East Valley", Id = 7 } );
                        Campuses.Add( new Rock.Client.Campus( ) { Name = "Anthem", Id = 8 } );

                        Genders = new List<string>( );
                        Genders.Add( "Unknown" );
                        Genders.Add( "Male" );
                        Genders.Add( "Female" );

                        PrayerCategories = new List< Rock.Client.Category >( );
                        PrayerCategories.Add( new Rock.Client.Category( ) { Name = "Addictive Behavior", Id = 109 } );
                        PrayerCategories.Add( new Rock.Client.Category( ) { Name = "Comfort/Grief", Id = 110 } );
                        PrayerCategories.Add( new Rock.Client.Category( ) { Name = "Current Events", Id = 111 } );
                        PrayerCategories.Add( new Rock.Client.Category( ) { Name = "Depression/Anxiety", Id = 112 } );
                        PrayerCategories.Add( new Rock.Client.Category( ) { Name = "Family Issues", Id = 113 } );
                    }

                    [JsonConstructor]
                    public GeneralData( object obj )
                    {
                        Campuses = new List<Rock.Client.Campus>( );
                        Genders = new List<string>( );

                        PrayerCategories = new List< Rock.Client.Category >( );
                    }

                    /// <summary>
                    /// Helper method for converting a campus' id to its name
                    /// </summary>
                    /// <returns>The identifier to name.</returns>
                    public string CampusIdToName( int campusId )
                    {
                        // guard against old, bad values.
                        Rock.Client.Campus campusObj = Campuses.Find( c => c.Id == campusId );
                        return campusObj != null ? campusObj.Name : Campuses[ 0 ].Name;
                    }

                    /// <summary>
                    /// Helper method for converting a campus' name to its ID
                    /// </summary>
                    /// <returns>The name to identifier.</returns>
                    public int CampusNameToId( string campusName )
                    {
                        Rock.Client.Campus campusObj = Campuses.Find( c => c.Name == campusName );
                        return campusObj != null ? campusObj.Id : 0;
                    }

                    /// <summary>
                    /// Helper method for converting a prayer category ID to its name
                    /// </summary>
                    public string PrayerIdToCategory( int categoryId )
                    {
                        // guard against old, bad values.
                        Rock.Client.Category categoryObj = PrayerCategories.Find( c => c.Id == categoryId );
                        return categoryObj != null ? categoryObj.Name : PrayerCategories[ 0 ].Name;
                    }

                    /// <summary>
                    /// Helper method for converting a prayer category name to its ID
                    /// </summary>
                    public int PrayerCategoryToId( string categoryName )
                    {
                        Rock.Client.Category categoryObj = PrayerCategories.Find( c => c.Name == categoryName );
                        return categoryObj != null ? categoryObj.Id : 0;
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
                    public List<Rock.Client.Campus> Campuses { get; set; }

                    /// <summary>
                    /// List of genders
                    /// </summary>
                    /// <value>The genders.</value>
                    public List<string> Genders { get; set; }

                    /// <summary>
                    /// Default list of prayer categories supported
                    /// </summary>
                    /// <value>The prayer categories.</value>
                    public List<Rock.Client.Category> PrayerCategories { get; set; }
                }
                public GeneralData Data { get; set; }

                public RockGeneralData( )
                {
                    Data = new GeneralData( );
                }

                public void GetGeneralData( HttpRequest.RequestResult generalDataResult )
                {
                    // now get our campuses.
                    RockApi.Instance.GetCampuses( delegate(System.Net.HttpStatusCode statusCode, string statusDescription, List<Rock.Client.Campus> campusList )
                        {
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                Data.Campuses = campusList;
                            }

                            // Chain other things here as needed
                            RockApi.Instance.GetPrayerCategories( 
                                delegate( System.Net.HttpStatusCode prayerStatusCode, string prayerStatusDescription, List<Rock.Client.Category> categoryList )
                                {
                                    if ( Rock.Mobile.Network.Util.StatusInSuccessRange( prayerStatusCode ) == true )
                                    {
                                        Data.PrayerCategories = categoryList;
                                    }

                                    // save!
                                    SaveToDevice( );

                                    // notify the caller
                                    if( generalDataResult != null )
                                    {
                                        generalDataResult( prayerStatusCode, prayerStatusDescription );
                                    }
                                });
                        } );
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

                            // catch a load exception and abort. Then we'll simply use default data.
                            try
                            {
                                // only take the general data if our version matches. Otherwise, make them start fresh.
                                GeneralData loadedData = JsonConvert.DeserializeObject<GeneralData>( json ) as GeneralData;
                                if( Data.Version == loadedData.Version )
                                {
                                    Data = loadedData;
                                }
                            }
                            catch( Exception e )
                            {
                                Console.WriteLine( string.Format( "{0}", e) );
                            }
                        }
                    }
                }
            }
        }
    }
}
