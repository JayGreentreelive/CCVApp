
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Android.Webkit;

using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;
using Android.Graphics;
using Android.GoogleMaps;
using Android.Gms.Maps;
using CCVApp.Shared;
using CCVApp.Shared.Network;
using CCVApp.Shared.Analytics;

namespace Droid
{
    namespace Tasks
    {
        namespace Connect
        {
            public class GroupArrayAdapter : BaseAdapter
            {
                GroupFinderFragment ParentFragment { get; set; }

                public GroupArrayAdapter( GroupFinderFragment parentFragment )
                {
                    ParentFragment = parentFragment;
                }

                public override int Count 
                {
                    get { return ParentFragment.GroupEntries.Count; }
                }

                public override Java.Lang.Object GetItem (int position) 
                {
                    // could wrap a Contact in a Java.Lang.Object
                    // to return it here if needed
                    return null;
                }

                public override long GetItemId (int position) 
                {
                    return 0;
                }

                public override View GetView(int position, View convertView, ViewGroup parent)
                {
                    GroupListItem messageItem = convertView as GroupListItem;
                    if ( messageItem == null )
                    {
                        messageItem = new GroupListItem( ParentFragment.Activity.BaseContext );
                    }

                    messageItem.ParentAdapter = this;
                    messageItem.Position = position;

                    messageItem.Title.Text = ParentFragment.GroupEntries[ position ].Title;
                    messageItem.Address.Text = ParentFragment.GroupEntries[ position ].Address;
                    messageItem.Neighborhood.Text = ParentFragment.GroupEntries[ position ].NeighborhoodArea;
                    messageItem.Distance.Text = ParentFragment.GroupEntries[ position ].Distance;

                    return messageItem;
                }

                public void OnClick( int position, int buttonIndex )
                {
                    ParentFragment.OnClick( position, buttonIndex );
                }
            }

            public class GroupListItem : LinearLayout
            {
                public LinearLayout TitleLayout { get; set; }
                public TextView Title { get; set; }
                public TextView Address { get; set; }
                public TextView Neighborhood { get; set; }
                public TextView Distance { get; set; }

                public GroupArrayAdapter ParentAdapter { get; set; }
                public int Position { get; set; }

                public GroupListItem( Context context ) : base( context )
                {
                    SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );
                    LayoutParameters = new AbsListView.LayoutParams( LayoutParams.MatchParent, LayoutParams.MatchParent );

                    Orientation = Orientation.Vertical;

                    LinearLayout contentLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    contentLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.MatchParent, LayoutParams.MatchParent );
                    contentLayout.Orientation = Orientation.Horizontal;
                    AddView( contentLayout );

