using System;
using CCVApp.Shared.Network;
using Rock.Mobile.Network;

namespace CCVApp
{
    namespace Shared
    {
        namespace Network
        {
            public sealed class RockNetworkManager
            {
                private static RockNetworkManager _Instance = new RockNetworkManager( );
                public static RockNetworkManager Instance { get { return _Instance; } }

                HttpRequest.RequestResult ResultCallback;

                public RockNetworkManager( )
                {
                }

                public void SyncRockData( HttpRequest.RequestResult resultCallback )
                {
                    ResultCallback = resultCallback;

                    // if we're logged in, sync any changes we've made with the server.
                    if( RockMobileUser.Instance.LoggedIn == true )
                    {
                        Console.WriteLine( "Logged in. Syncing out-of-sync data." );

                        //( this includes notes, profile changes, etc.)
                        RockApi.Instance.SyncWithServer( delegate 
                            {
                                // failure or not, server syncing is finished, so let's go ahead and 
                                // get launch data.
                                RockLaunchData.Instance.GetLaunchData( LaunchDataReceived );
                            });
                    }
                    else
                    {
                        Console.WriteLine( "Not Logged In. Skipping sync." );
                        RockLaunchData.Instance.GetLaunchData( LaunchDataReceived );
                    }
                }

                void LaunchDataReceived(System.Net.HttpStatusCode statusCode, string statusDescription)
                {
                    // At this point we're finished. Whether we succeeded or failed, we should now validate
                    // the version number. If getting launch data failed, that's ok, that will guarantee that
                    // the GeneralData version number isn't LESS than what we stored in LaunchData.

                    // if there's a newer General Data, grab it.
                    if( RockGeneralData.Instance.Data.Version < RockLaunchData.Instance.Data.GeneralDataVersion )
                    {
                        RockGeneralData.Instance.GetGeneralData( GeneralDataReceived );
                    }
                    else
                    {
                        // May not have anything else to do here
                        ResultCallback( statusCode, statusDescription );
                    }
                }

                void GeneralDataReceived(System.Net.HttpStatusCode statusCode, string statusDescription)
                {
                    // New general data received. Save it, update fields, etc.
                    ResultCallback( statusCode, statusDescription );
                }
            }
        }
    }
}

