using System;
using Rock.Client;
using Newtonsoft.Json;
using System.IO;
using Rock.Mobile.Network;
using CCVApp.Shared.Config;
using System.Collections.Generic;
using Facebook;
using RestSharp;

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

                private RockMobileUser( )
                {
                    Person = new Person();
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
                /// Attempts to login with whatever account type is bound.
                /// </summary>
                public void Login( HttpRequest.RequestResult loginResult )
                {
                    switch ( AccountType )
                    {
                        case BoundAccountType.Rock:
                        {
                            // todo: call a rock endpoint validating that we're still good. For now, we are.
                            LoggedIn = true;

                            loginResult( System.Net.HttpStatusCode.NoContent, "" );
                            break;
                        }

                        case BoundAccountType.Facebook:
                        {
                            //todo: We dont' need to do this anymore! We know they logged in once, so
                            // that's all we have to care about

                            // first verify that we're still good with Facebook (the user didn't revoke our permissions)
                            FacebookClient fbSession = new FacebookClient( AccessToken );
                            string infoRequest = FacebookManager.Instance.CreateInfoRequest( );

                            fbSession.GetTaskAsync( infoRequest ).ContinueWith( t =>
                                {
                                    if( t.IsFaulted == false || t.Exception == null )
                                    {
                                        // Since they logged in with Facebook, let's use that as their
                                        // latest info. Therefore, update it now.
                                        SyncFacebookInfoToPerson( t.Result );


                                        // move on to validate their ID with rock. For now, we are validated.
                                        LoggedIn = true;


                                        loginResult( System.Net.HttpStatusCode.NoContent, "" );
                                    }
                                    else
                                    {
                                        // error
                                        LogoutAndUnbind( );
                                        loginResult( System.Net.HttpStatusCode.BadRequest, "" );
                                    }
                                } );
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
                                        // get the user ID
                                        UserID = /*"facebook_" +  */FacebookManager.Instance.GetUserID( t.Result );

                                        // copy over all the facebook info we can into the Person object
                                        SyncFacebookInfoToPerson( t.Result );

                                        //TODO: Send this up to Rock. We can't since it doesn't accept UserIDs yet, so consider us logged in.
                                        //also, don't worry about revalidating Facebook, we've logged in ONCE that's all we care about till they logout.

                                        RockPassword = "";
                                        AccessToken = oauthResult.AccessToken;

                                        AccountType = BoundAccountType.Facebook;

                                        SaveToDevice( );

                                        result( true );
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
                            string profilePictureUrl = string.Format("https://graph.facebook.com/{0}/picture?type={1}&access_token={2}", UserID, "large", AccessToken);
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
                    // check to see if our person object changed. If our original json
                    // created at a point when we know we were sync'd with the server
                    // no longer matches our object, we should update it.
                    string currPersonJson = JsonConvert.SerializeObject( Person );
                    if( string.Compare( LastSyncdPersonJson, currPersonJson ) != 0 )
                    {
                        Console.WriteLine( "RockMobileUser: Syncing Profile" );
                        UpdateProfile( delegate(System.Net.HttpStatusCode statusCode, string statusDescription)
                            {
                                // If needed, make other calls here, chained, and finally


                                // return finished.
                                resultCallback( statusCode, statusDescription );
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
                            string json = reader.ReadLine();
                            _Instance = JsonConvert.DeserializeObject<RockMobileUser>( json ) as RockMobileUser;
                        }
                    }
                }
            }
        }
    }
}
