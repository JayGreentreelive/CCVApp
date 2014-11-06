using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using Rock.Mobile.Network;

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

                //Todo: This will actually exist in Rock.Client.Models
                public class LaunchData
                {
                    public LaunchData( )
                    {
                        GeneralDataVersion = 0;

                        News = new List<RockNews>( );
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
                }
                public LaunchData Data { get; set; }

                public RockLaunchData( )
                {
                    Data = new LaunchData( );
                }

                public void GetLaunchData( HttpRequest.RequestResult launchDataResult )
                {
                    Console.WriteLine( "Get LaunchData" );
                    RockApi.Instance.GetLaunchData(delegate(System.Net.HttpStatusCode statusCode, string statusDescription, LaunchData model)
                        {
                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                Data = model;
                            }

                            Console.WriteLine( "LaunchData Received With Status {0}", statusCode );

                            // notify the caller
                            if( launchDataResult != null )
                            {
                                launchDataResult( statusCode, statusDescription );
                            }
                        });
                }
            }
        }
    }
}
