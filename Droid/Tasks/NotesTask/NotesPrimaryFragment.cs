
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
using CCVApp.Shared.Network;
using Android.Graphics;
using RestSharp;
using Rock.Mobile.Network;
using CCVApp.Shared.Notes.Model;
using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;
using Rock.Mobile.PlatformUI;
using System.Net;
using System.IO;
using CCVApp.Shared;
using Rock.Mobile.PlatformUI.DroidNative;
using System.Threading;

namespace Droid
{
    namespace Tasks
    {
        namespace Notes
        {
            public class NotesArrayAdapter : BaseAdapter
            {
                List<SeriesEntry> SeriesEntries { get; set; }
                NotesPrimaryFragment ParentFragment { get; set; }
                Bitmap ImageMainPlaceholder { get; set; }
                Bitmap ImageThumbPlaceholder { get; set; }

                public NotesArrayAdapter( NotesPrimaryFragment parentFragment, List<SeriesEntry> series, Bitmap imageMainPlaceholder, Bitmap imageThumbPlaceholder )
                {
                    ParentFragment = parentFragment;

                    SeriesEntries = series;

                    ImageMainPlaceholder = imageMainPlaceholder;
                    ImageThumbPlaceholder = imageThumbPlaceholder;
                }

                public override int Count 
                {
                    get { return SeriesEntries.Count + 1; }
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
                    if ( position == 0 )
                    {
                        return GetPrimaryView( convertView, parent );
                    }
                    else
                    {
                        return GetStandardView( position - 1, convertView, parent );
                    }
                }

                View GetPrimaryView( View convertView, ViewGroup parent )
                {
                    SeriesPrimaryListItem primaryItem = convertView as SeriesPrimaryListItem;
                    if ( primaryItem == null )
                    {
                        primaryItem = new SeriesPrimaryListItem( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    }

                    primaryItem.ParentAdapter = this;

                    primaryItem.Billboard.SetImageBitmap( SeriesEntries[ 0 ].Billboard != null ? SeriesEntries[ 0 ].Billboard : ImageMainPlaceholder );
                    primaryItem.Billboard.SetScaleType( ImageView.ScaleType.CenterCrop );

                    if ( SeriesEntries[ 0 ].Series.Messages.Count > 0 )
                    {
                        primaryItem.Title.Text = SeriesEntries[ 0 ].Series.Messages[ 0 ].Name;
                        primaryItem.Speaker.Text = SeriesEntries[ 0 ].Series.Messages[ 0 ].Speaker;
                        primaryItem.Date.Text = SeriesEntries[ 0 ].Series.Messages[ 0 ].Date;

                        // toggle the Take Notes button
                        if ( string.IsNullOrEmpty( SeriesEntries[ 0 ].Series.Messages[ 0 ].NoteUrl ) == false )
                        {
                            primaryItem.ToggleTakeNotesButton( true );
                        }
                        else
                        {
                            primaryItem.ToggleTakeNotesButton( false );
                        }

                        // toggle the Watch button
                        if ( string.IsNullOrEmpty( SeriesEntries[ 0 ].Series.Messages[ 0 ].WatchUrl ) == false )
                        {
                            primaryItem.ToggleWatchButton( true );
                        }
                        else
                        {
                            primaryItem.ToggleWatchButton( false );
                        }
                    }
                    else
                    {
                        primaryItem.ToggleTakeNotesButton( false );
                        primaryItem.ToggleWatchButton( false );
                    }

                    return primaryItem;
                }

                View GetStandardView( int position, View convertView, ViewGroup parent )
                {
                    SeriesListItem seriesItem = convertView as SeriesListItem;
                    if ( seriesItem == null )
                    {
                        seriesItem = new SeriesListItem( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    }

                    seriesItem.Thumbnail.SetImageBitmap( SeriesEntries[ position ].Thumbnail != null ? SeriesEntries[ position ].Thumbnail : ImageThumbPlaceholder );
                    seriesItem.Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );

                    seriesItem.Title.Text = SeriesEntries[ position ].Series.Name;
                    seriesItem.DateRange.Text = SeriesEntries[ position ].Series.DateRanges;

                    return seriesItem;
                }

                public void WatchButtonClicked( )
                {
                    ParentFragment.WatchButtonClicked( );
                }

                public void TakeNotesButtonClicked( )
                {
                    ParentFragment.TakeNotesButtonClicked( );
                }
            }

