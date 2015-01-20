using System;
using Rock.Client;
using Newtonsoft.Json;
using System.IO;
using Rock.Mobile.Network;
using CCVApp.Shared.Config;
using System.Collections.Generic;
using Facebook;
using RestSharp;
using Rock.Mobile.Util.Strings;

namespace CCVApp
{
    namespace Shared
    {
        namespace Network
        {
            /// <summary>
            /// Basically a wrapper for Rock.Models that make up the "user" of this mobile app.
            /// </summary>
            public sealed class RockMobileUser
            {
                /// <summary>
                /// Defines what ACCOUNT is used to login to Rock. They are always logged in via Rock,
                /// but the account used to log in could be a Rock Account or their Facebook Account
                /// </summary>
                public enum BoundAccountType
                {
                    Facebook,
                    Rock,
                    None
                }
                public BoundAccountType AccountType { get; set; }

                /// <summary>
                /// Instance for MobileUser. We only allow single logins, so force a static instance.
                /// </summary>
                private static RockMobileUser _Instance = new RockMobileUser();
                public static RockMobileUser Instance { get { return _Instance; } }

                const string MOBILEUSER_DATA_FILENAME = "mobileuser.dat";

                /// <summary>
                /// Account - The ID representing the user. If they logged in via a Rock Account, it's a Username. If a social service,
                /// it might be their social service account ID.
                /// </summary>
                public string UserID { get; set; }

                /// <summary>
                /// Account - Rock Password. Only valid if they are logged in with a Rock Account. If they logged in via a social service,
                /// this will be empty.
                /// </summary>
                public string RockPassword { get; set; }

                /// <summary>
                /// Account - Access Token for Social Service. If they're logged in via a Rock Account we don't need this or care about it.
                /// If they are logged in via a Social Service we will.
                /// </summary>
                public string AccessToken { get; set; }

                /// <summary>
                /// True when logged in.
                /// </summary>
                public bool LoggedIn { get; set; }

                /// <summary>
                /// Person object representing this user's core personal data.
                /// </summary>
                /// <value>The person.</value>
                public Person Person;

                /// <summary>
                /// GroupLocation representing the address of their primary residence
                /// </summary>
                public Rock.Client.GroupLocation Address;

                // make the address getters methods, not properties, so json doesn't try to serialize them.
                public string Street1( )
                {
                    return Address.Location.Street1;
                }

                public string Street2( )
                {
                    return Address.Location.Street2;
                }

                public string City( )
                {
                    return Address.Location.City;
                }

                public string State( )
                {
                    return Address.Location.State;
                }

                public string Zip( )
                {
                    return Address.Location.PostalCode.Substring( 0, Address.Location.PostalCode.IndexOf( '-' ) );
                }

                /// <summary>
                /// The URL of the last video streamed, used so we can know whether
                /// to resume it or not.
                /// </summary>
                /// <value>The last streaming video URL.</value>
                public string LastStreamingVideoUrl { get; set; }

                /// <summary>
                /// The left off position of the last streaming video, so we can
                /// resume if desired.
                /// </summary>
                /// <value>The last streaming video position.</value>
                public double LastStreamingVideoPos { get; set; }

                /// <summary>
                /// If true they have a profile image, so we should look for it in our defined spot.
                /// The way profile images work is, Rock will tell us they have one via a url.
                /// We'll request it and retrieve it, and then store it locally.
                /// </summary>
                /// <value><c>true</c> if this instance has profile image; otherwise, <c>false</c>.</value>
                public bool HasProfileImage { get; set; }

                /// <summary>
                /// A json version of the person at the last point it was sync'd with the server.
                /// This allows us to update Person and save it, and in the case of a server sync failing,
                /// know that we need to try again.
                /// </summary>
                /// <value>The person json.</value>
                public string LastSyncdPersonJson { get; set; }

                public string LastSyncdAddressJson { get; set; }

                private RockMobileUser( )
                {
                    Person = new Person();
                    Address = new GroupLocation();
                }

                public string PreferredName( )
                {
                    if( string.IsNullOrEmpty( Person.NickName ) == false )
                    {
                        return Person.NickName;
                    }
                    else
                    {
                        return Person.FirstName;
                    }
                }

