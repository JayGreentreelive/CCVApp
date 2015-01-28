using System;
using System.Collections.Generic;
using Rock.Mobile.Util.Strings;
using CCVApp.Shared.Network;
using System.Collections;

namespace CCVApp.Shared
{
    public class GroupFinder
    {
        public GroupFinder( )
        {
        }

        public class GroupEntry
        {
            public string Title { get; set; }
            public string Address { get; set; }

            public string Distance { get; set; }

            public string NeighborhoodArea { get; set; }

            public string Latitude { get; set; }
            public string Longitude { get; set; }
        }

        public delegate void GetGroupsComplete( bool result, List<GroupEntry> groupEntry );
        public static void GetGroups( string address, GetGroupsComplete onCompletion )
        {
            List<GroupEntry> groupEntries = new List<GroupEntry>();

            // if this is true at the end of this function, we'll invoke onCompletion with a failed result.
            bool immediateError = true;

            // validate an address
            if ( string.IsNullOrEmpty( address ) == false )
            {
                // parse it
                string street = "";
                string city = "";
                string state = "";
                string zip = "";
                bool result = Parsers.ParseAddress( address, ref street, ref city, ref state, ref zip );
                if ( result == true )
                {
                    // we can now safely make the API call, which means THIS function can return without error.
                    immediateError = false;

                    // invoke the API, and when it calls our delegate, we can then invoke our original caller's onCompletion
                    RockApi.Instance.GetGroupsByLocation( CCVApp.Shared.Config.GeneralConfig.NeighborhoodGroupGeoFenceValueId, 
                        CCVApp.Shared.Config.GeneralConfig.NeighborhoodGroupValueId,
                        street, city, state, zip,
                        delegate(System.Net.HttpStatusCode statusCode, string statusDescription, List<Rock.Client.Group> model )
                        {
                            if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                // first thing we receive is the "area" group(s)
                                foreach ( Rock.Client.Group areaGroup in model )
                                {
                                    // in each area, there's an actual small group
                                    foreach ( Rock.Client.Group smallGroup in areaGroup.Groups )
                                    {
                                        // get the group location out of the small group enumerator
                                        IEnumerator enumerator = smallGroup.GroupLocations.GetEnumerator( );
                                        enumerator.MoveNext( );
                                        Rock.Client.Location location = ( (Rock.Client.GroupLocation)enumerator.Current ).Location;

                                        // and of course, each group has a location
                                        GroupEntry entry = new GroupEntry();
                                        entry.Title = smallGroup.Name;
                                        entry.Address = location.Street1 + "\n" + location.City + ", " + location.State + " " + location.PostalCode.Substring( 0, Math.Max( 0, location.PostalCode.IndexOf( '-' ) ) );
                                        entry.NeighborhoodArea = string.Format( "Part of the {0} Neighborhood", areaGroup.Name );
                                        entry.Distance = "1.5 miles away.";
                                        entry.Latitude = location.Latitude.ToString( );
                                        entry.Longitude = location.Longitude.ToString( );

                                        groupEntries.Add( entry );
                                    }
                                }
                            }

                            // our network delegate has been invoked and compelted, so now call whoever called us.
                            onCompletion( true, groupEntries );
                        } );
                }
            }

            if( immediateError == true )
            {
                onCompletion( false, groupEntries );
            }
        }
    }
}

