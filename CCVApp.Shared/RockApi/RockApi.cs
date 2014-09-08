using System;
using RestSharp;
using System.IO;
using Newtonsoft.Json;
using System.Net;

namespace CCVApp
{
    namespace Shared
    {
        namespace Network
        {
            /// <summary>
            /// Rock API contains methods for making REST calls to Rock.
            /// This should only be used directly if an object doesn't
            /// that does what is needed. Ex: RockMobileUser should be used
            /// in place of directly calling Profile end points when needing
            /// to manage the primary user's account.
            /// </summary>
            public sealed class RockApi
            {
                /// <summary>
                /// The instance of RockAPI
                /// </summary>
                static RockApi _Instance = new RockApi();
                public static RockApi  Instance { get { return _Instance; } }

                /// <summary>
                /// The timeout after which the REST call attempt is given up.
                /// </summary>
                const int RequestTimeoutMS = 15000;

                const string COOKIE_FILENAME = "cookies.dat";
                const string BaseUrl = "http://rock.ccvonline.com/api";

                /// <summary>
                /// End point for logging in
                /// </summary>
                const string AuthLoginEndPoint = "Auth/Login";

                /// <summary>
                /// End point for retrieving a Person object
                /// </summary>
                const string GetProfileEndPoint = "People/GetByUserName/";

                /// <summary>
                /// End point for updating a Person object
                /// </summary>
                const string PutProfileEndPoint = "People/";

                /// <summary>
                /// Stores the cookies received from Rock
                /// </summary>
                /// <value>The cookie container.</value>
                CookieContainer CookieContainer { get; set; }

                /// <summary>
                /// Request Response delegate that does not require a returned object
                /// </summary>
                public delegate void RequestResult(System.Net.HttpStatusCode statusCode, string statusDescription);

                /// <summary>
                /// Request response delegate that does require a returned object
                /// </summary>
                public delegate void RequestResult<TModel>(System.Net.HttpStatusCode statusCode, string statusDescription, TModel model);

                RockApi( )
                {
                    CookieContainer = new System.Net.CookieContainer();
                }

                public void Login( string username, string password, RequestResult resultHandler )
                {
                    RestRequest request = new RestRequest( Method.POST );
                    request.Resource = AuthLoginEndPoint;

                    request.AddParameter( "Username", username );
                    request.AddParameter( "Password", password );
                    request.AddParameter( "Persisted", true );

                    ExecuteAsync( request, delegate(HttpStatusCode statusCode, string statusDescription, object model) 
                        {
                            // if login was a success, save our cookie
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                SaveCookieToDevice();
                            }

                            // either way, notifiy the caller
                            if( resultHandler != null )
                            {
                                resultHandler( statusCode, statusDescription );
                            }
                        });
                }

                public void Logout()
                {
                    // reset our cookies
                    CookieContainer = new CookieContainer();
                }

                public void GetProfile( string userName, RequestResult<Rock.Client.Person> resultHandler )
                {
                    // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                    RestRequest request = new RestRequest( Method.GET );
                    request.Resource = GetProfileEndPoint;
                    request.Resource += string.IsNullOrEmpty( userName ) == true ? RockMobileUser.Instance.Username : userName;

                    ExecuteAsync<Rock.Client.Person>( request, resultHandler);
                }

                public void UpdateProfile( Rock.Client.Person person, RequestResult resultHandler )
                {
                    // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                    RestRequest request = new RestRequest( Method.PUT );
                    request.Resource = PutProfileEndPoint;
                    request.Resource += person.Id;

                    request.RequestFormat = DataFormat.Json;
                    request.AddBody( person );

                    ExecuteAsync( request, resultHandler);
                }

                public void GetLaunchData( RequestResult<RockLaunchData.LaunchData> resultHandler )
                {
                    // todo: add a "get LaunchData" end point.
                    resultHandler( HttpStatusCode.OK, "Success", RockLaunchData.Instance.Data );
                }