                /// <summary>
                /// Returns the phone number matching phoneTypeId, or an empty one if no match is found.
                /// </summary>
                /// <returns>The phone number.</returns>
                /// <param name="phoneTypeId">Phone type identifier.</param>
                public Rock.Client.PhoneNumber TryGetPhoneNumber( int phoneTypeId )
                {
                    Rock.Client.PhoneNumber requestedNumber = new Rock.Client.PhoneNumber( );

                    // if the user has phone numbers
                    if ( Person.PhoneNumbers != null )
                    {
                        // get an enumerator
                        IEnumerator<Rock.Client.PhoneNumber> enumerator = Person.PhoneNumbers.GetEnumerator( );
                        enumerator.MoveNext( );

                        // search for the phone number type requested
                        while ( enumerator.Current != null )
                        {
                            Rock.Client.PhoneNumber phoneNumber = enumerator.Current as Rock.Client.PhoneNumber;

                            // is this the right type?
                            if ( phoneNumber.NumberTypeValueId == phoneTypeId )
                            {
                                requestedNumber = phoneNumber;
                                break;
                            }
                            enumerator.MoveNext( );
                        }
                    }

                    return requestedNumber;
                }

                public void UpdateOrAddPhoneNumber( string phoneNumberDigits, int phoneTypeId )
                {
                    // begin by assuming we will need to add a new number
                    bool addNewPhoneNumber = true;

                    // do they have numbers?
                    if ( Person.PhoneNumbers != null )
                    {
                        // search for a matching Id
                        IEnumerator<Rock.Client.PhoneNumber> enumerator = Person.PhoneNumbers.GetEnumerator( );
                        enumerator.MoveNext( );

                        while ( enumerator.Current != null )
                        {
                            Rock.Client.PhoneNumber phoneNumber = enumerator.Current as Rock.Client.PhoneNumber;

                            // is this the phone type?
                            if ( phoneNumber.NumberTypeValueId == phoneTypeId )
                            {
                                // then set it, and we won't need to create one.
                                addNewPhoneNumber = false;

                                phoneNumber.Number = phoneNumberDigits;
                                phoneNumber.NumberFormatted = phoneNumberDigits.AsPhoneNumber( );
                                break;
                            }
                            enumerator.MoveNext( );
                        }
                    }

                    // if this is true, we couldn't find a matching phone type,
                    // so we'll add it as a new one.
                    if ( addNewPhoneNumber == true )
                    {
                        Rock.Client.PhoneNumber phoneNumber = new Rock.Client.PhoneNumber();
                        phoneNumber.Number = phoneNumberDigits;
                        phoneNumber.NumberFormatted = phoneNumberDigits.AsPhoneNumber( );
                        phoneNumber.NumberTypeValueId = phoneTypeId;

                        // make sure they even HAVE a phone number set
                        if ( Person.PhoneNumbers == null )
                        {
                            Person.PhoneNumbers = new List<PhoneNumber>();
                        }
                        Person.PhoneNumbers.Add( phoneNumber );
                    }
                }

                /// <summary>
                /// Attempts to login with whatever account type is bound.
                /// </summary>
                public void Login( HttpRequest.RequestResult loginResult )
                {
                    switch ( AccountType )
                    {
                        case BoundAccountType.Rock:
                        {
                            //todo: we could put a rock validation end point here, but
                            // it's not necessary. Our cookie will eventually just expire
                            // and we'll relogin.
                            LoggedIn = true;

                            loginResult( System.Net.HttpStatusCode.NoContent, "" );
                            break;
                        }

                        case BoundAccountType.Facebook:
                        {
                            //todo: we could put a rock validation end point here, but
                            // it's not necessary. Our cookie will eventually just expire
                            // and we'll relogin.
                            LoggedIn = true;

                            loginResult( System.Net.HttpStatusCode.NoContent, "" );
                            break;
                        }

                        default:
                        {
                            throw new Exception( "No account type bound, so I don't know how to log you in to Rock. Call Bind*Account first." );
                        }
                    }
                }

