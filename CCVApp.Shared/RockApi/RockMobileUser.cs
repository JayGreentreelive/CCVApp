using System;
using Rock.Client;
using Newtonsoft.Json;
using System.IO;
using Rock.Mobile.Network;
using CCVApp.Shared.Config;

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
                /// Instance for MobileUser. We only allow single logins, so force a static instance.
                /// </summary>
                private static RockMobileUser _Instance = new RockMobileUser();
                public static RockMobileUser Instance { get { return _Instance; } }

                const string MOBILEUSER_DATA_FILENAME = "mobileuser.dat";

                /// <summary>
                /// Account - Username
                /// </summary>
                /// <value>The username.</value>
                public string Username { get; set; }

                /// <summary>
                /// Account - Password
                /// </summary>
                /// <value>The password.</value>
                public string Password { get; set; }

                /// <summary>
                /// True when logged in
                /// </summary>
                /// <value><c>true</c> if logged in; otherwise, <c>false</c>.</value>
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

                public void Login( string username, string password, HttpRequest.RequestResult loginResult )
                {
                    RockApi.Instance.Login( username, password, delegate(System.Net.HttpStatusCode statusCode, string statusDescription) 
                        {
                            // if we received Ok (nocontent), we're logged in.
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                Username = username;
                                Password = password;

                                LoggedIn = true;

                                // save!
                                SaveToDevice( );
                            }

                            // notify the caller
                            if( loginResult != null )
                            { 
                                loginResult( statusCode, statusDescription );
                            }
                        });
                }

                public void Logout( )
                {
                    // clear the person and take a blank copy
                    Person = new Person();
                    LastSyncdPersonJson = JsonConvert.SerializeObject( Person );

                    LoggedIn = false;

                    Username = "";
                    Password = "";

                    RockApi.Instance.Logout( );

                    // save!
                    SaveToDevice( );
                }

                public void GetProfile( HttpRequest.RequestResult<Rock.Client.Person> profileResult )
                {
                    RockApi.Instance.GetProfile( Username, delegate(System.Net.HttpStatusCode statusCode, string statusDescription, Rock.Client.Person model)
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
                        string jpgFilename = Rock.Mobile.PlatformCommon.Droid.Context.GetExternalFilesDir( null ).ToString( ) + SpringboardConfig.ProfilePic;
                        #endif

                        return jpgFilename;
                    }
                }

                public void DownloadProfilePicture( int dimensionSize, HttpRequest.RequestResult profilePictureResult )
                {
                    RockApi.Instance.GetProfilePicture( Person.PhotoId.ToString(), dimensionSize, delegate(System.Net.HttpStatusCode statusCode, string statusDescription, MemoryStream imageStream)
                        {
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                // if successful, update the file on disk.
                                SetProfilePicture( imageStream );
                            }

                            // notify the caller
                            if( profilePictureResult != null )
                            {
                                profilePictureResult( statusCode, statusDescription );
                            }
                        });
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
