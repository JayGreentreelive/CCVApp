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
                /// End point for posting a profile picture
                /// </summary>
                const string PutProfilePictureEndPoint = "FileUploader.ashx?isBinaryFile=true&fileTypeGuid=03BD8476-8A9F-4078-B628-5B538F967AFC&isTemporary=false";

                /// <summary>
                /// End point for retrieving prayer requests
                /// </summary>
                const string GetPrayerRequestsEndPoint = "api/prayerrequests/public";

                /// <summary>
                /// End point for grabbing the categories for prayer
                /// </summary>
                const string GetPrayerCategoriesEndPoint = "api/categories/getChildren/1";

                /// <summary>
                /// End point for getting news items to be displayed in the news section
                /// </summary>
                const string GetNewsEndPoint = "api/ContentChannelItems?$filter=ContentChannel/Guid eq guid'EAE51F3E-C27B-4E7C-B9A0-16EB68129637'&LoadAttributes=True";// and Status eq 1";

                /// <summary>
                /// End point for retrieving a Group Object
                /// </summary>
                //const string GetFamiliesEndPoint = "api/Groups/GetFamilies/{0}?$expand=GroupType,Campus,GroupLocations";
                const string GetFamiliesEndPoint = "api/Groups/GetFamilies/{0}?$expand=GroupType,Campus,Members/GroupRole,GroupLocations/Location,GroupLocations/GroupLocationTypeValue,GroupLocations/Location/LocationTypeValue";


                /// <summary>
                /// End point for retrieving all groups near a given address.
                /// </summary>
                const string GetGroupsByLocationEndPoint = "api/Groups/ByLocation/{0}/{1}/{2}/{3}/{4}/{5}";

                /// <summary>
                /// End point for updating a user's primary group home campus
                /// </summary>
                const string PutHomeCampusEndPoint = "api/Groups/";

                /// <summary>
                /// End point for updating or creating an address.
                /// </summary>
                                                                          //GroupId/GroupLocationTypeValueId/Street1/City/State/Zip/CountryCode
                const string PutAddressEndPoint = "api/Groups/SaveAddress/{0}/{1}/{2}/{3}/{4}/{5}/{6}";


                const string ResolveDefinedValueEndPoint = "api/DefinedValues?$filter=";
                const string ResolveDefinedValueSuffix = "Guid eq guid'{0}'";

                /// <summary>
                /// End point for updating a phone number
                /// </summary>
                const string PutPhoneNumberEndPoint = "api/PhoneNumbers/";

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
                /// End point for getting a list of campuses
                /// </summary>
                const string GetCampusesEndPoint = "api/Campuses/";

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
                    // update the profile by the personID
                    RestRequest request = GetRockRestRequest( Method.PUT );
                    request.AddBody( person );

                    Request.ExecuteAsync( BaseUrl + PutProfileEndPoint + person.Id, request, resultHandler);
                }

                public void UpdateHomeCampus( Rock.Client.Group primaryGroup, HttpRequest.RequestResult resultHandler )
                {
                    // To update their home campus, we'll actually update their primary groupID's campus, which will effectively update their campus,
                    // but also the campus for anyone else in that group. (Which is fine)
                    // update the profile by the personID
                    Rock.Client.Group updatedGroup = new Rock.Client.Group();
                    updatedGroup.Id = primaryGroup.Id;
                    updatedGroup.Guid = primaryGroup.Guid;
                    updatedGroup.IsSystem = primaryGroup.IsSystem;
                    updatedGroup.ParentGroupId = null;
                    updatedGroup.GroupTypeId = primaryGroup.GroupTypeId;
                    updatedGroup.CampusId = primaryGroup.CampusId;
                    updatedGroup.ScheduleId = null;
                    updatedGroup.Name = primaryGroup.Name;
                    updatedGroup.Description = primaryGroup.Description;
                    updatedGroup.IsSecurityRole = primaryGroup.IsSecurityRole;
                    updatedGroup.IsActive = primaryGroup.IsActive;
                    updatedGroup.Order = primaryGroup.Order;
                    updatedGroup.AllowGuests = primaryGroup.AllowGuests;
                    updatedGroup.GroupType = null;

                    RestRequest request = GetRockRestRequest( Method.PUT );
                    request.AddBody( updatedGroup );

                    Request.ExecuteAsync( BaseUrl + PutHomeCampusEndPoint + updatedGroup.Id, request, resultHandler);
                }

                public void UpdateAddress( Rock.Client.Group family, Rock.Client.GroupLocation address, HttpRequest.RequestResult resultHandler )
                {
                    RestRequest request = GetRockRestRequest( Method.PUT );

                    string requestUrl = string.Format( BaseUrl + PutAddressEndPoint, family.Id, 
                                                                                     address.GroupLocationTypeValueId, 
                                                                                     address.Location.Street1, 
                                                                                     address.Location.City, 
                                                                                     address.Location.State, 
                                                                                     address.Location.PostalCode, 
                                                                                     address.Location.Country );

                    Request.ExecuteAsync( requestUrl, request, resultHandler );
                }

                public void GetDefinedValues( List<System.Guid> definedValueGuidList, HttpRequest.RequestResult<List<Rock.Client.DefinedValue>> resultHandler )
                {
                    RestRequest request = GetRockRestRequest( Method.GET );

                    string requestUrl = BaseUrl + ResolveDefinedValueEndPoint;

                    // append the first guid to the request URL
                    requestUrl += string.Format( ResolveDefinedValueSuffix, definedValueGuidList[ 0 ] );

                    // are there more?
                    if ( definedValueGuidList.Count > 1 )
                    {
                        // go through the list, adding them in proper odata format.
                        for ( int i = 1; i < definedValueGuidList.Count; i++ )
                        {
                            requestUrl += " or ";
                            requestUrl += string.Format( ResolveDefinedValueSuffix, definedValueGuidList[ i ] );
                        }

                    }

                    Request.ExecuteAsync<List<Rock.Client.DefinedValue>>( requestUrl, request, resultHandler );
                }

                public void UpdatePhoneNumber( Rock.Client.PhoneNumber phoneNumber, bool isNew, HttpRequest.RequestResult resultHandler )
                {
                    RestRequest request = null;
                    string requestUrl = PutPhoneNumberEndPoint;
                    if ( isNew )
                    {
                        request = GetRockRestRequest( Method.POST );
                    }
                    else
                    {
                        // if we're updating an existing number, put the ID
                        request = GetRockRestRequest( Method.PUT );
                        requestUrl += phoneNumber.Id;
                    }
                    request.AddBody( phoneNumber );

                    // fire off the request
                    Request.ExecuteAsync( BaseUrl + requestUrl, request, resultHandler);
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

                public class ImageResponse
                {
                    public string Id { get; set; }
                    public string FileName { get; set; }
                }

                public void UpdateProfilePicture( MemoryStream image, HttpRequest.RequestResult<int> resultHandler )
                {
                    // send up the image for the user
                    RestRequest request = GetRockRestRequest( Method.POST );
                    request.AddFile( "file0", image.GetBuffer( ), "profilePic.jpg" );

                    string requestUrl = BaseUrl + PutProfilePictureEndPoint;

                    Request.ExecuteAsync( requestUrl, request, delegate(HttpStatusCode statusCode, string statusDescription, byte[] responseBytes )
                        {
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                // deserialize the raw response into our wrapper class
                                ImageResponse imageResponse = JsonConvert.DeserializeObject<ImageResponse>( System.Text.Encoding.ASCII.GetString( responseBytes ) );

                                // now call the final result
                                resultHandler( statusCode, statusDescription, int.Parse( imageResponse.Id ) );
                            }
                            else
                            {
                                resultHandler( statusCode, statusDescription, 0 );
                            }
                        } );
                }

                public void GetPrayerCategories( HttpRequest.RequestResult<List<Rock.Client.Category>> resultHandler )
                {
                    RestRequest request = GetRockRestRequest( Method.GET );
                    string requestUrl = BaseUrl + GetPrayerCategoriesEndPoint;

                    // get the resonse
                    Request.ExecuteAsync< List<Rock.Client.Category> >( requestUrl, request, resultHandler );
                }

                public void GetCampuses( HttpRequest.RequestResult< List<Rock.Client.Campus> > resultHandler )
                {
                    // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                    RestRequest request = GetRockRestRequest( Method.GET );
                    string requestUrl = BaseUrl + GetCampusesEndPoint;

                    // get the raw response
                    Request.ExecuteAsync< List<Rock.Client.Campus> >( requestUrl, request, resultHandler);
                }

                public void GetFamiliesOfPerson( int personId, HttpRequest.RequestResult< List<Rock.Client.Group> > resultHandler )
                {
                    // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                    RestRequest request = GetRockRestRequest( Method.GET );
                    string requestUrl = string.Format( BaseUrl + GetFamiliesEndPoint, personId.ToString( ) );

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

                public void GetNews( HttpRequest.RequestResult< List<Rock.Client.ContentChannelItem> > resultHandler )
                {
                    // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                    RestRequest request = GetRockRestRequest( Method.GET );
                    string requestUrl = BaseUrl + GetNewsEndPoint;

                    // get the raw response
                    Request.ExecuteAsync< List<Rock.Client.ContentChannelItem> >( requestUrl, request, resultHandler );
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
                    RockLaunchData.Instance.SaveToDevice( );
                    RockMobileUser.Instance.SaveToDevice( );
                    //SaveCookieToDevice( );
                }

                public void LoadObjectsFromDevice( )
                {
                    RockGeneralData.Instance.LoadFromDevice( );
                    RockLaunchData.Instance.LoadFromDevice( );
                    RockMobileUser.Instance.LoadFromDevice( );
                    //LoadCookieFromDevice( );
                }

                public void SyncWithServer( HttpRequest.RequestResult result )
                {
                    // this is a chance for anything saved but not uploaded to Rock to upload to Rock.
                    Console.WriteLine( "Sync with server" );

                    // USER PROFILE
                    RockMobileUser.Instance.SyncDirtyObjects( delegate(System.Net.HttpStatusCode statusCode, string statusDescription)
                        {
                            Console.WriteLine( "Sync with server complete with code {0}", statusCode );

                            // this is called back on the main thread, so from here we can execute more requests,
                            // or notify the caller.

                            // ADD MORE THINGS HERE

                            result( statusCode, statusDescription );
                        });
                }
            }
        }
    }
}
