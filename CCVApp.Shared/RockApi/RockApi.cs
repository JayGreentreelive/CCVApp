using System;
using RestSharp;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using System.Collections.Generic;
using Rock.Mobile.Network;

namespace CCVApp
{
    namespace Shared
    {
        namespace Network
        {
            /// <summary>
            /// Rock API contains methods for making REST calls to Rock.
            /// This should only be used directly if an object doesn't exist
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

                const string COOKIE_FILENAME = "cookies.dat";
                const string BaseUrl = "http://rock.ccvonline.com/";

                /// <summary>
                /// End point for logging in
                /// </summary>
                const string AuthLoginEndPoint = "api/Auth/Login";

                /// <summary>
                /// End point for logging in
                /// </summary>
                const string AuthFacebookLoginEndPoint = "api/Auth/FacebookLogin";

                /// <summary>
                /// End point for retrieving a Person object
                /// </summary>
                const string GetProfileEndPoint = "api/People/GetByUserName/";

                /// <summary>
                /// End point for retrieving a profile picture with a specific SQUARE size.
                /// </summary>
                const string GetProfilePictureEndPoint = "GetImage.ashx?id={0}&width={1}&height={1}";

                /// <summary>
                /// End point for retrieving prayer requests
                /// </summary>
                /// 
                //const string GetPrayerRequestsEndPoint = "api/prayerrequests";
                //const string GetPrayerRequestsEndPoint = "api/prayerrequests?$filter=(IsApproved eq true) and (IsPublic eq true) and (IsActive eq true) and ( (ExpirationDate ge datetime'{0:yyyy-MM-dd}') or (ExpirationDate eq null) )&$expand=Category";
                const string GetPrayerRequestsEndPoint = "api/prayerrequests/public";

                /// <summary>
                /// End point for retrieving a Group Object
                /// </summary>
                const string GetFamiliesEndPoint = "api/Groups/GetFamilies/";
                const string GetGroupsByLocationEndPoint = "api/Groups/ByLocation/{0}/{1}/{2}/{3}/{4}/{5}";

                //const string PutGroupEndPoint = "api/Groups/GetFamilies/";

                /// <summary>
                /// End point for posting a prayer request
                /// </summary>
                const string PutPrayerRequestEndPoint = "api/prayerrequests";

                /// <summary>
                /// End point for updating a prayer request's prayed count.
                /// </summary>
                const string UpdatePrayerCountEndPoint = "api/prayerrequests/prayed/";

                /// <summary>
                /// End point for updating a Person object
                /// </summary>
                const string PutProfileEndPoint = "api/People/";

                /// <summary>
                /// The header key used for passing up the mobile app authorization token.
                /// </summary>
                const string AuthorizationTokenHeaderKey = "Authorization-Token";

                /// <summary>
                /// Stores the cookies received from Rock
                /// </summary>
                /// <value>The cookie container.</value>
                CookieContainer CookieContainer { get; set; }

                /// <summary>
                /// Our object for making REST calls.
                /// </summary>
                /// <value>The request.</value>
                HttpRequest Request { get; set; }

                RockApi( )
                {
                    //CookieContainer = new System.Net.CookieContainer();

                    Request = new HttpRequest();
                    //Request.CookieContainer = CookieContainer;
                }

                public void LoginFacebook( object facebookUser, HttpRequest.RequestResult resultHandler )
                {
                    RestRequest request = GetRockRestRequest( Method.POST );
                    //request.Resource = AuthLoginEndPoint;

                    request.AddBody( facebookUser );

                    Request.ExecuteAsync( BaseUrl + AuthFacebookLoginEndPoint, request, delegate(HttpStatusCode statusCode, string statusDescription, object model) 
                        {
                            // if login was a success, save our cookie
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                //SaveCookieToDevice();
                            }

                            // either way, notifiy the caller
                            if( resultHandler != null )
                            {
                                resultHandler( statusCode, statusDescription );
                            }
                        });
                }