                /// <summary>
                /// Called by us when done attempting to bind an account to Rock. For example,
                /// if a user wants to login via Facebook, we first have to get authorization FROM facebook,
                /// which means that could fail, and thus BindResult will return false.
                /// </summary>
                public delegate void BindResult( bool success );

                public void BindRockAccount( string username, string password, BindResult bindResult )
                {
                    RockApi.Instance.Login( username, password, delegate(System.Net.HttpStatusCode statusCode, string statusDescription) 
                        {
                            // if we received Ok (nocontent), we're logged in.
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                UserID = username;
                                RockPassword = password;

                                AccessToken = "";

                                AccountType = BoundAccountType.Rock;

                                // save!
                                SaveToDevice( );

                                bindResult( true);
                            }
                            else
                            {
                                bindResult( false );
                            }
                        });
                }

                public delegate void GetUserCredentials( string fromUri, FacebookClient session );
                public void BindFacebookAccount( GetUserCredentials getCredentials )
                {
                    Dictionary<string, object> loginRequest = FacebookManager.Instance.CreateLoginRequest( );

                    FacebookClient fbSession = new FacebookClient( );
                    string requestUri = fbSession.GetLoginUrl( loginRequest ).AbsoluteUri;

                    getCredentials( requestUri, fbSession );
                }

                public bool HasFacebookResponse( string response, FacebookClient session )
                {
                    // if true is returned, there IS a response, so the caller can call the below FacebookCredentialResult
                    FacebookOAuthResult oauthResult;
                    return session.TryParseOAuthCallbackUrl( new Uri( response ), out oauthResult );
                }

                public void FacebookCredentialResult( string response, FacebookClient session, BindResult result )
                {
                    // make sure we got a valid access token
                    FacebookOAuthResult oauthResult;
                    if( session.TryParseOAuthCallbackUrl (new Uri ( response ), out oauthResult) == true )
                    {
                        if ( oauthResult.IsSuccess )
                        {
                            // now attempt to get their basic info
                            FacebookClient fbSession = new FacebookClient( oauthResult.AccessToken );
                            string infoRequest = FacebookManager.Instance.CreateInfoRequest( );

                            fbSession.GetTaskAsync( infoRequest ).ContinueWith( t =>
                                {
                                    // if there was no problem, we are logged in and can send this up to Rock
                                    if ( t.IsFaulted == false || t.Exception == null )
                                    {
                                        // now login via rock with the facebook credentials to verify we're good
                                        RockApi.Instance.LoginFacebook( t.Result, delegate(System.Net.HttpStatusCode statusCode, string statusDescription) 
                                            {
                                                if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                                                {
                                                    UserID = "facebook_" + FacebookManager.Instance.GetUserID( t.Result );
                                                    RockPassword = "";

                                                    AccessToken = oauthResult.AccessToken;

                                                    AccountType = BoundAccountType.Facebook;

                                                    // save!
                                                    SaveToDevice( );

                                                    result( true );
                                                }
                                                else
                                                {
                                                    result( false );
                                                }
                                            });
                                    }
                                    else
                                    {
                                        // didn't work out.
                                        result( false );
                                    }
                                } );
                        }
                        else
                        {
                            result( false );
                        }
                    }
                    else
                    {
                        // didn't work out.
                        result( false );
                    }
                }

                public void LogoutAndUnbind( )
                {
                    // clear the person and take a blank copy
                    Person = new Person();
                    LastSyncdPersonJson = JsonConvert.SerializeObject( Person );

                    LoggedIn = false;
                    AccountType = BoundAccountType.None;

                    UserID = "";
                    RockPassword = "";
                    AccessToken = "";

                    //FacebookManager.Instance.Logout( );
                    RockApi.Instance.Logout( );

                    // save!
                    SaveToDevice( );
                }

                void SyncFacebookInfoToPerson( object infoObj )
                {
                    Person.FirstName = FacebookManager.Instance.GetFirstName( infoObj );
                    Person.NickName = FacebookManager.Instance.GetFirstName( infoObj );
                    Person.LastName = FacebookManager.Instance.GetLastName( infoObj );
                    Person.Email = FacebookManager.Instance.GetEmail( infoObj );
                }

