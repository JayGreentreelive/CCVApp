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
                        //ALWAYS INCREMENT THIS IF UPDATING THE MODEL
                        ClientModelVersion = 0;
                        //

                        GeneralDataServerTime = DateTime.MinValue;

                        News = new List<RockNews>( );
                        NoteDB = new NoteDB( );

                        // for the hardcoded news, leave OFF the image extensions, so that we can add them with scaling for iOS.
                        DefaultNews = new List<RockNews>( );
                        DefaultNews.Add( new RockNews( "Baptisms", "Baptism is one of the most important events in the life of a Christian. If you've made a commitment to Christ, " + 
                            "it's time to take the next step and make your decision known. Baptism is the best way to express your faith and " + 
                            "reflect your life change. Find out more about baptism through Starting Point, register online or contact your neighborhood pastor.",

                            "http://www.ccvonline.com/Arena/default.aspx?page=17655&campus=1",

                            "",
                            "news_baptism_main",

                            "",
                            "news_baptism_header",

                            System.Guid.Empty ) );

                        DefaultNews.Add( new RockNews( "Starting Point", "If you’re asking yourself, “Where do I begin at CCV?” — the answer is Starting Point. " +
                            "In Starting Point you’ll find out what CCV is all about, take a deep look at the Christian " + 
                            "faith, and learn how you can get involved. Childcare is available for attending parents.",

                            "http://www.ccvonline.com/Arena/default.aspx?page=17400&campus=1",

                            "",
                            "news_startingpoint_main", 

                            "",
                            "news_startingpoint_header",
                        
                            System.Guid.Empty ) );


                        DefaultNews.Add( new RockNews( "Learn More", "Wondering what else CCV is about? Check out our website.", 

                            "http://ccv.church/Arena/default.aspx?page=17369&campus=1", 

                            "",
                            "news_learnmore_main",

                            "",
                            "news_learnmore_header",
                        
                            System.Guid.Empty ) );
                    }

                    /// <summary>
                    /// Copies the hardcoded default news into the News list,
                    /// so that there is SOMETHING for the user to see. Should only be done
                    /// if there is no news available after getting launch data.
                    /// </summary>
                    public void CopyDefaultNews( )
                    {
                        // COPY the general items into our own new list.
                        foreach ( RockNews newsItem in DefaultNews )
                        {
                            RockNews copiedNews = new RockNews( newsItem );
                            News.Add( copiedNews );

                            // also cache the compiled in main and header images so the News system can get them transparently
                            #if __IOS__
                            string mainImageName;
                            string headerImageName;
                            if( UIKit.UIScreen.MainScreen.Scale > 1 )
                            {
                                mainImageName = string.Format( "{0}/{1}@{2}x.png", Foundation.NSBundle.MainBundle.BundlePath, copiedNews.ImageName, UIKit.UIScreen.MainScreen.Scale );
                                headerImageName = string.Format( "{0}/{1}@{2}x.png", Foundation.NSBundle.MainBundle.BundlePath, copiedNews.HeaderImageName, UIKit.UIScreen.MainScreen.Scale );
                            }
                            else
                            {
                                mainImageName = string.Format( "{0}/{1}.png", Foundation.NSBundle.MainBundle.BundlePath, copiedNews.ImageName, UIKit.UIScreen.MainScreen.Scale );
                                headerImageName = string.Format( "{0}/{1}.png", Foundation.NSBundle.MainBundle.BundlePath, copiedNews.HeaderImageName, UIKit.UIScreen.MainScreen.Scale );
                            }

                            #elif __ANDROID__
                            string mainImageName = copiedNews.ImageName + ".png";
                            string headerImageName = copiedNews.HeaderImageName + ".png";
                            #endif

                            // cache the main image
                            MemoryStream stream = Rock.Mobile.Util.FileIO.AssetConvert.AssetToStream( mainImageName );
                            stream.Position = 0;
                            FileCache.Instance.SaveFile( stream, copiedNews.ImageName, FileCache.CacheFileNoExpiration );
                            stream.Dispose( );

                            // cache the header image
                            stream = Rock.Mobile.Util.FileIO.AssetConvert.AssetToStream( headerImageName );
                            stream.Position = 0;
                            FileCache.Instance.SaveFile( stream, copiedNews.HeaderImageName, FileCache.CacheFileNoExpiration );
                            stream.Dispose( );
                        }
                    }

                    /// <summary>
                    /// The last time that GeneralData was updated by the server. Each time we run,
                    /// we'll check with the server to see if there's a newer server time. If there is,
                    /// we need to download GeneralData again.
                    /// </summary>
                    /// <value>The version.</value>
                    public DateTime GeneralDataServerTime { get; set; }

                    /// <summary>
                    /// Default news to display when there's no connection available
                    /// </summary>
                    /// <value>The news.</value>
                    public List<RockNews> News { get; set; }

                    /// <summary>
                    /// The core object that stores info about the sermon notes.
                    /// </summary>
                    public NoteDB NoteDB { get; set; }

                    /// <summary>
                    /// The last time the noteDB was downloaded. This helps us know whether to
                    /// update it or not, in case the user hasn't quit the app in days.
                    /// </summary>
                    public DateTime NoteDBTimeStamp { get; set; }

                    /// <summary>
                    /// Used on the app's first run, or there's no network connection
                    /// and no valid downloaded news to use.
                    /// </summary>
                    /// <value>The default news.</value>
                    List<RockNews> DefaultNews { get; set; }

                    /// <summary>
                    /// Private to the client, this should be updated if the model
                    /// changes at all, so that we don't attempt to load an older one when upgrading the app.
                    /// </summary>
                    public int ClientModelVersion { get; protected set; }
                }
                public LaunchData Data { get; set; }

                /// <summary>
                /// True if the notedb.xml is in the process of being downloaded. This is so that
                /// if the user visits Messages WHILE we're downloading, we can wait instead of requesting it.
                /// </summary>
                public bool RequestingNoteDB { get; private set; }

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
                /// (So if NoteDB fails, GetNoteDB will be called by Messages when the user taps on it)
                /// </summary>
                /// <param name="launchDataResult">Launch data result.</param>
                public void GetLaunchData( HttpRequest.RequestResult launchDataResult )
                {
                    Console.WriteLine( "Get LaunchData" );

                    // first get the general data server time, so that we know whether we should update the
                    // general data or not.
                    RockApi.Instance.GetGeneralDataTime( delegate(System.Net.HttpStatusCode statusCode, string statusDescription, DateTime generalDataTime )
                            {
                                if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) )
                                {
                                    if( generalDataTime != DateTime.MinValue )
                                    {
                                        Data.GeneralDataServerTime = generalDataTime;
                                    }
                                }

                                // now get the news.
                                GetNews( delegate 
                                    {
                                        // chain any other required launch data actions here.
                                        Console.WriteLine( "Get LaunchData DONE" );

                                        // notify the caller now that we're done
                                        if( launchDataResult != null )
                                        {
                                            // send OK, because whether we failed or not, the caller doessn't need to care.
                                            launchDataResult( System.Net.HttpStatusCode.OK, "" );
                                        }
                                    });
                            } );
                }

                void GetNews( HttpRequest.RequestResult resultCallback )
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
                                    string featuredGuid = item.AttributeValues[ "FeatureImage" ].Value;
                                    string imageUrl = "http://rock.ccvonline.com/GetImage.ashx?Guid=" + featuredGuid;

                                    string bannerGuid = item.AttributeValues[ "PromotionImage" ].Value;
                                    string bannerUrl = "http://rock.ccvonline.com/GetImage.ashx?Guid=" + bannerGuid;

                                    string detailUrl = item.AttributeValues[ "DetailsURL" ].Value;

                                    // take either the campus guid or empty, if there is no campus assigned (meaning the news should be displayed for ALL campuses)
                                    string guidStr = item.AttributeValues[ "Campus" ].Value;
                                    Guid campusGuid = string.IsNullOrEmpty( guidStr ) == false ? new Guid( guidStr ) : Guid.Empty;

                                    RockNews newsItem = new RockNews( item.Title, item.Content, detailUrl, imageUrl, item.Title + "_main.png", bannerUrl, item.Title + "_banner.png", campusGuid );
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

                public void GetNoteDB( HttpRequest.RequestResult resultCallback )
                {
                    RequestingNoteDB = true;

                    Rock.Mobile.Network.HttpRequest request = new HttpRequest();
                    RestRequest restRequest = new RestRequest( Method.GET );
                    restRequest.RequestFormat = DataFormat.Xml;

                    request.ExecuteAsync<NoteDB>( NoteConfig.BaseURL + "note_db.xml", restRequest, 
                        delegate(System.Net.HttpStatusCode statusCode, string statusDescription, NoteDB noteModel )
                        {
                            if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true && noteModel != null )
                            {
                                Console.WriteLine( "Got NoteDB info." );
                                Data.NoteDB = noteModel;
                                Data.NoteDB.MakeURLsAbsolute( );
                                Data.NoteDBTimeStamp = DateTime.Now;

                                // download the first note so the user can immediately access it without having to wait
                                // for other crap.
                                if( Data.NoteDB.SeriesList[ 0 ].Messages.Count > 0 && 
                                    string.IsNullOrEmpty( Data.NoteDB.SeriesList[ 0 ].Messages[ 0 ].NoteUrl ) == false )
                                {
                                    CCVApp.Shared.Notes.Note.TryDownloadNote( Data.NoteDB.SeriesList[ 0 ].Messages[ 0 ].NoteUrl, Data.NoteDB.HostDomain, delegate
                                        {
                                            RequestingNoteDB = false;

                                            if ( resultCallback != null )
                                            {
                                                resultCallback( statusCode, statusDescription );
                                            }
                                        });
                                }
                                else
                                {
                                    Console.WriteLine( "No note for latest message." );

                                    RequestingNoteDB = false;

                                    if ( resultCallback != null )
                                    {
                                        resultCallback( statusCode, statusDescription );
                                    }
                                }
                            }
                            else if ( noteModel == null )
                            {
                                statusDescription = "NoteDB downloaded but failed parsing.";
                                statusCode = System.Net.HttpStatusCode.BadRequest;
                                Console.WriteLine( statusDescription );

                                RequestingNoteDB = false;

                                if ( resultCallback != null )
                                {
                                    resultCallback( statusCode, statusDescription );
                                }
                            }
                            else
                            {
                                Console.WriteLine( "NoteDB request failed." );
                                RequestingNoteDB = false;

                                if ( resultCallback != null )
                                {
                                    resultCallback( statusCode, statusDescription );
                                }
                            }
                        } );
                }

                /// <summary>
                /// Returns true if there ARE no series in the note DB, or if the last time the noteDB
                /// was downloaded was too long ago.
                /// </summary>
                public bool NeedSeriesDownload( )
                {
                    // if the series hasn't been downloaded yet, or it's older than a day, redownload it.
                    TimeSpan seriesDelta = DateTime.Now - Data.NoteDBTimeStamp;
                    if ( Data.NoteDB.SeriesList.Count == 0 || seriesDelta.TotalDays >= 1 )
                    {
                        return true;
                    }

                    return false;
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

                            try
                            {
                                // guard against the LaunchData changing and the user having old data.
                                LaunchData loadedData = JsonConvert.DeserializeObject<LaunchData>( json ) as LaunchData;
                                if( loadedData.ClientModelVersion == Data.ClientModelVersion )
                                {
                                    Data = loadedData;
                                }
                            }
                            catch( Exception )
                            {
                            }
                        }
                    }

                    // we HAVE to have news. So, if there isn't any after loading,
                    // take the general data's news.
                    if ( Data.News.Count == 0 )
                    {
                        Data.CopyDefaultNews( );
                    }
                }
            }
        }
    }
}