                public void Login( string username, string password, HttpRequest.RequestResult resultHandler )
                {
                    RestRequest request = GetRockRestRequest( Method.POST );
                    //request.Resource = AuthLoginEndPoint;

                    request.AddParameter( "Username", username );
                    request.AddParameter( "Password", password );
                    request.AddParameter( "Persisted", true );

                    Request.ExecuteAsync( BaseUrl + AuthLoginEndPoint, request, delegate(HttpStatusCode statusCode, string statusDescription, object model) 
                        {
                            // if login was a success, save our cookie
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                //SaveCookieToDevice();
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

                public void GetPrayers( HttpRequest.RequestResult< List<Rock.Client.PrayerRequest> > resultHandler )
                {
                    // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                    RestRequest request = GetRockRestRequest( Method.GET );

                    // insert today's date as the expiration limit
                    string requestString = BaseUrl + string.Format( GetPrayerRequestsEndPoint, DateTime.Now );

                    Request.ExecuteAsync< List<Rock.Client.PrayerRequest> >( requestString, request, resultHandler);
                }

                public void PutPrayer( Rock.Client.PrayerRequest prayer, HttpRequest.RequestResult resultHandler )
                {
                    // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                    RestRequest request = GetRockRestRequest( Method.POST );
                    request.AddBody( prayer );

                    Request.ExecuteAsync( BaseUrl + PutPrayerRequestEndPoint, request, resultHandler);
                }

                public void IncrementPrayerCount( int prayerId, HttpRequest.RequestResult resultHandler )
                {
                    // build a URL that contains the ID for the prayer that is getting another prayer
                    RestRequest request = GetRockRestRequest( Method.PUT );

                    string requestUrl = BaseUrl + UpdatePrayerCountEndPoint;
                    requestUrl += prayerId.ToString( );

                    Request.ExecuteAsync( requestUrl, request, resultHandler);
                }

                public void GetProfile( string userName, HttpRequest.RequestResult<Rock.Client.Person> resultHandler )
                {
                    // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                    RestRequest request = GetRockRestRequest( Method.GET );

                    string requestUrl = BaseUrl + GetProfileEndPoint;
                    requestUrl += string.IsNullOrEmpty( userName ) == true ? RockMobileUser.Instance.UserID : userName;

                    Request.ExecuteAsync<Rock.Client.Person>( requestUrl, request, resultHandler);
                }

                public void UpdateProfile( Rock.Client.Person person, HttpRequest.RequestResult resultHandler )
                {
                    // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                    RestRequest request = GetRockRestRequest( Method.PUT );
                    request.AddBody( person );

                    Request.ExecuteAsync( BaseUrl + PutProfileEndPoint + person.Id, request, resultHandler);
                }

                public void GetProfilePicture( string photoId, uint dimensionSize, HttpRequest.RequestResult<MemoryStream> resultHandler )
                {
                    // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                    RestRequest request = GetRockRestRequest( Method.GET );
                    string requestUrl = BaseUrl + string.Format( GetProfilePictureEndPoint, photoId, dimensionSize );

                    // get the raw response
                    Request.ExecuteAsync( requestUrl, request, delegate(HttpStatusCode statusCode, string statusDescription, byte[] model) 
                        {
                            MemoryStream memoryStream = new MemoryStream( model );

                            resultHandler( statusCode, statusDescription, memoryStream );

                            memoryStream.Dispose( );
                        });
                }

                /*public void UpdateProfilePicture( string photoId, int dimensionSize, MemoryStream image, RequestResult resultHandler )
                {
                    // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                    RestRequest request = GetRockRestRequest( Method.PUT );
                    request.Resource = string.Format( GetProfilePictureEndPoint, photoId, dimensionSize );
                    request.AddBody( image );

                    ExecuteAsync( request, resultHandler);
                }*/

                public void GetFamiliesOfPerson( int personId, HttpRequest.RequestResult< List<Rock.Client.Group> > resultHandler )
                {
                    // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                    RestRequest request = GetRockRestRequest( Method.GET );
                    string requestUrl = BaseUrl + GetFamiliesEndPoint + personId.ToString( );

                    // get the raw response
                    Request.ExecuteAsync< List<Rock.Client.Group> >( requestUrl, request, resultHandler);
                }

                public void GetGroupsByLocation( int geoFenceGroupTypeId, int groupTypeId, string street, string city, string state, string zip, HttpRequest.RequestResult< List<Rock.Client.Group> > resultHandler )
                {
                    // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                    RestRequest request = GetRockRestRequest( Method.GET );
                    string requestUrl = BaseUrl + string.Format( GetGroupsByLocationEndPoint, geoFenceGroupTypeId, groupTypeId, street, city, state, zip );

                    // get the raw response
                    Request.ExecuteAsync< List<Rock.Client.Group> >( requestUrl, request, resultHandler);
                }

                /// <summary>
                /// Simple wrapper function to make sure all required headers get placed in
                /// any REST request made to Rock.
                /// </summary>
                /// <returns>The rock rest request.</returns>
                RestRequest GetRockRestRequest( Method method )
                {
                    RestRequest request = new RestRequest( method );
                    request.RequestFormat = DataFormat.Json;
                    request.AddHeader( AuthorizationTokenHeaderKey, CCVApp.Shared.Config.GeneralConfig.RockMobileAppAuthorizationKey );
                 
                    return request;
                }

                public void GetLaunchData( HttpRequest.RequestResult<RockLaunchData.LaunchData> resultHandler )
                {
                    // todo: add a "get LaunchData" end point.
                    resultHandler( HttpStatusCode.OK, "Success", RockLaunchData.Instance.Data );
                }

                public void GetGeneralData( HttpRequest.RequestResult<RockGeneralData.GeneralData> resultHandler )
                {
                    // todo: add a "get GeneralData" end point.
                    resultHandler( HttpStatusCode.OK, "Success", RockGeneralData.Instance.Data );
                }

                /*private void SaveCookieToDevice( )
                {
                    string filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), COOKIE_FILENAME);

                    // open a stream
                    using (StreamWriter writer = new StreamWriter(filePath, false))
                    {
                        // store our cookies. We cannot serialize the container, so we retrieve and save just the 
                        // cookies we care about.
                        CookieCollection cookieCollection = CookieContainer.GetCookies( new Uri( BaseUrl ) );
                        writer.WriteLine( cookieCollection.Count.ToString( ) );
                        for ( int i = 0; i < cookieCollection.Count; i++ )
                        {
                            string cookieStr = JsonConvert.SerializeObject( cookieCollection[ i ] );
                            writer.WriteLine( cookieStr );
                        }
                    }
                }*/

                /*private void LoadCookieFromDevice( )
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
                }*/

                public void SaveObjectsToDevice( )
                {
                    RockGeneralData.Instance.SaveToDevice( );
                    RockMobileUser.Instance.SaveToDevice( );
                    //SaveCookieToDevice( );
                }

                public void LoadObjectsFromDevice( )
                {
                    RockGeneralData.Instance.LoadFromDevice( );
                    RockMobileUser.Instance.LoadFromDevice( );
                    //LoadCookieFromDevice( );
                }

                public void SyncWithServer( HttpRequest.RequestResult result )
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
