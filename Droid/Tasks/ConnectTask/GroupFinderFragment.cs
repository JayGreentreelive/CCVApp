
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
using Rock.Mobile.Animation;
using Android.Gms.Maps.Model;

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

                    // if there's a meeting time set, display it. Otherwise we won't display that row
                    messageItem.MeetingTime.Visibility = ViewStates.Visible;
                    if ( string.IsNullOrEmpty( ParentFragment.GroupEntries[ position ].MeetingTime ) == false )
                    {
                        messageItem.MeetingTime.Text = ParentFragment.GroupEntries[ position ].MeetingTime;
                    }
                    else
                    {
                        messageItem.MeetingTime.Text = ConnectStrings.GroupFinder_ContactForTime;
                    }

                    // if this is the nearest group, add a label saying so
                    messageItem.Distance.Text = string.Format( "{0:##.0} {1}", ParentFragment.GroupEntries[ position ].Distance, ConnectStrings.GroupFinder_MilesSuffix );
                    if ( position == 0 )
                    {
                        messageItem.Distance.Text += " " + ConnectStrings.GroupFinder_ClosestTag;
                    }

                    //messageItem.Neighborhood.Text = string.Format( ConnectStrings.GroupFinder_Neighborhood, ParentFragment.GroupEntries[ position ].NeighborhoodArea );


                    return messageItem;
                }

                public void OnClick( int position, int buttonIndex )
                {
                    ParentFragment.OnClick( position, buttonIndex );
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

                public Button JoinButton { get; set; }
                //public TextView Neighborhood { get; set; }

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
                    Title.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Medium_Font_Bold ), TypefaceStyle.Normal );
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

                    Typeface buttonFontFace = Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Icon_Font_Secondary );

                    JoinButton = new Button( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    JoinButton.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)JoinButton.LayoutParameters ).Weight = 0;
                    ( (LinearLayout.LayoutParams)JoinButton.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    JoinButton.SetTypeface( buttonFontFace, TypefaceStyle.Normal );
                    JoinButton.SetTextSize( Android.Util.ComplexUnitType.Dip, ConnectConfig.GroupFinder_Join_IconSize );
                    JoinButton.Text = ConnectConfig.GroupFinder_JoinIcon;
                    JoinButton.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    JoinButton.SetBackgroundDrawable( null );
                    JoinButton.FocusableInTouchMode = false;
                    JoinButton.Focusable = false;
                    contentLayout.AddView( JoinButton );

                    JoinButton.Click += (object sender, EventArgs e ) =>
                        {
                            ParentAdapter.OnClick( Position, 1 );
                        };


                    MeetingTime = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    MeetingTime.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    MeetingTime.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Light ), TypefaceStyle.Normal );
                    MeetingTime.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    MeetingTime.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
                    TitleLayout.AddView( MeetingTime );

                    Distance  = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Distance.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Distance.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Light ), TypefaceStyle.Normal );
                    Distance.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    Distance.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
                    TitleLayout.AddView( Distance );

                    /*Neighborhood = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Neighborhood.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Neighborhood.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Light ), TypefaceStyle.Normal );
                    Neighborhood.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    Neighborhood.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    TitleLayout.AddView( Neighborhood );*/

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
                uint StreetBackgroundColor { get; set; }

                public EditText City { get; set; }
                uint CityBackgroundColor { get; set; }

                public EditText State { get; set; }
                uint StateBackgroundColor { get; set; }

                public EditText Zip { get; set; }
                uint ZipBackgroundColor { get; set; }

                public Android.Gms.Maps.MapView MapView { get; set; }
                public GoogleMap Map { get; set; }
                public TextView SearchResult { get; set; }

                public LinearLayout FooterLayout { get; set; }
                public TextView FooterDetailsText { get; set; }
                public TextView FooterJoinText { get; set; }

                View Seperator { get; set; }
                public List<GroupFinder.GroupEntry> GroupEntries { get; set; }
                public List<Android.Gms.Maps.Model.Marker> MarkerList { get; set; }
                public GroupFinder.GroupEntry SourceLocation { get; set; }

                View StreetSeperator { get; set; }
                View CitySeperator { get; set; }
                View StateSeperator { get; set; }

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

                    GroupEntries = new List<GroupFinder.GroupEntry>();
                    MarkerList = new List<Android.Gms.Maps.Model.Marker>();
                    SourceLocation = new GroupFinder.GroupEntry();

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
                    ControlStyling.StyleTextField( Street, ConnectStrings.GroupFinder_StreetPlaceholder, ControlStylingConfig.Large_Font_Bold, ControlStylingConfig.Large_FontSize );
                    Street.SetSingleLine( );
                    Street.SetHorizontallyScrolling( true );
                    Street.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
                    Street.SetOnEditorActionListener( this );
                    Street.SetMinWidth( (int) (fixedWidth * 1.50f) );
                    Street.SetMaxWidth( (int) (fixedWidth * 1.50f) );
                    Street.InputType |= Android.Text.InputTypes.TextFlagNoSuggestions | Android.Text.InputTypes.TextFlagCapWords;
                    StreetBackgroundColor = ControlStylingConfig.BG_Layer_Color;

                    StreetSeperator = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    StreetSeperator.LayoutParameters = new LinearLayout.LayoutParams( 0, ViewGroup.LayoutParams.MatchParent );
                    StreetSeperator.LayoutParameters.Width = 2;
                    StreetSeperator.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );


                    // City
                    City = new EditText( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    City.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)City.LayoutParameters ).Weight = 1;
                    ControlStyling.StyleTextField( City, ConnectStrings.GroupFinder_CityPlaceholder, ControlStylingConfig.Large_Font_Bold, ControlStylingConfig.Large_FontSize );
                    City.SetSingleLine( true );
                    City.SetHorizontallyScrolling( true );
                    City.SetMaxLines( 1 );
                    City.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
                    City.InputType |= Android.Text.InputTypes.TextFlagCapWords;
                    City.SetOnEditorActionListener( this );
                    City.SetMinWidth( (int) (fixedWidth * 1.25f) );
                    City.SetMaxWidth( (int) (fixedWidth * 1.25f) );
                    CityBackgroundColor = ControlStylingConfig.BG_Layer_Color;

                    CitySeperator = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    CitySeperator.LayoutParameters = new LinearLayout.LayoutParams( 0, ViewGroup.LayoutParams.MatchParent );
                    CitySeperator.LayoutParameters.Width = 2;
                    CitySeperator.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );


                    // State
                    State = new EditText( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    State.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)State.LayoutParameters ).Weight = 1;
                    ControlStyling.StyleTextField( State, ConnectStrings.GroupFinder_StatePlaceholder, ControlStylingConfig.Large_Font_Bold, ControlStylingConfig.Large_FontSize );
                    State.SetSingleLine( );
                    State.Hint = ConnectStrings.GroupFinder_StatePlaceholder;
                    State.Text = ConnectStrings.GroupFinder_DefaultState;
                    State.InputType |= Android.Text.InputTypes.TextFlagCapWords;
                    State.SetOnEditorActionListener( this );
                    State.SetMinWidth( (int) (fixedWidth / 1.50f) );
                    State.SetMaxWidth( (int) (fixedWidth / 1.50f) );
                    StateBackgroundColor = ControlStylingConfig.BG_Layer_Color;

                    StateSeperator = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    StateSeperator.LayoutParameters = new LinearLayout.LayoutParams( 0, ViewGroup.LayoutParams.MatchParent );
                    StateSeperator.LayoutParameters.Width = 2;
                    StateSeperator.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );


                    // Zip
                    Zip = new EditText( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Zip.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)Zip.LayoutParameters ).Weight = 1;
                    ControlStyling.StyleTextField( Zip, ConnectStrings.GroupFinder_ZipPlaceholder, ControlStylingConfig.Large_Font_Bold, ControlStylingConfig.Large_FontSize );
                    Zip.SetSingleLine( );
                    Zip.SetMaxLines( 1 );
                    Zip.Hint = ConnectStrings.GroupFinder_ZipPlaceholder;
                    Zip.SetOnEditorActionListener( this );
                    Zip.SetMinWidth( (int) (fixedWidth * 1.05f) );
                    Zip.SetMaxWidth( (int) (fixedWidth * 1.05f) );
                    Zip.InputType = Android.Text.InputTypes.ClassNumber;
                    ZipBackgroundColor = ControlStylingConfig.BG_Layer_Color;


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


                    FooterLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    FooterLayout.Orientation = Android.Widget.Orientation.Horizontal;
                    FooterLayout.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)FooterLayout.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    ( (LinearLayout.LayoutParams)FooterLayout.LayoutParameters ).LeftMargin = 25;

                    FooterDetailsText  = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    FooterDetailsText.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    FooterDetailsText.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                    FooterDetailsText.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    FooterDetailsText.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    FooterDetailsText.Text = ConnectStrings.GroupFinder_DetailsLabel;
                    FooterDetailsText.Gravity = GravityFlags.Center;

                    FooterJoinText  = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    FooterJoinText.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)FooterJoinText.LayoutParameters ).Gravity = GravityFlags.Right;
                    ( (LinearLayout.LayoutParams)FooterJoinText.LayoutParameters ).RightMargin = 25;
                    FooterJoinText.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                    FooterJoinText.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    FooterJoinText.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    FooterJoinText.Text = ConnectStrings.GroupFinder_JoinLabel;
                    FooterJoinText.Gravity = GravityFlags.Center;


                    Seperator = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Seperator.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, 0 );
                    Seperator.LayoutParameters.Height = 2;
                    Seperator.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );

                    ListView = new ListView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    ListView.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );
                    ListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e ) =>
                        {
                            OnClick( e.Position, 0 );
                        };
                    ListView.SetOnTouchListener( this );
                    ListView.Adapter = new GroupArrayAdapter( this );

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
                    ((LinearLayout)view).AddView( FooterLayout );

                    FooterLayout.AddView( FooterDetailsText );

                    // fill the remaining space with a dummy view, and that will align our speaker to the right
                    View dummyView = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    dummyView.LayoutParameters = new LinearLayout.LayoutParams( 0, 0 );
                    ( (LinearLayout.LayoutParams)dummyView.LayoutParameters ).Weight = 1;
                    FooterLayout.AddView( dummyView );

                    FooterLayout.AddView( FooterJoinText );

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

                public void OnClick( int position, int buttonIndex )
                {
                    if ( buttonIndex == 0 )
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
                    else if ( buttonIndex == 1 )
                    {
                        // Ok! notify the parent they tapped Join, and it will launch the
                        // join group fragment! It's MARCH, FRIDAY THE 13th!!!! OH NOOOO!!!!
                        ParentTask.OnClick( this, position, GroupEntries[ position ] );
                        Console.WriteLine( "Join neighborhood group in row {0}", position );
                    }
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

                    string address = Street.Text + " " + City.Text + ", " + State.Text + ", " + Zip.Text;

                    if ( GroupEntries.Count > 0 )
                    {
                        Android.Gms.Maps.Model.LatLngBounds.Builder builder = new Android.Gms.Maps.Model.LatLngBounds.Builder();

                        // add the source position
                        Android.Gms.Maps.Model.MarkerOptions markerOptions = new Android.Gms.Maps.Model.MarkerOptions();
                        Android.Gms.Maps.Model.LatLng pos = new Android.Gms.Maps.Model.LatLng( SourceLocation.Latitude, SourceLocation.Longitude );
                        markerOptions.SetPosition( pos );
                        markerOptions.InvokeIcon( BitmapDescriptorFactory.DefaultMarker( BitmapDescriptorFactory.HueGreen ) );
                        builder.Include( pos );

                        Android.Gms.Maps.Model.Marker marker = Map.AddMarker( markerOptions );
                        MarkerList.Add( marker );

                        for ( int i = 0; i < GroupEntries.Count; i++ )
                        {
                            // add the positions to the map
                            markerOptions = new Android.Gms.Maps.Model.MarkerOptions();
                            pos = new Android.Gms.Maps.Model.LatLng( GroupEntries[ i ].Latitude, GroupEntries[ i ].Longitude );
                            markerOptions.SetPosition( pos );
                            markerOptions.SetTitle( GroupEntries[ i ].Title );
                            markerOptions.SetSnippet( string.Format( "{0:##.0} {1}", GroupEntries[ i ].Distance, ConnectStrings.GroupFinder_MilesSuffix ) );

                            builder.Include( pos );

                            marker = Map.AddMarker( markerOptions );
                            MarkerList.Add( marker );
                        }

                        Android.Gms.Maps.Model.LatLngBounds bounds = builder.Build( );

                        CameraUpdate camPos = CameraUpdateFactory.NewLatLngBounds( bounds, 200 );
                        Map.AnimateCamera( camPos );

                        // show the info window for the first (closest) group
                        MarkerList[ 1 ].ShowInfoWindow( );

                        //SearchResult.Text = ConnectStrings.GroupFinder_GroupsFound;
                        SearchResult.Text = string.Format( ConnectStrings.GroupFinder_Neighborhood, GroupEntries[ 0 ].NeighborhoodArea );

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

                bool RetrievingGroups { get; set; }

                void GetGroups( )
                {
                    if ( RetrievingGroups == false )
                    {
                        RetrievingGroups = true;

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

                            CCVApp.Shared.GroupFinder.GetGroups( Street.Text, City.Text, State.Text, Zip.Text, delegate( GroupFinder.GroupEntry sourceLocation, List<GroupFinder.GroupEntry> groupEntries )
                                {
                                    SourceLocation = sourceLocation;

                                    groupEntries.Sort( delegate(GroupFinder.GroupEntry x, GroupFinder.GroupEntry y )
                                        {
                                            return x.Distance < y.Distance ? -1 : 1;
                                        } );

                                    GroupEntries = groupEntries;

                                    UpdateMap( );

                                    ( ListView.Adapter as GroupArrayAdapter ).SetSelectedRow( -1 );

                                    Street.Enabled = true;
                                    City.Enabled = true;
                                    State.Enabled = true;
                                    Zip.Enabled = true;
                                    ProgressBar.Visibility = ViewStates.Gone;

                                    RetrievingGroups = false;
                                } );
                        }

                        ValidateTextFields( );
                    }
                }

                void ValidateTextFields( )
                {
                    // this will color the invalid fields red so the user knows they need to fill them in.

                    // Also, with the animation complete, set RetrievingGroups to false

                    // Validate Street
                    uint targetStreetColor = string.IsNullOrEmpty( Street.Text ) == true ? ControlStylingConfig.BadInput_BG_Layer_Color : ControlStylingConfig.BG_Layer_Color; 
                    Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( StreetBackgroundColor, targetStreetColor, Street, delegate { StreetBackgroundColor = targetStreetColor; RetrievingGroups = false; } );


                    // Validate City
                    uint targetCityColor = string.IsNullOrEmpty( City.Text ) == true ? ControlStylingConfig.BadInput_BG_Layer_Color : ControlStylingConfig.BG_Layer_Color; 
                    Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( CityBackgroundColor, targetCityColor, City, delegate { CityBackgroundColor = targetCityColor; } );


                    // Validate State
                    uint targetStateColor = string.IsNullOrEmpty( State.Text ) == true ? ControlStylingConfig.BadInput_BG_Layer_Color : ControlStylingConfig.BG_Layer_Color; 
                    Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( StateBackgroundColor, targetStateColor, State, delegate { StateBackgroundColor = targetStateColor; } );


                    // Validate Zip
                    uint targetZipColor = string.IsNullOrEmpty( Zip.Text ) == true ? ControlStylingConfig.BadInput_BG_Layer_Color : ControlStylingConfig.BG_Layer_Color; 
                    Rock.Mobile.PlatformSpecific.Android.UI.Util.AnimateViewColor( ZipBackgroundColor, targetZipColor, Zip, delegate { ZipBackgroundColor = targetZipColor; } );
                }
            }
        }
    }
}

