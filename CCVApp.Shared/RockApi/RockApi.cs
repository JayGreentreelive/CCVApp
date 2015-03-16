using System;
using RestSharp;
using System.IO;
using Newtonsoft.Json;
using System.Net;
using System.Collections.Generic;
using Rock.Mobile.Network;
using CCVApp.Shared.Config;

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
                const string GetProfileByGuidEndPoint = "api/People?$filter=Guid eq guid'{0}'";

                /// <summary>
                /// End point for posting a new login that will be associated with a user
                /// </summary>
                const string PutLoginInfoEndPoint = "api/UserLogins";

                /// <summary>
                /// End point for retrieving a profile picture with a specific SQUARE size.
                /// </summary>
                const string GetProfilePictureEndPoint = "GetImage.ashx?id={0}&width={1}&height={1}";

                /// <summary>
                /// End point for posting a profile picture
                /// </summary>
                const string PutProfilePictureEndPoint = "FileUploader.ashx?isBinaryFile=true&fileTypeGuid=03BD8476-8A9F-4078-B628-5B538F967AFC&isTemporary=false";

                /// <summary>
                /// End point for placing the user in a group of "people who's profile picture needs evaluation"
                /// Required after they update their profile picture
                /// </summary>
                const string GetProfileImageMemberEndPoint = "api/groupmembers/";

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
                //const string GetNewsEndPoint = "/api/ContentChannelItems?$filter=ContentChannel/Guid eq guid'EAE51F3E-C27B-4E7C-B9A0-16EB68129637' and Status eq '2' and (StartDateTime ge DateTime'2000-02-03' or StartDateTime eq null) and (ExpireDateTime le DateTime'2020-03-30' or ExpireDateTime eq null)&LoadAttributes=True";
                const string GetNewsEndPoint = "/api/ContentChannelItems?$filter=ContentChannel/Guid eq guid'EAE51F3E-C27B-4E7C-B9A0-16EB68129637' and Status eq '2' and (StartDateTime le DateTime'{0}' or StartDateTime eq null) and (ExpireDateTime ge DateTime'{1}' or ExpireDateTime eq null)&LoadAttributes=True";

                /// <summary>
                /// End point for getting the last time General Data was updated. If newer,
                /// we'll know we need to update GeneralData.
                /// </summary>
                const string GetGeneralDataTimeEndPoint = "/api/AttributeValues?$filter=AttributeId eq {0}";

                /// <summary>
                /// End point for retrieving a Group Object
                /// </summary>
                //const string GetFamiliesEndPoint = "api/Groups/GetFamilies/{0}?$expand=GroupType,Campus,GroupLocations";
                const string GetFamiliesEndPoint = "api/Groups/GetFamilies/{0}?$expand=GroupType,Campus,Members/GroupRole,GroupLocations/Location,GroupLocations/GroupLocationTypeValue,GroupLocations/Location/LocationTypeValue";
                //

                /// <summary>
                /// End point for retrieving all groups near a given address.
                /// </summary>
                //const string GetGroupsByLocationEndPoint = "api/Groups/ByLocation/{0}/{1}/{2}/{3}/{4}/{5}";
                const string GetLocationFromAddressEndPoint = "api/locations/{0}/{1}/{2}/{3}";
                const string GetGroupsByLocationEndPoint = "api/Groups/ByLocation/{0}/{1}/{2}";


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
                const string PutPostProfileEndPoint = "api/People/";

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

                    Request.ExecuteAsync( BaseUrl + PutPostProfileEndPoint + person.Id, request, resultHandler);
                }

                public void RegisterNewUser( Rock.Client.Person person, Rock.Client.PhoneNumber phoneNumber, string username, string password, HttpRequest.RequestResult resultHandler )
                {
                    //JHM 3-11-15 ALMOST FRIDAY THE 13th SIX YEARS LATER YAAAHH!!!!!
                    //Until we release, or at least are nearly done testing, do not allow actual user registrations.
                    resultHandler( HttpStatusCode.OK, "" );
                    return;
                    
                    // this is a complex end point. To register a user is a multiple step process.
                    //1. Create a new Person on Rock
                    //2. Request that Person back
                    //3. Create a new Login for that person
                    //4. Post the location, phone number and home campus
                    person.Guid = Guid.NewGuid( );

                    CreateProfile( person, 
                        delegate(System.Net.HttpStatusCode statusCode, string statusDescription )
                        {
                            if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) )
                            {
                                GetProfileByGuid( person.Guid,
                                    delegate(System.Net.HttpStatusCode personStatusCode, string personStatusDescription, Rock.Client.Person model) 
                                    {
                                        if ( Rock.Mobile.Network.Util.StatusInSuccessRange( personStatusCode ) )
                                        {
                                            // it worked. now put their login info. Waiting for mike to give me a handy model. yay.
                                            CreateLogin( model.Id, username, password, 
                                                delegate(System.Net.HttpStatusCode loginStatusCode, string loginStatusDescription) 
                                                {
                                                    // if this worked, we are home free
                                                    if ( Rock.Mobile.Network.Util.StatusInSuccessRange( loginStatusCode ) )
                                                    {
                                                        // now update their phone number, if valid
                                                        if( phoneNumber != null )
                                                        {
                                                            phoneNumber.PersonId = model.Id;
                                                            phoneNumber.Guid = Guid.NewGuid( );

                                                            UpdatePhoneNumber( phoneNumber, true, 
                                                                delegate(HttpStatusCode phoneStatusCode, string phoneStatusDescription) 
                                                                {
                                                                    if( resultHandler != null )
                                                                    {
                                                                        resultHandler( phoneStatusCode, phoneStatusDescription );
                                                                    }
                                                                });
                                                        }
                                                        else
                                                        {
                                                            // phone number not provided, go with the result of the login creation
                                                            if( resultHandler != null )
                                                            {
                                                                resultHandler( loginStatusCode, loginStatusDescription );
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // Failed
                                                        if( resultHandler != null )
                                                        {
                                                            resultHandler( loginStatusCode, loginStatusDescription );
                                                        }
                                                    }
                                                });
                                        }
                                        else
                                        {
                                            // Failed
                                            if( resultHandler != null )
                                            {
                                                resultHandler( personStatusCode, personStatusDescription );
                                            }
                                        }
                                    });
                            }
                            else
                            {
                                // Failed
                                if( resultHandler != null )
                                {
                                    resultHandler( statusCode, statusDescription );
                                }
                            }
                        } );
                }

                void CreateProfile( Rock.Client.Person person, HttpRequest.RequestResult resultHandler )
                {
                    // create a person object that can go up to rock, and copy the relavant data from the passed in arg
                    Rock.Client.Person newPerson = new Rock.Client.Person( );
                    newPerson.Guid = person.Guid;
                    newPerson.BirthDate = person.BirthDate;
                    newPerson.BirthDay = person.BirthDay;
                    newPerson.BirthMonth = person.BirthMonth;
                    newPerson.BirthYear = person.BirthYear;
                    newPerson.Email = person.Email;
                    newPerson.FirstName = person.FirstName;
                    newPerson.LastName = person.LastName;
                    newPerson.Gender = person.Gender;

                    RestRequest request = GetRockRestRequest( Method.POST );
                    request.AddBody( person );

                    Request.ExecuteAsync( BaseUrl + PutPostProfileEndPoint, request, resultHandler);
                }

                void CreateLogin( int personId, string username, string password, HttpRequest.RequestResult resultHandler )
                {
                    Rock.Client.UserLoginWithPlainTextPassword newLogin = new Rock.Client.UserLoginWithPlainTextPassword();
                    newLogin.UserName = username;
                    newLogin.PlainTextPassword = password;
                    newLogin.PersonId = personId;
                    newLogin.Guid = Guid.NewGuid( );
                    newLogin.EntityTypeId = GeneralConfig.UserLoginEntityTypeId;

                    RestRequest request = GetRockRestRequest( Method.POST );
                    request.AddBody( newLogin );

                    Request.ExecuteAsync( BaseUrl + PutLoginInfoEndPoint, request, resultHandler);
                }

                void GetProfileByGuid( Guid guid, HttpRequest.RequestResult<Rock.Client.Person> resultHandler )
                {
                    // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                    RestRequest request = GetRockRestRequest( Method.GET );

                    string requestUrl = string.Format( BaseUrl + GetProfileByGuidEndPoint, guid.ToString( ) );
                    Request.ExecuteAsync< List<Rock.Client.Person> >( requestUrl, request, 
                        delegate(HttpStatusCode statusCode, string statusDescription, List<Rock.Client.Person> modelList) 
                        {
                            Rock.Client.Person returnPerson = null;

                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                returnPerson = modelList[ 0 ];
                            }

                            resultHandler( statusCode, statusDescription, returnPerson );
                        });
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

                public void UpdateProfileImageGroup( Rock.Client.Person person, HttpRequest.RequestResult resultHandler )
                {
                    // first see if the user is already a member of this group (which is true if they've EVER attempted to update their profile picture.
                    RestRequest request = GetRockRestRequest( Method.GET );
                    string requestUrl = BaseUrl + GetProfileImageMemberEndPoint + string.Format( "?$filter=PersonId eq {0} and GroupId eq {1}", person.Id, GeneralConfig.ApplicationGroup_PhotoRequest_ValueId );
                    Request.ExecuteAsync< List<Rock.Client.GroupMember> >( requestUrl, request, 
                        delegate(HttpStatusCode statusCode, string statusDescription, List<Rock.Client.GroupMember> model )
                        {
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                // if it's null, they are NOT in the group and we should POST. if it's valid,
                                // we can simply update the existing.
                                if( model.Count == 0 )
                                {
                                    request = GetRockRestRequest( Method.POST );

                                    Rock.Client.GroupMember groupMember = new Rock.Client.GroupMember();
                                    groupMember.Guid = Guid.NewGuid( );
                                    groupMember.PersonId = person.Id;
                                    groupMember.GroupMemberStatus = GeneralConfig.GroupMemberStatus_Pending_ValueId;
                                    groupMember.GroupId = GeneralConfig.ApplicationGroup_PhotoRequest_ValueId;
                                    groupMember.GroupRoleId = GeneralConfig.GroupMemberRole_Member_ValueId;
                                    request.AddBody( groupMember );

                                    requestUrl = BaseUrl + GetProfileImageMemberEndPoint;

                                    // send off the request and let the original caller have the result
                                    Request.ExecuteAsync( requestUrl, request, resultHandler );
                                }
                                else
                                {
                                    // otherwise, we'll do a PUT
                                    request = GetRockRestRequest( Method.PUT );

                                    Rock.Client.GroupMember groupMember = new Rock.Client.GroupMember();

                                    // set the status to pending
                                    groupMember.GroupMemberStatus = GeneralConfig.GroupMemberStatus_Pending_ValueId;

                                    // and copy over all the other data
                                    groupMember.PersonId = model[ 0 ].PersonId;
                                    groupMember.Guid = model[ 0 ].Guid;
                                    groupMember.GroupId = model[ 0 ].GroupId;
                                    groupMember.GroupRoleId = model[ 0 ].GroupRoleId;
                                    groupMember.Id = model[ 0 ].Id;
                                    groupMember.IsSystem = model[ 0 ].IsSystem;

                                    request.AddBody( groupMember );


                                    requestUrl = BaseUrl + GetProfileImageMemberEndPoint + string.Format( "{0}", groupMember.Id );

                                    // send off the request and let the original caller have the result
                                    Request.ExecuteAsync( requestUrl, request, resultHandler );
                                }
                            }
                            else
                            {
                                // fail...
                                resultHandler( statusCode, statusDescription );
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

                /*public void GetGroupsByLocation( int geoFenceGroupTypeId, int groupTypeId, string street, string city, string state, string zip, HttpRequest.RequestResult< List<Rock.Client.Group> > resultHandler )
                {
                    // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                    RestRequest request = GetRockRestRequest( Method.GET );
                    string requestUrl = BaseUrl + string.Format( GetGroupsByLocationEndPoint, geoFenceGroupTypeId, groupTypeId, street, city, state, zip );

                    // get the raw response
                    Request.ExecuteAsync< List<Rock.Client.Group> >( requestUrl, request, resultHandler);
                }*/

                public void GetLocationFromAddress( string street, string city, string state, string zip, HttpRequest.RequestResult<Rock.Client.Location> resultHandler )
                {
                    // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                    RestRequest request = GetRockRestRequest( Method.GET );
                    string requestUrl = BaseUrl + string.Format( GetLocationFromAddressEndPoint, street, city, state, zip );

                    // first get the location based on the info passed up
                    Request.ExecuteAsync<Rock.Client.Location>( requestUrl, request, resultHandler );
                }

                public void GetGroupsByLocation( int geoFenceGroupTypeId, int groupTypeId, int locationId, HttpRequest.RequestResult< List<Rock.Client.Group> > resultHandler )
                {
                    // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                    RestRequest request = GetRockRestRequest( Method.GET );
                    string requestUrl = BaseUrl + string.Format( GetGroupsByLocationEndPoint, geoFenceGroupTypeId, groupTypeId, locationId );

                    Request.ExecuteAsync< List<Rock.Client.Group> >( requestUrl, request, resultHandler );
                }

                public void GetNews( HttpRequest.RequestResult< List<Rock.Client.ContentChannelItem> > resultHandler )
                {
                    // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                    RestRequest request = GetRockRestRequest( Method.GET );
                    string requestUrl = string.Format( BaseUrl + GetNewsEndPoint, DateTime.Now.ToString( "s" ), DateTime.Now.ToString( "s" ) );


                    // get the raw response
                    Request.ExecuteAsync< List<Rock.Client.ContentChannelItem> >( requestUrl, request, resultHandler );
                }

                public class DateTimeModel
                {
                    public string ValueAsDateTime { get; set; }
                }
                public void GetGeneralDataTime( HttpRequest.RequestResult<DateTime> resultHandler )
                {
                    // request a profile by the username. If no username is specified, we'll use the logged in user's name.
                    RestRequest request = GetRockRestRequest( Method.GET );
                    string requestUrl = string.Format( BaseUrl + GetGeneralDataTimeEndPoint, GeneralConfig.GeneralDataTimeValueId );


                    // get the raw response
                    Request.ExecuteAsync< List<DateTimeModel> >( requestUrl, request, delegate(HttpStatusCode statusCode, string statusDescription, List<DateTimeModel> model) 
                        {
                            DateTime dateTime = DateTime.MinValue;
                            if( model != null )
                            {
                                dateTime = DateTime.Parse( model[ 0 ].ValueAsDateTime );
                            }

                            resultHandler( statusCode, statusDescription, dateTime );
                        } );
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
