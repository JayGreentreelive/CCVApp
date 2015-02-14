
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
                public int SelectedIndex { get; set; }

                public GroupArrayAdapter( GroupFinderFragment parentFragment )
                {
                    ParentFragment = parentFragment;
                    SelectedIndex = -1;
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

                    // the list is sorted, so we can safely assume the first entry is the closest group.
                    // Color it uniquely
                    if ( position == 0 )
                    {
                        messageItem.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ConnectConfig.GroupFinder_ClosestGroupColor ) );
                    }
                    else if ( SelectedIndex == position )
                    {
                        messageItem.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );
                    }
                    else
                    {
                        messageItem.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );
                    }

                    messageItem.ParentAdapter = this;
                    messageItem.Position = position;

                    messageItem.Title.Text = ParentFragment.GroupEntries[ position ].Title;
                    //messageItem.Address.Text = ParentFragment.GroupEntries[ position ].Address;
                    messageItem.MeetingTime.Text = string.Format( ConnectStrings.GroupFinder_MeetingTime, ParentFragment.GroupEntries[ position ].Day, ParentFragment.GroupEntries[ position ].Time );

                    // if this is the nearest group, add a label saying so
                    messageItem.Distance.Text = string.Format( "{0:##.0} {1}", ParentFragment.GroupEntries[ position ].Distance, ConnectStrings.GroupFinder_MilesSuffix );
                    if ( position == 0 )
                    {
                        messageItem.Distance.Text += " " + ConnectStrings.GroupFinder_ClosestTag;
                    }

                    messageItem.Neighborhood.Text = string.Format( ConnectStrings.GroupFinder_Neighborhood, ParentFragment.GroupEntries[ position ].NeighborhoodArea );


                    return messageItem;
                }

                public void OnClick( int position, int buttonIndex )
                {
                    ParentFragment.OnClick( position );
                }

                public void SetSelectedRow( int position )
                {
                    // set the selection index
                    SelectedIndex = position;

                    // and, inefficiently, force the whole dumb list to redraw.
                    // It's either this or manage all the list view items myself. Just..no.
                    NotifyDataSetChanged( );
                }
            }

            public class GroupListItem : LinearLayout
            {
                public LinearLayout TitleLayout { get; set; }
                public TextView Title { get; set; }
                //public TextView Address { get; set; }
                public TextView MeetingTime { get; set; }
                public TextView Distance { get; set; }
                public TextView Neighborhood { get; set; }

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

                    /*Address = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Address.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Address.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                    Address.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    Address.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    TitleLayout.AddView( Address );*/

                    MeetingTime = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    MeetingTime.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    MeetingTime.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                    MeetingTime.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    MeetingTime.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
                    TitleLayout.AddView( MeetingTime );

                    Distance  = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Distance.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Distance.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                    Distance.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    Distance.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
                    TitleLayout.AddView( Distance );

                    Neighborhood = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Neighborhood.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Neighborhood.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                    Neighborhood.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    Neighborhood.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    TitleLayout.AddView( Neighborhood );

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
                public EditText Street { get; set; }
                public EditText City { get; set; }
                public EditText State { get; set; }
                public EditText Zip { get; set; }
                public Android.Gms.Maps.MapView MapView { get; set; }
                public GoogleMap Map { get; set; }
                public TextView SearchResult { get; set; }
                View Seperator { get; set; }
                public List<GroupFinder.GroupEntry> GroupEntries { get; set; }
                public List<Android.Gms.Maps.Model.Marker> MarkerList { get; set; }

                View StreetSeperator { get; set; }
                View CitySeperator { get; set; }
                View StateSeperator { get; set; }


                public override void OnCreate( Bundle savedInstanceState )
                {
                    base.OnCreate( savedInstanceState );

                    GroupEntries = new List<GroupFinder.GroupEntry>();
                    MarkerList = new List<Android.Gms.Maps.Model.Marker>();

                    AddressLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    AddressLayout.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    AddressLayout.Orientation = Orientation.Horizontal;

                    ProgressBar = new ProgressBar( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    ProgressBar.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)ProgressBar.LayoutParameters ).Gravity = GravityFlags.Center;
                    ProgressBar.Indeterminate = true;
                    ProgressBar.Visibility = ViewStates.Gone;


                    // limit the address to 90% of the screen so it doesn't conflict with the progress bar.
                    Point displaySize = new Point( );
                    Activity.WindowManager.DefaultDisplay.GetSize( displaySize );
                    float fixedWidth = displaySize.X / 4.0f;


                    // Street
                    Street = new EditText( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Street.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)Street.LayoutParameters ).Weight = 1;
                    Street.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Large_Font_Bold ), TypefaceStyle.Normal );
                    Street.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Large_FontSize );
                    Street.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
                    Street.SetSingleLine( );
                    Street.SetHorizontallyScrolling( true );
                    Street.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
                    Street.Hint = ConnectStrings.GroupFinder_StreetPlaceholder;
                    Street.SetOnEditorActionListener( this );
                    Street.SetMinWidth( (int) (fixedWidth * 1.50f) );
                    Street.SetMaxWidth( (int) (fixedWidth * 1.50f) );
                    Street.SetBackgroundDrawable( null );

                    StreetSeperator = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    StreetSeperator.LayoutParameters = new LinearLayout.LayoutParams( 0, ViewGroup.LayoutParams.MatchParent );
                    StreetSeperator.LayoutParameters.Width = 2;
                    StreetSeperator.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );


                    // City
                    City = new EditText( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    City.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)City.LayoutParameters ).Weight = 1;
                    City.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Large_Font_Bold ), TypefaceStyle.Normal );
                    City.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Large_FontSize );
                    City.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
                    City.SetSingleLine( true );
                    City.SetHorizontallyScrolling( true );
                    City.SetMaxLines( 1 );
                    City.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
                    City.Hint = ConnectStrings.GroupFinder_CityPlaceholder;
                    City.SetOnEditorActionListener( this );
                    City.SetMinWidth( (int) (fixedWidth * 1.25f) );
                    City.SetMaxWidth( (int) (fixedWidth * 1.25f) );
                    City.SetBackgroundDrawable( null );

                    CitySeperator = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    CitySeperator.LayoutParameters = new LinearLayout.LayoutParams( 0, ViewGroup.LayoutParams.MatchParent );
                    CitySeperator.LayoutParameters.Width = 2;
                    CitySeperator.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );


                    // State
                    State = new EditText( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    State.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)State.LayoutParameters ).Weight = 1;
                    State.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Large_Font_Bold ), TypefaceStyle.Normal );
                    State.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Large_FontSize );
                    State.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
                    State.SetSingleLine( );
                    State.Hint = ConnectStrings.GroupFinder_StatePlaceholder;
                    State.Text = ConnectStrings.GroupFinder_DefaultState;
                    State.SetOnEditorActionListener( this );
                    State.SetMinWidth( (int) (fixedWidth / 1.50f) );
                    State.SetMaxWidth( (int) (fixedWidth / 1.50f) );
                    State.SetBackgroundDrawable( null );

                    StateSeperator = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    StateSeperator.LayoutParameters = new LinearLayout.LayoutParams( 0, ViewGroup.LayoutParams.MatchParent );
                    StateSeperator.LayoutParameters.Width = 2;
                    StateSeperator.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );


                    // Zip
                    Zip = new EditText( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Zip.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)Zip.LayoutParameters ).Weight = 1;
                    Zip.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Large_Font_Bold ), TypefaceStyle.Normal );
                    Zip.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Large_FontSize );
                    Zip.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
                    Zip.SetSingleLine( );
                    Zip.SetMaxLines( 1 );
                    Zip.Hint = ConnectStrings.GroupFinder_ZipPlaceholder;
                    Zip.SetOnEditorActionListener( this );
                    Zip.SetMinWidth( (int) (fixedWidth * 1.05f) );
                    Zip.SetMaxWidth( (int) (fixedWidth * 1.05f) );
                    Zip.InputType = Android.Text.InputTypes.ClassNumber;
                    Zip.SetBackgroundDrawable( null );


                    MapView = new Android.Gms.Maps.MapView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    MapView.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );

                    MapView.LayoutParameters.Height = (int) (displaySize.Y * .50f);
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
                            OnClick( e.Position );
                        };
                    ListView.SetOnTouchListener( this );
                    ListView.Adapter = new GroupArrayAdapter( this );
                }

                public bool OnEditorAction(TextView v, Android.Views.InputMethods.ImeAction actionId, KeyEvent keyEvent)
                {
                    // don't allow searching until the map is valid (which it should be by now)
                    if ( Map != null )
                    {
                        GetGroups( );
                    }
                    return true;
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

                    view.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

                    // setup the address layout, which has the address text, padding, and finally the progress bar.
                    ((LinearLayout)view).AddView( AddressLayout );
                    AddressLayout.AddView( Street );
                    AddressLayout.AddView( StreetSeperator );

                    AddressLayout.AddView( City );
                    AddressLayout.AddView( CitySeperator );

                    AddressLayout.AddView( State );
                    AddressLayout.AddView( StateSeperator );

                    AddressLayout.AddView( Zip );

                    ((LinearLayout)view).AddView( MapView );
                    ((LinearLayout)view).AddView( SearchResult );
                    ((LinearLayout)view).AddView( Seperator );
                    ((LinearLayout)view).AddView( ProgressBar );
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
                        Street.Text = RockMobileUser.Instance.Street1( );
                        City.Text = RockMobileUser.Instance.City( );
                        State.Text = RockMobileUser.Instance.State( );
                        Zip.Text = RockMobileUser.Instance.Zip( );

                        GetGroups( );
                    }
                }

                public bool OnMarkerClick( Android.Gms.Maps.Model.Marker marker )
                {
                    // select the appropriate row
                    int position = MapMarkerToRow( marker );

                    ListView.SmoothScrollToPosition( position );
                    ( ListView.Adapter as GroupArrayAdapter ).SetSelectedRow( position );

                    return false;
                }

                int MapMarkerToRow( Android.Gms.Maps.Model.Marker marker )
                {
                    // given a map marker, get the index of it in the row list
                    for ( int i = 0; i < GroupEntries.Count; i++ )
                    {
                        double currLatitude = GroupEntries[ i ].Latitude;
                        double currLongitude = GroupEntries[ i ].Longitude;

                        if ( marker.Position.Latitude == currLatitude &&
                             marker.Position.Longitude == currLongitude )
                        {
                            return i;
                        }
                    }

                    return -1;
                }

                Android.Gms.Maps.Model.Marker RowToMapMarker( int row )
                {
                    // setup the row marker coordinates
                    double rowLatitude = GroupEntries[ row ].Latitude;
                    double rowLongitude = GroupEntries[ row ].Longitude;

                    // go thru each marker and find the match, and then return it
                    foreach ( Android.Gms.Maps.Model.Marker marker in MarkerList )
                    {
                        if ( marker.Position.Latitude == rowLatitude &&
                             marker.Position.Longitude == rowLongitude )
                        {
                            return marker;
                        }
                    }

                    return null;
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

                public void OnClick( int position )
                {
                    // select the row
                    ( ListView.Adapter as GroupArrayAdapter ).SetSelectedRow( position );

                    // scroll it into view
                    ListView.SmoothScrollToPosition( position );

                    // hide all other marker windows (if showing)
                    // go thru each marker and find the match, and then return it
                    foreach ( Android.Gms.Maps.Model.Marker currMarker in MarkerList )
                    {
                        currMarker.HideInfoWindow( );
                    }

                    // center that map marker
                    Android.Gms.Maps.Model.Marker marker = RowToMapMarker( position );
                    marker.ShowInfoWindow( );

                    Android.Gms.Maps.Model.LatLng centerMarker = new Android.Gms.Maps.Model.LatLng( marker.Position.Latitude, marker.Position.Longitude );

                    CameraUpdate camPos = CameraUpdateFactory.NewLatLngZoom( centerMarker, Map.CameraPosition.Zoom );
                    Map.AnimateCamera( camPos, 250, null );
                }

                public override void OnPause()
                {
                    base.OnPause();

                    MapView.OnPause( );
                }

                void UpdateMap( )
                {
                    Map.Clear( );
                    MarkerList.Clear( );

                    Android.Gms.Maps.Model.LatLngBounds.Builder builder = new Android.Gms.Maps.Model.LatLngBounds.Builder();

                    for ( int i = 0; i < GroupEntries.Count; i++ )
                    {
                        // add the positions to the map
                        Android.Gms.Maps.Model.MarkerOptions markerOptions = new Android.Gms.Maps.Model.MarkerOptions();
                        Android.Gms.Maps.Model.LatLng pos = new Android.Gms.Maps.Model.LatLng( GroupEntries[ i ].Latitude, GroupEntries[ i ].Longitude );
                        markerOptions.SetPosition( pos );
                        markerOptions.SetTitle( GroupEntries[ i ].Title );
                        markerOptions.SetSnippet( string.Format( "{0:##.0} {1}", GroupEntries[ i ].Distance, ConnectStrings.GroupFinder_MilesSuffix ) );

                        builder.Include( pos );

                        Android.Gms.Maps.Model.Marker marker = Map.AddMarker( markerOptions );
                        MarkerList.Add( marker );
                    }

                    string address = Street.Text + " " + City.Text + ", " + State.Text + ", " + Zip.Text;

                    if ( GroupEntries.Count > 0 )
                    {
                        Android.Gms.Maps.Model.LatLngBounds bounds = builder.Build( );

                        CameraUpdate camPos = CameraUpdateFactory.NewLatLngBounds( bounds, 200 );
                        Map.AnimateCamera( camPos );

                        // show the info window for the first (closest) group
                        MarkerList[ 0 ].ShowInfoWindow( );

                        SearchResult.Text = ConnectStrings.GroupFinder_GroupsFound;

                        // record an analytic that they searched
                        GroupFinderAnalytic.Instance.Trigger( GroupFinderAnalytic.Location, address );
                        GroupFinderAnalytic.Instance.Trigger( GroupFinderAnalytic.Neighborhood, GroupEntries[ 0 ].NeighborhoodArea );
                    }
                    else
                    {
                        SearchResult.Text = ConnectStrings.GroupFinder_NoGroupsFound;

                        // no groups found, so move the camera to the default position
                        Android.Gms.Maps.Model.LatLng defaultPos = new Android.Gms.Maps.Model.LatLng( ConnectConfig.GroupFinder_DefaultLatitude, ConnectConfig.GroupFinder_DefaultLongitude );
                        CameraUpdate camPos = CameraUpdateFactory.NewLatLngZoom( defaultPos,  ConnectConfig.GroupFinder_DefaultScale_Android );
                        Map.AnimateCamera( camPos );

                        // record that this address failed
                        GroupFinderAnalytic.Instance.Trigger( GroupFinderAnalytic.OutOfBounds, address );
                    }
                }

                void GetGroups( )
                {
                    if ( string.IsNullOrEmpty( Street.Text ) == false &&
                         string.IsNullOrEmpty( City.Text ) == false &&
                         string.IsNullOrEmpty( State.Text ) == false &&
                         string.IsNullOrEmpty( Zip.Text ) == false )
                    {
                        Street.Enabled = false;
                        City.Enabled = false;
                        State.Enabled = false;
                        Zip.Enabled = false;

                        ProgressBar.Visibility = ViewStates.Visible;

                        CCVApp.Shared.GroupFinder.GetGroups( Street.Text, City.Text, State.Text, Zip.Text, delegate( List<GroupFinder.GroupEntry> groupEntries )
                            {
                                groupEntries.Sort( delegate(GroupFinder.GroupEntry x, GroupFinder.GroupEntry y) 
                                    {
                                        return x.Distance < y.Distance ? -1 : 1;
                                    });

                                GroupEntries = groupEntries;

                                UpdateMap( );

                                ( ListView.Adapter as GroupArrayAdapter ).SetSelectedRow( -1 );

                                Street.Enabled = true;
                                City.Enabled = true;
                                State.Enabled = true;
                                Zip.Enabled = true;
                                ProgressBar.Visibility = ViewStates.Gone;
                            } );
                    }
                }
            }
        }
    }
}

