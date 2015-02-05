using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Rock.Mobile.Network;
using CCVApp.Shared.Notes.Model;
using RestSharp;
using CCVApp.Shared.Config;

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
            public sealed class RockLaunchData
            {
                private static RockLaunchData _Instance = new RockLaunchData( );
                public static RockLaunchData Instance { get { return _Instance; } }

                const string LAUNCH_DATA_FILENAME = "mobilelaunchdata.dat";

                // wrapper for managing the data obtained at launch
                public class LaunchData
                {
                    public LaunchData( )
                    {
                        GeneralDataVersion = 0;

                        News = new List<RockNews>( );
                        Series = new List<Series>( );
                    }

                    /// <summary>
                    /// Current version of the General Data. If Rock tells us there's a version
                    /// with a greater value than this, we will update.
                    /// </summary>
                    /// <value>The version.</value>
                    public int GeneralDataVersion { get; set; }

                    /// <summary>
                    /// Default news to display when there's no connection available
                    /// </summary>
                    /// <value>The news.</value>
                    public List<RockNews> News { get; set; }

                    /// <summary>
                    /// The list of sermon series and messages. This goes here so that
                    /// we can shortcut the user to the latest message from the main page if they want.
                    /// It also allows us to store them so they don't need to be downloaded every time they
                    /// visit the Messages page.
                    /// </summary>
                    /// <value>The series.</value>
                    public List<Series> Series { get; set; }
                }
                public LaunchData Data { get; set; }

                /// <summary>
                /// True if the series.xml is in the process of being downloaded. This is so that
                /// if the user visits Messages WHILE we're downloading, we can wait instead of requesting it.
                /// </summary>
                /// <value><c>true</c> if requesting series; otherwise, <c>false</c>.</value>
                public bool RequestingSeries { get; private set; }

                public RockLaunchData( )
                {
                    Data = new LaunchData( );
                }

                /// <summary>
                /// The news UI should immediatley hook into this on launch so we can notify when news is ready for display.
                /// NOT CURRENTLY USING IT. ONLY NEEDED IF WE WANT TO UPDATE THE NEWS _WHILE_ THE USER IS SITTING ON THE NEWS PAGE.
                /// </summary>
                public delegate void NewsItemsDownloaded( );
                public NewsItemsDownloaded NewsItemsDownloadedCallback { get; set; }

                /// <summary>
                /// Wrapper function for getting the basic things we need at launch (news, notes, etc.)
                /// If for some reason one of these fails, they will be called independantly by the appropriate systems
                /// (So if series fails, GetSeries will be called by Messages when the user taps on it)
                /// </summary>
                /// <param name="launchDataResult">Launch data result.</param>
                public void GetLaunchData( HttpRequest.RequestResult launchDataResult )
                {
                    Console.WriteLine( "Get LaunchData" );

                    // request the initial news
                    GetNews( delegate 
                        {
                            // whether it worked or not, now grab the series info. 
                            GetSeries( delegate
                                {
                                    // notify the caller now that we're done
                                    if( launchDataResult != null )
                                    {
                                        // send OK, because whether we failed or not, the caller doessn't need to care.
                                        launchDataResult( System.Net.HttpStatusCode.OK, "" );
                                    }
                                });
                        } );
                }

                public void GetNews( HttpRequest.RequestResult resultCallback )
                {
                    RockApi.Instance.GetNews( delegate(System.Net.HttpStatusCode statusCode, string statusDescription, List<Rock.Client.ContentChannelItem> model )
                        {
                            if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                Console.WriteLine( "Got news from Rock." );

                                // setup the new rock news
                                Data.News.Clear( );
                                foreach( Rock.Client.ContentChannelItem item in model )
                                {
                                    RockNews newsItem = new RockNews( item.Title, item.Content, "http://www.yahoo.com", "", "" );
                                    Data.News.Add( newsItem );
                                }
                            }
                            else
                            {
                                Console.WriteLine( "News request failed." );
                            }

                            if ( resultCallback != null )
                            {
                                resultCallback( statusCode, statusDescription );
                            }
                        } );
                }

                public void GetSeries( HttpRequest.RequestResult resultCallback )
                {
                    RequestingSeries = true;

                    Rock.Mobile.Network.HttpRequest request = new HttpRequest();
                    RestRequest restRequest = new RestRequest( Method.GET );
                    restRequest.RequestFormat = DataFormat.Xml;

                    request.ExecuteAsync<List<Series>>( NoteConfig.BaseURL + "series.xml", restRequest, 
                        delegate(System.Net.HttpStatusCode statusCode, string statusDescription, List<Series> seriesModel )
                        {
                            if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true && seriesModel != null )
                            {
                                Console.WriteLine( "Got series info." );
                                Data.Series = seriesModel;
                            }
                            else if ( seriesModel == null )
                            {
                                statusDescription = "Series downloaded but failed parsing.";
                                statusCode = System.Net.HttpStatusCode.BadRequest;
                                Console.WriteLine( statusDescription );
                            }
                            else
                            {
                                Console.WriteLine( "Series request failed." );
                            }

                            RequestingSeries = false;

                            if ( resultCallback != null )
                            {
                                resultCallback( statusCode, statusDescription );
                            }
                        } );
                }

                public void SaveToDevice( )
                {
                    string filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), LAUNCH_DATA_FILENAME);

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
                    string filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), LAUNCH_DATA_FILENAME);

                    // if the file exists
                    if ( System.IO.File.Exists( filePath ) == true )
                    {
                        // read it
                        using ( StreamReader reader = new StreamReader( filePath ) )
                        {
                            string json = reader.ReadLine( );
                            Data = JsonConvert.DeserializeObject<LaunchData>( json ) as LaunchData;
                        }
                    }

                    // we HAVE to have news. So, if there isn't any after loading,
                    // take the general data's news.
                    if ( Data.News.Count == 0 )
                    {
                        // COPY the general items into our own new list.
                        foreach ( RockNews newsItem in RockGeneralData.Instance.Data.News )
                        {
                            RockNews copiedNews = new RockNews( newsItem );
                            Data.News.Add( copiedNews );
                        }
                    }
                }
            }
        }
    }
}