                public void GetProfile( HttpRequest.RequestResult<Rock.Client.Person> profileResult )
                {
                    RockApi.Instance.GetProfile( UserID, delegate(System.Net.HttpStatusCode statusCode, string statusDescription, Rock.Client.Person model)
                        {
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                // on retrieval, convert this version for dirty compares later
                                Person = model;
                                LastSyncdPersonJson = JsonConvert.SerializeObject( Person );

                                // save!
                                SaveToDevice( );
                            }

                            // notify the caller
                            if( profileResult != null )
                            {
                                profileResult( statusCode, statusDescription, model );
                            }
                        });
                }

                public void UpdateProfile( HttpRequest.RequestResult profileResult )
                {
                    RockApi.Instance.UpdateProfile( Person, delegate(System.Net.HttpStatusCode statusCode, string statusDescription)
                        {
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                // if successful, update our json so we have a match and don't try to update again later.
                                LastSyncdPersonJson = JsonConvert.SerializeObject( Person );
                            }

                            // whether we succeeded in updating with the server or not, save to disk.
                            SaveToDevice( );

                            if( profileResult != null )
                            {
                                profileResult( statusCode, statusDescription );
                            }
                        });
                }

                public void GetAddress( HttpRequest.RequestResult< List<Rock.Client.Group> > addressResult )
                {
                    // for the address (which implicitly is their primary residence address), first get all group locations associated with them
                    RockApi.Instance.GetGroupLocations( Person.Id, delegate(System.Net.HttpStatusCode statusCode, string statusDescription, List<Rock.Client.Group> model)
                        {
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                // find what we'll consider their primary address
                                foreach( Rock.Client.Group personGroup in model )
                                {
                                    foreach( Rock.Client.GroupLocation groupLocation in personGroup.GroupLocations )
                                    {
                                        if( groupLocation.GroupLocationTypeValueId == CCVApp.Shared.Config.GeneralConfig.PrimaryResidenceLocationValueId )
                                        {
                                            Address = groupLocation;
                                            break;
                                        }
                                    }
                                }

                                // on retrieval, convert this version for dirty compares later
                                LastSyncdAddressJson = JsonConvert.SerializeObject( Address );

                                // save!
                                SaveToDevice( );
                            }

                            // notify the caller
                            if( addressResult != null )
                            {
                                addressResult( statusCode, statusDescription, model );
                            }
                        });
                }

                public void UpdateAddress( HttpRequest.RequestResult addressResult )
                {
                    /*RockApi.Instance.UpdateGroupLocation( Person.Id, Address, delegate(System.Net.HttpStatusCode statusCode, string statusDescription)
                        {
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                // if successful, update our json so we have a match and don't try to update again later.
                                LastSyncdAddressJson = JsonConvert.SerializeObject( Address );
                            }

                            // whether we succeeded in updating with the server or not, save to disk.
                            SaveToDevice( );

                            if( addressResult != null )
                            {
                                addressResult( statusCode, statusDescription );
                            }
                        });*/

                    if( addressResult != null )
                    {
                        addressResult( System.Net.HttpStatusCode.OK, "" );
                    }
                }

                public void SetProfilePicture( MemoryStream imageStream )
                {
                    // write the file out
                    using (FileStream writer = new FileStream(ProfilePicturePath, FileMode.Create))
                    {
                        imageStream.WriteTo( writer );
                    }

                    // now we have a picture!
                    HasProfileImage = true;

                    SaveToDevice( );

                    //todo: send it on up to Rock, too!
                }

                public string ProfilePicturePath
                {
                    get 
                    {
                        // get the path based on the platform
                        #if __IOS__
                        string jpgFilename = System.IO.Path.Combine ( Environment.GetFolderPath(Environment.SpecialFolder.Personal), SpringboardConfig.ProfilePic );
                        #else
                        string jpgFilename = Rock.Mobile.PlatformSpecific.Android.Core.Context.GetExternalFilesDir( null ).ToString( ) + SpringboardConfig.ProfilePic;
                        #endif

                        return jpgFilename;
                    }
                }

                public void TryDownloadProfilePicture( uint dimensionSize, HttpRequest.RequestResult profilePictureResult )
                {
                    // todo: Do we want to always get the profile pic for the bound account, or
                    // do we want to ask them, or do we want to pull down facebook's once and upload it to rock? Sigh,
                    // so many options
                    switch ( AccountType )
                    {
                        case BoundAccountType.Facebook:
                        {
                            // grab the actual image
                            string facebookID = UserID.Substring( UserID.IndexOf( "_" ) + 1 ); //chop off the "facebook_" prefix we add.
                            string profilePictureUrl = string.Format("https://graph.facebook.com/{0}/picture?type={1}&access_token={2}", facebookID, "large", AccessToken);
                            RestRequest request = new RestRequest( Method.GET );

                            // get the raw response
                            HttpRequest webRequest = new HttpRequest();
                            webRequest.ExecuteAsync( profilePictureUrl, request, delegate(System.Net.HttpStatusCode statusCode, string statusDescription, byte[] model )
                                {
                                    // it worked out ok!
                                    if ( Util.StatusInSuccessRange( statusCode ) == true )
                                    {
                                        MemoryStream imageStream = new MemoryStream( model );
                                        SetProfilePicture( imageStream );
                                        imageStream.Dispose( );
                                    }

                                    // notify the caller
                                    if ( profilePictureResult != null )
                                    {
                                        profilePictureResult( statusCode, statusDescription );
                                    }

                                } );
                            break;
                        }

                        case BoundAccountType.Rock:
                        {
                            if ( Person.PhotoId != null )
                            {
                                RockApi.Instance.GetProfilePicture( Person.PhotoId.ToString( ), dimensionSize, delegate(System.Net.HttpStatusCode statusCode, string statusDescription, MemoryStream imageStream )
                                    {
                                        if ( Util.StatusInSuccessRange( statusCode ) == true )
                                        {
                                            // if successful, update the file on disk.
                                            SetProfilePicture( imageStream );
                                        }

                                        // notify the caller
                                        if ( profilePictureResult != null )
                                        {
                                            profilePictureResult( statusCode, statusDescription );
                                        }
                                    } );
                            }
                            break;
                        }
                    }
                }

                public void SyncDirtyObjects( HttpRequest.RequestResult resultCallback )
                {
                    // check to see if our person object OR address object changed. If our original json
                    // created at a point when we know we were sync'd with the server
                    // no longer matches our object, we should update it.
                    string currPersonJson = JsonConvert.SerializeObject( Person );
                    string currAddressJson = JsonConvert.SerializeObject( Address );

                    if( string.Compare( LastSyncdPersonJson, currPersonJson ) != 0 || 
                        string.Compare( LastSyncdAddressJson, currAddressJson ) != 0 )
                    {
                        Console.WriteLine( "RockMobileUser: Syncing Profile" );
                        UpdateProfile( delegate(System.Net.HttpStatusCode statusCode, string statusDescription)
                            {
                                UpdateAddress( delegate(System.Net.HttpStatusCode code, string description)
                                    {
                                        // If needed, make other calls here, chained, and finally

                                        // return finished.
                                        resultCallback( statusCode, statusDescription );
                                    });
                            });
                    }
                    else
                    {
                        Console.WriteLine( "RockMobileUser: No sync needed." );

                        // nothing need be sync'd, call back with ok.
                        resultCallback( System.Net.HttpStatusCode.OK, "Success" );
                    }
                }

                public void SaveToDevice( )
                {
                    string filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), MOBILEUSER_DATA_FILENAME);

                    // open a stream
                    using (StreamWriter writer = new StreamWriter(filePath, false))
                    {
                        string json = JsonConvert.SerializeObject( this );
                        writer.WriteLine( json );
                    }
                }

                public void LoadFromDevice(  )
                {
                    // at startup, this should be called to allow current objects to be restored.
                    string filePath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal), MOBILEUSER_DATA_FILENAME);

                    // if the file exists
                    if(System.IO.File.Exists(filePath) == true)
                    {
                        // read it
                        using (StreamReader reader = new StreamReader(filePath))
                        {
                            // guard against corrupt data
                            string json = reader.ReadLine();
                            if ( json != null )
                            {
                                _Instance = JsonConvert.DeserializeObject<RockMobileUser>( json ) as RockMobileUser;
                            }
                        }
                    }
                }
            }
        }
    }
}