            internal class BorderedActionButton
            {
                public BorderedRectView Layout { get; set; }
                public Button Button { get; set; }
                public TextView Icon { get; set; }
                public TextView Label { get; set; }

                public void AddToView( ViewGroup parentView )
                {
                    // first, create the layout that will store the button and label
                    Layout = new BorderedRectView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Layout.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    parentView.AddView( Layout );

                    // now create the linearLayout that will store the button labels (Symbol & Text)
                    LinearLayout labelLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    labelLayout.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (RelativeLayout.LayoutParams)labelLayout.LayoutParameters ).AddRule( LayoutRules.CenterInParent );
                    Layout.AddView( labelLayout );

                    // add the button, which is just a frame wrapping the entire layout
                    Button = new Button( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Button.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );
                    Button.SetBackgroundDrawable( null );
                    Layout.AddView( Button );

                    // now set the icon
                    Icon = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    labelLayout.AddView( Icon );

                    // and lastly the text
                    Label = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Label.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)Label.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    labelLayout.AddView( Label );
                }
            }

            public class SeriesPrimaryListItem : LinearLayout
            {
                public NotesArrayAdapter ParentAdapter { get; set; }

                //TextView Header { get; set; }
                LinearLayout DetailsLayout { get; set; }

                // stuff that will be set by data
                public Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView Billboard { get; set; }
                public TextView Title { get; set; }
                public TextView Date { get; set; }
                public TextView Speaker { get; set; }
                //

                LinearLayout ButtonLayout { get; set; }
                BorderedActionButton WatchButton { get; set; }
                BorderedActionButton TakeNotesButton { get; set; }

                TextView Footer { get; set; }