                public void GetGeneralData( RequestResult<RockGeneralData.GeneralData> resultHandler )
                {
                    // todo: add a "get GeneralData" end point.
                    resultHandler( HttpStatusCode.OK, "Success", RockGeneralData.Instance.Data );
                }

                /// <summary>
                /// Wrapper for ExecuteAsync<> that requires no generic Type.
                /// </summary>
                /// <param name="request">Request.</param>
                /// <param name="resultHandler">Result handler.</param>
                private void ExecuteAsync( RestRequest request, RequestResult resultHandler )
                {
                    ExecuteAsync<object>( request, delegate(HttpStatusCode statusCode, string statusDescription, object model) 
                        {
                            // call the provided handler and drop the dummy object
                            resultHandler( statusCode, statusDescription );
                        });
                }

                private void ExecuteAsync<TModel>( RestRequest request, RequestResult<TModel> resultHandler ) where TModel : new( )
                {
                    RestClient restClient = new RestClient( );
                    restClient.BaseUrl = BaseUrl;
                    restClient.CookieContainer = CookieContainer;

                    // don't wait longer than 15 seconds
                    request.Timeout = RequestTimeoutMS;

                    restClient.ExecuteAsync<TModel>( request, response => 
                        {
                            // exception or not, notify the caller of the desponse
                            Rock.Mobile.Threading.UIThreading.PerformOnUIThread( delegate 
                                { 
                                    resultHandler( response != null ? response.StatusCode : HttpStatusCode.RequestTimeout, 
                                                   response != null ? response.StatusDescription : "Client has no connection.", 
                                                   response != null ? response.Data : new TModel() );
                                });
                        });
                }

                private void SaveCookieToDevice( )
                {
                    string filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), COOKIE_FILENAME);

                    // open a stream
                    using (StreamWriter writer = new StreamWriter(filePath, false))
                    {
                        // store our cookies. We cannot serialize the container, so we retrieve and save just the 
                        // cookies we care about.
                        CookieCollection cookieCollection = CookieContainer.GetCookies( new Uri( BaseUrl ) );
                        writer.WriteLine( cookieCollection.Count.ToString() );
                        for( int i = 0; i < cookieCollection.Count; i++ )
                        {
                            string cookieStr = JsonConvert.SerializeObject( cookieCollection[i] );
                            writer.WriteLine( cookieStr );
                        }
                    }
                }

                private void LoadCookieFromDevice( )
                {
                    // at startup, this should be called to allow current objects to be restored.
                    string filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), COOKIE_FILENAME);

                    // if the file exists
                    if(System.IO.File.Exists(filePath) == true)
                    {
                        // read it
                        using (StreamReader reader = new StreamReader(filePath))
                        {
                            // load our cookies
                            int numCookies = int.Parse( reader.ReadLine() );
                            for( int i = 0; i < numCookies; i++ )
                            {
                                string cookieStr = reader.ReadLine();
                                Cookie cookie = JsonConvert.DeserializeObject<Cookie>( cookieStr ) as Cookie;
                                CookieContainer.Add( cookie );
                            }
                        }
                    }
                }

                public void SaveObjectsToDevice( )
                {
                    RockGeneralData.Instance.SaveToDevice( );
                    RockMobileUser.Instance.SaveToDevice( );
                    SaveCookieToDevice( );
                }

                public void LoadObjectsFromDevice( )
                {
                    RockGeneralData.Instance.LoadFromDevice( );
                    RockMobileUser.Instance.LoadFromDevice( );
                    LoadCookieFromDevice( );
                }

                public void SyncWithServer( RequestResult result )
                {
                    Console.WriteLine( "Sync with server" );

                    // this is a chance for anything unsaved to go ahead and save
                    RockMobileUser.Instance.SyncDirtyObjects( delegate(System.Net.HttpStatusCode statusCode, string statusDescription)
                        {
                            Console.WriteLine( "Sync with server complete with code {0}", statusCode );

                            // this is called back on the main thread, so from here we can execute more requests,
                            // or notify the caller.
                            result( statusCode, statusDescription );
                        });
                }
            }
        }
    }
}