                    TitleLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    TitleLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.WrapContent, LayoutParams.WrapContent );
                    TitleLayout.Orientation = Orientation.Vertical;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).Weight = 1;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).LeftMargin = 25;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).TopMargin = 10;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).BottomMargin = 10;
                    contentLayout.AddView( TitleLayout );

                    Title = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Title.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Title.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Medium_Font_Regular ), TypefaceStyle.Normal );
                    Title.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Medium_FontSize );
                    Title.SetSingleLine( );
                    Title.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
                    Title.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
                    TitleLayout.AddView( Title );

                    Address = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Address.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Address.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                    Address.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    Address.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    TitleLayout.AddView( Address );

                    Neighborhood = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Neighborhood.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Neighborhood.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                    Neighborhood.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    Neighborhood.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    TitleLayout.AddView( Neighborhood );

                    Distance  = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Distance.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Distance.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                    Distance.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    Distance.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    TitleLayout.AddView( Distance );

                    // add our own custom seperator at the bottom
                    View seperator = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    seperator.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, 0 );
                    seperator.LayoutParameters.Height = 2;
                    seperator.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );
                    AddView( seperator );
                }
            }

            public class GroupFinderFragment : TaskFragment, IOnMapReadyCallback, TextView.IOnEditorActionListener, Android.Gms.Maps.GoogleMap.IOnMarkerClickListener
            {
                ListView ListView { get; set; }

                LinearLayout AddressLayout { get; set; }
                ProgressBar ProgressBar { get; set; }
                public EditText Address { get; set; }
                public Android.Gms.Maps.MapView MapView { get; set; }
                public GoogleMap Map { get; set; }
                public TextView SearchResult { get; set; }
                View Seperator { get; set; }
                public List<GroupFinder.GroupEntry> GroupEntries { get; set; }


                public override void OnCreate( Bundle savedInstanceState )
                {
                    base.OnCreate( savedInstanceState );

                    GroupEntries = new List<GroupFinder.GroupEntry>();

                    AddressLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    AddressLayout.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    AddressLayout.Orientation = Orientation.Horizontal;

                    ProgressBar = new ProgressBar( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    ProgressBar.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)ProgressBar.LayoutParameters ).Gravity = GravityFlags.Right;
                    ProgressBar.Indeterminate = true;
                    ProgressBar.Visibility = ViewStates.Invisible;

                    Address = new EditText( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Address.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)Address.LayoutParameters ).Weight = 1;
                    Address.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Large_Font_Bold ), TypefaceStyle.Normal );
                    Address.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Large_FontSize );
                    Address.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
                    Address.SetMaxLines( 1 );
                    Address.SetSingleLine( );
                    Address.Hint = ConnectStrings.GroupFinder_AddressPlaceholder;
                    Address.SetOnEditorActionListener( this );

                    // limit the address to 90% of the screen so it doesn't conflict with the progress bar.
                    Point displaySize = new Point( );
                    Activity.WindowManager.DefaultDisplay.GetSize( displaySize );
                    int fixedWidth = (int) ( displaySize.X * 90.0f );
                    Address.SetMinWidth( fixedWidth );
                    Address.SetMaxWidth( fixedWidth );


                    MapView = new Android.Gms.Maps.MapView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    MapView.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );
                    MapView.LayoutParameters.Height = 600;
                    MapView.GetMapAsync( this );
                    MapView.SetBackgroundColor( Color.Black );
                    MapView.OnCreate( savedInstanceState );

                    SearchResult  = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    SearchResult.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    SearchResult.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                    SearchResult.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    SearchResult.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    SearchResult.Text = ConnectStrings.GroupFinder_BeforeSearch;
                    SearchResult.Gravity = GravityFlags.Center;

                    Seperator = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Seperator.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, 0 );
                    Seperator.LayoutParameters.Height = 2;
                    Seperator.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

                    ListView = new ListView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    ListView.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );
                    ListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e ) =>
                        {
                            ParentTask.OnClick( this, e.Position );
                        };
                    ListView.SetOnTouchListener( this );
                    ListView.Adapter = new GroupArrayAdapter( this );
                }

                public bool OnEditorAction(TextView v, Android.Views.InputMethods.ImeAction actionId, KeyEvent keyEvent)
                {
                    // don't allow searching until the map is valid (which it should be by now)
                    if ( Map != null )
                    {
                        GetGroups( v.Text );
                    }
                    return false;
                }

                public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
                {
                    if (container == null)
                    {
                        // Currently in a layout without a container, so no reason to create our view.
                        return null;
                    }

                    View view = inflater.Inflate(Resource.Layout.Connect_GroupFinder, container, false);
                    view.SetOnTouchListener( this );

                    view.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

                    // setup the address layout, which has the address text, padding, and finally the progress bar.
                    ((LinearLayout)view).AddView( AddressLayout );
                    AddressLayout.AddView( Address );
                    AddressLayout.AddView( ProgressBar );

                    ((LinearLayout)view).AddView( MapView );
                    ((LinearLayout)view).AddView( SearchResult );
                    ((LinearLayout)view).AddView( Seperator );
                    ((LinearLayout)view).AddView( ListView );

                    return view;
                }

                public void OnMapReady( GoogleMap map )
                {
                    Map = map;

                    Map.SetOnMarkerClickListener( this );

                    // set the map to a default position
                    // additionally, set the default position for the map to whatever specified area.
                    Android.Gms.Maps.Model.LatLng defaultPos = new Android.Gms.Maps.Model.LatLng( ConnectConfig.GroupFinder_DefaultLatitude, ConnectConfig.GroupFinder_DefaultLongitude );

                    CameraUpdate camPos = CameraUpdateFactory.NewLatLngZoom( defaultPos,  ConnectConfig.GroupFinder_DefaultScale_Android );
                    map.MoveCamera( camPos );


                    // see if there's an address for this person that we can automatically use.
                    if ( RockMobileUser.Instance.HasFullAddress( ) == true )
                    {
                        Address.Text = RockMobileUser.Instance.Street1( ) + " " + RockMobileUser.Instance.City( ) + ", " + RockMobileUser.Instance.State( ) + ", " + RockMobileUser.Instance.Zip( );
                        GetGroups( Address.Text );
                    }
                }

                public bool OnMarkerClick( Android.Gms.Maps.Model.Marker marker )
                {
                    return false;
                }

                public override void OnSaveInstanceState(Bundle outState)
                {
                    base.OnSaveInstanceState(outState);
                    MapView.OnSaveInstanceState( outState );
                }

                public override void OnLowMemory()
                {
                    base.OnLowMemory();
                    MapView.OnLowMemory( );
                }

                public override void OnResume()
                {
                    base.OnResume();
                    MapView.OnResume( );

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.RevealForTime( 3.0f );
                }

                public override void OnDestroy()
                {
                    base.OnDestroy();
                    MapView.OnDestroy( );
                }

                public void OnClick( int position, int buttonIndex )
                {
                }

                public override void OnPause()
                {
                    base.OnPause();

                    MapView.OnPause( );
                }

                void GetGroups( string address )
                {
                    Address.Enabled = false;
                    ProgressBar.Visibility = ViewStates.Visible;

                    CCVApp.Shared.GroupFinder.GetGroups( address, delegate(bool result, List<GroupFinder.GroupEntry> groupEntry )
                        {
                            if ( result == true )
                            {
                                Map.Clear( );

                                Android.Gms.Maps.Model.LatLngBounds.Builder builder = new Android.Gms.Maps.Model.LatLngBounds.Builder( );

                                for( int i = 0; i < groupEntry.Count; i++ )
                                {
                                    // add the positions to the map
                                    Android.Gms.Maps.Model.MarkerOptions markerOptions = new Android.Gms.Maps.Model.MarkerOptions( );
                                    Android.Gms.Maps.Model.LatLng pos = new Android.Gms.Maps.Model.LatLng( double.Parse( groupEntry[ i ].Latitude ), double.Parse( groupEntry[ i ].Longitude ) );
                                    markerOptions.SetPosition( pos  );
                                    markerOptions.SetTitle( groupEntry[ i ].Title );
                                    markerOptions.SetSnippet( groupEntry[ i ].Distance );

                                    builder.Include( pos );

                                    Map.AddMarker( markerOptions );
                                }

                                if( groupEntry.Count > 0 )
                                {
                                    Android.Gms.Maps.Model.LatLngBounds bounds = builder.Build( );

                                    CameraUpdate camPos = CameraUpdateFactory.NewLatLngBounds( bounds, 200 );
                                    Map.AnimateCamera( camPos );

                                    SearchResult.Text = ConnectStrings.GroupFinder_GroupsFound;

                                    // record an analytic that they searched
                                    GroupFinderAnalytic.Instance.Trigger( GroupFinderAnalytic.Location, address );

                                    GroupFinderAnalytic.Instance.Trigger( GroupFinderAnalytic.Neighborhood, groupEntry[ 0 ].NeighborhoodArea );
                                }
                                else
                                {
                                    SearchResult.Text = ConnectStrings.GroupFinder_NoGroupsFound;

                                    // record that this address failed
                                    GroupFinderAnalytic.Instance.Trigger( GroupFinderAnalytic.OutOfBounds, address );
                                }

                                GroupEntries = groupEntry;
                                ( ListView.Adapter as BaseAdapter ).NotifyDataSetChanged( );
                            }
                            else
                            {
                                Springboard.DisplayError( ConnectStrings.GroupFinder_InvalidAddressHeader, ConnectStrings.GroupFinder_InvalidAddressMsg );
                                SearchResult.Text = ConnectStrings.GroupFinder_BeforeSearch;
                            }

                            Address.Enabled = true;
                            ProgressBar.Visibility = ViewStates.Invisible;
                        } );
                }
            }
        }
    }
}