                public SeriesPrimaryListItem( Context context ) : base( context )
                {
                    SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

                    Orientation = Orientation.Vertical;

                    /*Header = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Header.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Header.Text = MessagesStrings.Series_TopBanner;
                    Header.SetTypeface( Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Medium_Font_Regular ), TypefaceStyle.Normal );
                    Header.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Medium_FontSize );
                    Header.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    ( (LinearLayout.LayoutParams)Header.LayoutParameters ).Gravity = GravityFlags.Center;
                    AddView( Header );*/

                    Billboard = new Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Billboard.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    Billboard.SetScaleType( ImageView.ScaleType.CenterCrop );
                    AddView( Billboard );

                    Title = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Title.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Title.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Large_Font_Bold ), TypefaceStyle.Normal );
                    Title.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Large_FontSize );
                    Title.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).TopMargin = 25;
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).LeftMargin = 25;
                    AddView( Title );

                    DetailsLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    DetailsLayout.Orientation = Orientation.Horizontal;
                    DetailsLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.WrapContent, LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)DetailsLayout.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    ( (LinearLayout.LayoutParams)DetailsLayout.LayoutParameters ).LeftMargin = 25;
                    ( (LinearLayout.LayoutParams)DetailsLayout.LayoutParameters ).RightMargin = 25;
                    ( (LinearLayout.LayoutParams)DetailsLayout.LayoutParameters ).BottomMargin = 50;
                    AddView( DetailsLayout );

                    Date = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Date.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Date.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                    Date.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    Date.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    DetailsLayout.AddView( Date );

                    // fill the remaining space with a dummy view, and that will align our speaker to the right
                    View dummyView = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    dummyView.LayoutParameters = new LinearLayout.LayoutParams( 0, 0 );
                    ( (LinearLayout.LayoutParams)dummyView.LayoutParameters ).Weight = 1;
                    DetailsLayout.AddView( dummyView );

                    Speaker = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Speaker.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Speaker.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                    Speaker.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    Speaker.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    DetailsLayout.AddView( Speaker );


                    // setup the buttons
                    ButtonLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    ButtonLayout.Orientation = Orientation.Horizontal;
                    ButtonLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.MatchParent, LayoutParams.WrapContent );
                    AddView( ButtonLayout );


                    // Watch Button
                    WatchButton = new BorderedActionButton();
                    WatchButton.AddToView( ButtonLayout );

                    ( (LinearLayout.LayoutParams)WatchButton.Layout.LayoutParameters ).LeftMargin = -5;
                    ( (LinearLayout.LayoutParams)WatchButton.Layout.LayoutParameters ).RightMargin = -1;
                    ( (LinearLayout.LayoutParams)WatchButton.Layout.LayoutParameters ).Weight = 1;
                    WatchButton.Layout.BorderWidth = 1;
                    WatchButton.Layout.SetBorderColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );
                    WatchButton.Layout.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

                    WatchButton.Icon.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Icon_Font_Secondary ), TypefaceStyle.Normal );
                    WatchButton.Icon.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Series_Table_IconSize );
                    WatchButton.Icon.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    WatchButton.Icon.Text = NoteConfig.Series_Table_Watch_Icon;

                    WatchButton.Label.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                    WatchButton.Label.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    WatchButton.Label.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    WatchButton.Label.Text = MessagesStrings.Series_Table_Watch;

                    WatchButton.Button.Click += (object sender, EventArgs e ) =>
                    {
                        ParentAdapter.WatchButtonClicked( );
                    };
                    //



                    // TakeNotes Button
                    TakeNotesButton = new BorderedActionButton();
                    TakeNotesButton.AddToView( ButtonLayout );

                    ( (LinearLayout.LayoutParams)TakeNotesButton.Layout.LayoutParameters ).LeftMargin = -2;
                    ( (LinearLayout.LayoutParams)TakeNotesButton.Layout.LayoutParameters ).RightMargin = -5;
                    ( (LinearLayout.LayoutParams)TakeNotesButton.Layout.LayoutParameters ).Weight = 1;
                    TakeNotesButton.Layout.BorderWidth = 1;
                    TakeNotesButton.Layout.SetBorderColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );
                    TakeNotesButton.Layout.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

                    TakeNotesButton.Icon.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Icon_Font_Secondary ), TypefaceStyle.Normal );
                    TakeNotesButton.Icon.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Series_Table_IconSize );
                    TakeNotesButton.Icon.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    TakeNotesButton.Icon.Text = NoteConfig.Series_Table_TakeNotes_Icon;

                    TakeNotesButton.Label.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                    TakeNotesButton.Label.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    TakeNotesButton.Label.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    TakeNotesButton.Label.Text = MessagesStrings.Series_Table_TakeNotes;

                    TakeNotesButton.Button.Click += (object sender, EventArgs e ) =>
                    {
                        ParentAdapter.TakeNotesButtonClicked( );
                    };
                    //


                    Footer = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Footer.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)Footer.LayoutParameters ).TopMargin = -5;
                    Footer.Text = MessagesStrings.Series_Table_PreviousMessages;
                    Footer.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                    Footer.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    Footer.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    Footer.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Table_Footer_Color ) );
                    Footer.Gravity = GravityFlags.Center;
                    AddView( Footer );
                }

                public void ToggleWatchButton( bool enabled )
                {
                    WatchButton.Button.Enabled = enabled;

                    if ( enabled == true )
                    {
                        WatchButton.Icon.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                        WatchButton.Label.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                    }
                    else
                    {
                        WatchButton.Icon.SetTextColor( Color.DimGray );
                        WatchButton.Label.SetTextColor( Color.DimGray );
                    }
                }

                public void ToggleTakeNotesButton( bool enabled )
                {
                    TakeNotesButton.Button.Enabled = enabled;

                    if ( enabled == true )
                    {
                        TakeNotesButton.Icon.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                        TakeNotesButton.Label.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                    }
                    else
                    {
                        TakeNotesButton.Icon.SetTextColor( Color.DimGray );
                        TakeNotesButton.Label.SetTextColor( Color.DimGray );
                    }
                }
            }

            public class SeriesListItem : LinearLayout
            {
                public Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView Thumbnail { get; set; }

                public LinearLayout TitleLayout { get; set; }
                public TextView Title { get; set; }
                public TextView DateRange { get; set; }
                public TextView Chevron { get; set; }
                public View Seperator { get; set; }

                public SeriesListItem( Context context ) : base( context )
                {
                    Orientation = Orientation.Vertical;

                    SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );

                    LinearLayout contentLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    contentLayout.Orientation = Orientation.Horizontal;
                    contentLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.WrapContent, LayoutParams.WrapContent );
                    AddView( contentLayout );

                    Thumbnail = new Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Thumbnail.LayoutParameters = new LinearLayout.LayoutParams( (int)Rock.Mobile.Graphics.Util.UnitToPx( NoteConfig.Series_Main_CellWidth ), (int)Rock.Mobile.Graphics.Util.UnitToPx( NoteConfig.Series_Main_CellHeight ) );
                    ( (LinearLayout.LayoutParams)Thumbnail.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );
                    contentLayout.AddView( Thumbnail );

                    TitleLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    TitleLayout.Orientation = Orientation.Vertical;
                    TitleLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.WrapContent, LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).LeftMargin = 25;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).TopMargin = 50;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).BottomMargin = 50;
                    contentLayout.AddView( TitleLayout );

                    Title = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Title.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Title.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Medium_Font_Regular ), TypefaceStyle.Normal );
                    Title.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Medium_FontSize );
                    Title.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ) );
                    Title.SetSingleLine( );
                    Title.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
                    TitleLayout.AddView( Title );

                    DateRange = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    DateRange.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    DateRange.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                    DateRange.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    DateRange.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    TitleLayout.AddView( DateRange );

                    // fill the remaining space with a dummy view, and that will align our chevron to the right
                    View dummyView = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    dummyView.LayoutParameters = new LinearLayout.LayoutParams( 0, 0 );
                    ( (LinearLayout.LayoutParams)dummyView.LayoutParameters ).Weight = 1;
                    contentLayout.AddView( dummyView );

                    Chevron = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Chevron.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)Chevron.LayoutParameters ).Gravity = GravityFlags.CenterVertical | GravityFlags.Right;
                    Typeface fontFace = Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Icon_Font_Secondary );
                    Chevron.SetTypeface(  fontFace, TypefaceStyle.Normal );
                    Chevron.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Series_Table_IconSize );
                    Chevron.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    Chevron.Text = NoteConfig.Series_Table_Navigate_Icon;
                    contentLayout.AddView( Chevron );

                    // add our own custom seperator at the bottom
                    Seperator = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Seperator.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, 0 );
                    Seperator.LayoutParameters.Height = 2;
                    Seperator.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );
                    AddView( Seperator );
                }
            }

            /// <summary>
            /// A wrapper class that consolidates the message, it's thumbnail and podcast status
            /// </summary>
            public class SeriesEntry
            {
                public Series Series { get; set; }
                public Bitmap Billboard { get; set; }
                public Bitmap Thumbnail { get; set; }
            }

            public class NotesPrimaryFragment : TaskFragment
            {
                public List<SeriesEntry> SeriesEntries { get; set; }

                ProgressBar ProgressBar { get; set; }
                ListView ListView { get; set; }

                Bitmap ImageMainPlaceholder { get; set; }
                Bitmap ImageThumbPlaceholder { get; set; }

                bool FragmentActive { get; set; }

                public NotesPrimaryFragment( ) : base( )
                {
                    SeriesEntries = new List<SeriesEntry>();
                }

                public override void OnCreate(Bundle savedInstanceState)
                {
                    base.OnCreate(savedInstanceState);

                    System.IO.Stream thumbnailStream = Activity.BaseContext.Assets.Open( GeneralConfig.NotesThumbPlaceholder );
                    ImageThumbPlaceholder = BitmapFactory.DecodeStream( thumbnailStream );

                    System.IO.Stream mainImageStream = Activity.BaseContext.Assets.Open( GeneralConfig.NotesMainPlaceholder );
                    ImageMainPlaceholder = BitmapFactory.DecodeStream( mainImageStream );
                }

                public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
                {
                    if (container == null)
                    {
                        // Currently in a layout without a container, so no reason to create our view.
                        return null;
                    }

					View view = inflater.Inflate(Resource.Layout.Notes_Primary, container, false);
                    view.SetOnTouchListener( this );

                    ProgressBar = view.FindViewById<ProgressBar>( Resource.Id.notes_primary_activityIndicator );
                    ListView = view.FindViewById<ListView>( Resource.Id.notes_primary_list );

                    ListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e ) =>
                        {
                            // we ignore a tap on position 0, because that's the header with Watch/Take Notes
                            if( e.Position > 0 )
                            {
                                ParentTask.OnClick( this, e.Position - 1 );
                            }
                        };
                    ListView.SetOnTouchListener( this );

                    return view;
                }

                public void WatchButtonClicked( )
                {
                    // notify the task that the Watch button was clicked
                    ParentTask.OnClick( this, -1, 1 );
                }

                public void TakeNotesButtonClicked( )
                {
                    // notify the task that the Take Notes button was clicked
                    ParentTask.OnClick( this, -1, 2 );
                }

                public Bitmap GetSeriesBillboard( int index )
                {
                    return SeriesEntries[ index ].Billboard != null ? SeriesEntries[ index ].Billboard : ImageMainPlaceholder;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    FragmentActive = true;

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    // what's the state of the series xml?
                    if ( RockLaunchData.Instance.RequestingNoteDB == true )
                    {
                        ProgressBar.Visibility = ViewStates.Visible;

                        // kick off a thread that will poll the download status and
                        // call "SeriesReady()" when the download is finished.
                        Thread waitThread = new Thread( WaitAsync );
                        waitThread.Start( );

                        while ( waitThread.IsAlive == false );
                    }
                    else if ( RockLaunchData.Instance.NeedSeriesDownload( ) == true )
                    {
                        ProgressBar.Visibility = ViewStates.Visible;

                        RockLaunchData.Instance.GetNoteDB( delegate
                            {
                                // don't worry about the result. The point is we tried,
                                // and now will either use downloaded data, saved data, or throw an error to the user.
                                SeriesReady( );
                            } );
                    }
                    else
                    {
                        // we have the series, so we can move forward.
                        SeriesReady( );
                    }
                }

                void WaitAsync( )
                {
                    // while we're still requesting the series, simply wait
                    while ( CCVApp.Shared.Network.RockLaunchData.Instance.RequestingNoteDB == true );

                    // now that tis' finished, update the notes.
                    SeriesReady( );
                }

                void SeriesReady( )
                {
                    // on the main thread, update the list
                    Rock.Mobile.Threading.Util.PerformOnUIThread(delegate
                        {
                            ProgressBar.Visibility = ViewStates.Gone;

                            // if there are now series entries, we're good
                            if ( RockLaunchData.Instance.Data.NoteDB.SeriesList.Count > 0 )
                            {
                                if( FragmentActive == true )
                                {
                                    ListView.Adapter = new NotesArrayAdapter( this, SeriesEntries, ImageMainPlaceholder, ImageThumbPlaceholder );
                                }

                                // setup the series entries either way, because that doesn't require the fragment to be active
                                SetupSeriesEntries( RockLaunchData.Instance.Data.NoteDB.SeriesList );
                            }
                            else
                            {
                                if ( FragmentActive == true )
                                {
                                    // error
                                    Springboard.DisplayError( MessagesStrings.Error_Title, MessagesStrings.Error_Message );
                                }
                            }
                        } );
                }

                void SetupSeriesEntries( List<Series> seriesList )
                {
                    SeriesEntries.Clear( );

                    foreach ( Series series in seriesList )
                    {
                        // add the entry to our list
                        SeriesEntry entry = new SeriesEntry();
                        SeriesEntries.Add( entry );

                        // copy over the series and give it a placeholder image
                        entry.Series = series;

                        // attempt to load both its images from cache
                        bool needDownload = TryLoadCachedImage( entry );
                        if ( needDownload )
                        {
                            // something failed, so see what needs to be downloaded (could be both)
                            if ( entry.Billboard == null )
                            {
                                FileCache.Instance.DownloadFileToCache( entry.Series.BillboardUrl, entry.Series.Name + "_bb", delegate { SeriesImageDownloaded( ); } );
                            }

                            if ( entry.Thumbnail == null )
                            {
                                FileCache.Instance.DownloadFileToCache( entry.Series.ThumbnailUrl, entry.Series.Name + "_thumb", delegate { SeriesImageDownloaded( ); } );
                            }
                        }
                    }
                }

                bool TryLoadCachedImage( SeriesEntry entry )
                {
                    bool needImage = false;

                    // check the billboard
                    if ( entry.Billboard == null )
                    {
                        MemoryStream imageStream = (MemoryStream)FileCache.Instance.LoadFile( entry.Series.Name + "_bb" );
                        if ( imageStream != null )
                        {
                            try
                            {
                                entry.Billboard = BitmapFactory.DecodeStream( imageStream );
                            }
                            catch( Exception )
                            {
                                FileCache.Instance.RemoveFile( entry.Series.Name + "_bb" );
                                System.Console.WriteLine( "Image {0} is corrupt. Removing.", entry.Series.Name + "_bb" );
                            }

                            imageStream.Dispose( );
                        }
                        else
                        {
                            needImage = true;
                        }
                    }

                    // check the thumbnail
                    if ( entry.Thumbnail == null )
                    {
                        MemoryStream imageStream = (MemoryStream)FileCache.Instance.LoadFile( entry.Series.Name + "_thumb" );
                        if ( imageStream != null )
                        {
                            try
                            {
                                entry.Thumbnail = BitmapFactory.DecodeStream( imageStream );
                            }
                            catch( Exception )
                            {
                                FileCache.Instance.RemoveFile( entry.Series.Name + "_thumb" );
                                System.Console.WriteLine( "Image {0} is corrupt. Removing.", entry.Series.Name + "_thumb" );
                            }
                            imageStream.Dispose( );
                        }
                        else
                        {
                            needImage = true;
                        }
                    }

                    return needImage;
                }

                /// <summary>
                /// Goes thru the series list, and for each image not already downloaded, downloads it.
                /// Note that this is different than the normal SetupSeriesEntries above, because that will
                /// only download if the image is corrupt or not loaded, and is "on demand". This is meant to be
                /// called in advance so the images are ready.
                /// </summary>
                public void DownloadImages( )
                {
                    if ( RockLaunchData.Instance.RequestingNoteDB == false && RockLaunchData.Instance.NeedSeriesDownload( ) == false )
                    {
                        // for each entry in the series, see if it has been downloaded yet. If not, do it.
                        foreach ( Series series in RockLaunchData.Instance.Data.NoteDB.SeriesList )
                        {
                            //bool fileExists = FileCache.Instance.FileExists( series.Name + "_bb" );
                            //if ( fileExists == false )
                            {
                                FileCache.Instance.DownloadFileToCache( series.BillboardUrl, series.Name + "_bb", null );
                            }

                            //fileExists = FileCache.Instance.FileExists( series.Name + "_thumb" );
                            //if( fileExists == false )
                            {
                                FileCache.Instance.DownloadFileToCache( series.ThumbnailUrl, series.Name + "_thumb", null );
                            }
                        }   
                    }
                }

                void SeriesImageDownloaded( )
                {
                    if ( FragmentActive == true )
                    {
                        Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                            {
                                // using only the cache, try to load any image that isn't loaded
                                foreach ( SeriesEntry entry in SeriesEntries )
                                {
                                    TryLoadCachedImage( entry );
                                }

                                ( ListView.Adapter as NotesArrayAdapter ).NotifyDataSetChanged( );
                            } );
                    }
                }

                public override void OnPause( )
                {
                    base.OnPause( );

                    FragmentActive = false;
                }
            }
        }
    }
}

