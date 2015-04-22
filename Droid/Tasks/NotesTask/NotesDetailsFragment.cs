
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
using CCVApp.Shared;
using Rock.Mobile.PlatformUI;
using System.IO;
using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;
using CCVApp.Shared.Analytics;

namespace Droid
{
    namespace Tasks
    {
        namespace Notes
        {
            public class NotesDetailsArrayAdapter : BaseAdapter
            {
                List<MessageEntry> Messages { get; set; }

                Series Series { get; set; }
                Bitmap SeriesBillboard { get; set; }

                NotesDetailsFragment ParentFragment { get; set; }

                public NotesDetailsArrayAdapter( NotesDetailsFragment parentFragment, List<MessageEntry> messages, Series series, Bitmap seriesBillboard )
                {
                    ParentFragment = parentFragment;

                    Messages = messages;

                    Series = series;

                    SeriesBillboard = seriesBillboard;
                }

                public override int Count 
                {
                    get { return Messages.Count + 1; }
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
                    MessagePrimaryListItem messageItem = convertView as MessagePrimaryListItem;
                    if ( messageItem == null )
                    {
                        messageItem = new MessagePrimaryListItem( ParentFragment.Activity.BaseContext );
                    }

                    messageItem.ParentAdapter = this;

                    messageItem.Thumbnail.SetImageBitmap( SeriesBillboard );
                    messageItem.Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );

                    messageItem.Title.Text = Series.Name;
                    messageItem.DateRange.Text = Series.DateRanges;
                    messageItem.Desc.Text = Series.Description;

                    return messageItem;
                }

                View GetStandardView( int position, View convertView, ViewGroup parent )
                {
                    MessageListItem messageItem = convertView as MessageListItem;
                    if ( messageItem == null )
                    {
                        messageItem = new MessageListItem( ParentFragment.Activity.BaseContext );
                    }

                    messageItem.ParentAdapter = this;
                    messageItem.Position = position;

                    messageItem.Title.Text = Messages[ position ].Message.Name;
                    messageItem.Date.Text = Messages[ position ].Message.Date;
                    messageItem.Speaker.Text = Messages[ position ].Message.Speaker;

                    if ( string.IsNullOrEmpty( Messages[ position ].Message.AudioUrl ) == true )
                    {
                        messageItem.ToggleListenButton( false );
                    }
                    else
                    {
                        messageItem.ToggleListenButton( true );
                    }

                    if ( string.IsNullOrEmpty( Messages[ position ].Message.WatchUrl ) == true )
                    {
                        messageItem.ToggleWatchButton( false );
                    }
                    else
                    {
                        messageItem.ToggleWatchButton( true );
                    }

                    if ( string.IsNullOrEmpty( Messages[ position ].Message.NoteUrl ) == true )
                    {
                        messageItem.ToggleTakeNotesButton( false );
                    }
                    else
                    {
                        messageItem.ToggleTakeNotesButton( true );
                    }

                    return messageItem;
                }

                public void OnClick( int position, int buttonIndex )
                {
                    ParentFragment.OnClick( position, buttonIndex );
                }
            }

            public class MessagePrimaryListItem : LinearLayout
            {
                public NotesDetailsArrayAdapter ParentAdapter { get; set; }

                // stuff that will be set by data
                public Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView Thumbnail { get; set; }
                public TextView Title { get; set; }
                public TextView DateRange { get; set; }
                public TextView Desc { get; set; }
                //

                public MessagePrimaryListItem( Context context ) : base( context )
                {
                    SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

                    Orientation = Orientation.Vertical;

                    Thumbnail = new Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Thumbnail.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );
                    AddView( Thumbnail );

                    Title = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Title.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Title.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Large_Font_Bold ), TypefaceStyle.Normal );
                    Title.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Large_FontSize );
                    Title.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).TopMargin = 25;
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).LeftMargin = 25;
                    AddView( Title );

                    DateRange = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    DateRange.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    DateRange.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Light ), TypefaceStyle.Normal );
                    DateRange.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    DateRange.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
                    ( (LinearLayout.LayoutParams)DateRange.LayoutParameters ).TopMargin = 0;
                    ( (LinearLayout.LayoutParams)DateRange.LayoutParameters ).LeftMargin = 25;
                    AddView( DateRange );

                    Desc = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Desc.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Desc.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Light ), TypefaceStyle.Normal );
                    Desc.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    Desc.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
                    ( (LinearLayout.LayoutParams)Desc.LayoutParameters ).TopMargin = 10;
                    ( (LinearLayout.LayoutParams)Desc.LayoutParameters ).LeftMargin = 25;
                    ( (LinearLayout.LayoutParams)Desc.LayoutParameters ).RightMargin = 25;
                    ( (LinearLayout.LayoutParams)Desc.LayoutParameters ).BottomMargin = 25;
                    AddView( Desc );
                }
            }

            public class MessageListItem : LinearLayout
            {
                public LinearLayout TitleLayout { get; set; }
                public TextView Title { get; set; }
                public TextView Date { get; set; }
                public TextView Speaker { get; set; }

                RelativeLayout ButtonFrameLayout { get; set; }
                Button ListenButton { get; set; }
                Button WatchButton { get; set; }
                Button TakeNotesButton { get; set; }

                public NotesDetailsArrayAdapter ParentAdapter { get; set; }
                public int Position { get; set; }

                public MessageListItem( Context context ) : base( context )
                {
                    SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor ) );
                    LayoutParameters = new AbsListView.LayoutParams( LayoutParams.MatchParent, LayoutParams.MatchParent );

                    Orientation = Orientation.Vertical;

                    LinearLayout contentLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    contentLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.MatchParent, LayoutParams.MatchParent );
                    contentLayout.Orientation = Orientation.Horizontal;
                    AddView( contentLayout );

                    TitleLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    TitleLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.MatchParent, LayoutParams.WrapContent );
                    TitleLayout.Orientation = Orientation.Vertical;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).Weight = 1;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).LeftMargin = 25;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).TopMargin = 50;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).BottomMargin = 50;
                    contentLayout.AddView( TitleLayout );

                    Title = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Title.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Title.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Medium_Font_Bold ), TypefaceStyle.Normal );
                    Title.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Medium_FontSize );
                    Title.SetSingleLine( );
                    Title.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
                    Title.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor ) );
                    TitleLayout.AddView( Title );

                    Date = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Date.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Date.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                    Date.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    Date.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    TitleLayout.AddView( Date );

                    Speaker = new TextView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    Speaker.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Speaker.SetTypeface( Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Small_Font_Regular ), TypefaceStyle.Normal );
                    Speaker.SetTextSize( Android.Util.ComplexUnitType.Dip, ControlStylingConfig.Small_FontSize );
                    Speaker.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    Speaker.SetMaxLines( 1 );
                    TitleLayout.AddView( Speaker );

                    // add our own custom seperator at the bottom
                    View seperator = new View( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    seperator.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, 0 );
                    seperator.LayoutParameters.Height = 2;
                    seperator.SetBackgroundColor( Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ) );
                    AddView( seperator );


                    // setup the buttons
                    LinearLayout buttonLayout = new LinearLayout( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    buttonLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.MatchParent, LayoutParams.MatchParent );
                    ( (LinearLayout.LayoutParams)buttonLayout.LayoutParameters ).Weight = 1;
                    buttonLayout.Orientation = Orientation.Horizontal;
                    contentLayout.AddView( buttonLayout );

                    Typeface buttonFontFace = Rock.Mobile.PlatformSpecific.Android.Graphics.FontManager.Instance.GetFont( ControlStylingConfig.Icon_Font_Secondary );

                    ListenButton = new Button( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    ListenButton.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)ListenButton.LayoutParameters ).Weight = 1;
                    ( (LinearLayout.LayoutParams)ListenButton.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    ListenButton.SetTypeface( buttonFontFace, TypefaceStyle.Normal );
                    ListenButton.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Details_Table_IconSize );
                    ListenButton.Text = NoteConfig.Series_Table_Listen_Icon;
                    ListenButton.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( NoteConfig.Details_Table_IconColor ) );
                    ListenButton.SetBackgroundDrawable( null );
                    buttonLayout.AddView( ListenButton );

                    WatchButton = new Button( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    WatchButton.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)WatchButton.LayoutParameters ).Weight = 1;
                    ( (LinearLayout.LayoutParams)WatchButton.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    WatchButton.SetTypeface( buttonFontFace, TypefaceStyle.Normal );
                    WatchButton.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Details_Table_IconSize );
                    WatchButton.Text = NoteConfig.Series_Table_Watch_Icon;
                    WatchButton.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( NoteConfig.Details_Table_IconColor ) );
                    WatchButton.SetBackgroundDrawable( null );
                    buttonLayout.AddView( WatchButton );

                    TakeNotesButton = new Button( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                    TakeNotesButton.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)TakeNotesButton.LayoutParameters ).Weight = 1;
                    ( (LinearLayout.LayoutParams)TakeNotesButton.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    TakeNotesButton.SetTypeface( buttonFontFace, TypefaceStyle.Normal );
                    TakeNotesButton.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Details_Table_IconSize );
                    TakeNotesButton.Text = NoteConfig.Series_Table_TakeNotes_Icon;
                    TakeNotesButton.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( NoteConfig.Details_Table_IconColor ) );
                    TakeNotesButton.SetBackgroundDrawable( null );
                    buttonLayout.AddView( TakeNotesButton );

                    ListenButton.Click += (object sender, EventArgs e ) =>
                        {
                            ParentAdapter.OnClick( Position, 0 );
                        };

                    WatchButton.Click += (object sender, EventArgs e ) =>
                        {
                            ParentAdapter.OnClick( Position, 1 );
                        };

                    TakeNotesButton.Click += (object sender, EventArgs e ) =>
                        {
                            ParentAdapter.OnClick( Position, 2 );
                        };
                }

                public void ToggleListenButton( bool enabled )
                {
                    if ( enabled == true )
                    {
                        ListenButton.Enabled = true;
                        ListenButton.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( NoteConfig.Details_Table_IconColor ) );
                    }
                    else
                    {
                        ListenButton.Enabled = false;

                        uint disabledColor = Rock.Mobile.Graphics.Util.ScaleRGBAColor( ControlStylingConfig.TextField_PlaceholderTextColor, 2, false );
                        ListenButton.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( disabledColor ) );
                    }
                }

                public void ToggleWatchButton( bool enabled )
                {
                    if ( enabled == true )
                    {
                        WatchButton.Enabled = true;
                        WatchButton.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( NoteConfig.Details_Table_IconColor ) );
                    }
                    else
                    {
                        WatchButton.Enabled = false;

                        uint disabledColor = Rock.Mobile.Graphics.Util.ScaleRGBAColor( ControlStylingConfig.TextField_PlaceholderTextColor, 2, false );
                        WatchButton.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( disabledColor ) );
                    }
                }

                public void ToggleTakeNotesButton( bool enabled )
                {
                    if ( enabled == true )
                    {
                        TakeNotesButton.Enabled = true;
                        TakeNotesButton.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( NoteConfig.Details_Table_IconColor ) );
                    }
                    else
                    {
                        TakeNotesButton.Enabled = false;
                        
                        uint disabledColor = Rock.Mobile.Graphics.Util.ScaleRGBAColor( ControlStylingConfig.TextField_PlaceholderTextColor, 2, false );
                        TakeNotesButton.SetTextColor( Rock.Mobile.PlatformUI.Util.GetUIColor( disabledColor ) );
                    }
                }
            }

            /// <summary>
            /// A wrapper class that consolidates the message, it's thumbnail and podcast status
            /// </summary>
            public class MessageEntry
            {
                public Series.Message Message { get; set; }
                //public Bitmap Thumbnail { get; set; }
                //public bool HasPodcast { get; set; }
            }

            public class NotesDetailsFragment : TaskFragment
            {
                public Series Series { get; set; }
                public List<MessageEntry> Messages { get; set; }
                public Bitmap SeriesBillboard { get; set; }

                ListView MessagesListView { get; set; }

                bool FragmentActive { get; set; }

                public override void OnCreate( Bundle savedInstanceState )
                {
                    base.OnCreate( savedInstanceState );

                    Messages = new List<MessageEntry>();
                }

                public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
                {
                    if (container == null)
                    {
                        // Currently in a layout without a container, so no reason to create our view.
                        return null;
                    }

                    View view = inflater.Inflate(Resource.Layout.Notes_Details, container, false);
                    view.SetOnTouchListener( this );

                    // setup our message list view
                    MessagesListView = view.FindViewById<ListView>( Resource.Id.notes_details_list );
                    MessagesListView.SetOnTouchListener( this );
                    MessagesListView.Divider = null;

                    return view;
                }

                public void OnClick( int position, int buttonIndex )
                {
                    ParentTask.OnClick( this, position, buttonIndex );
                }

                public override void OnResume()
                {
                    base.OnResume();

                    FragmentActive = true;

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( true );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    // log the series they tapped on.
                    MessageAnalytic.Instance.Trigger( MessageAnalytic.BrowseSeries, Series.Name );

                    Messages.Clear( );

                    // setup the messages list
                    MessagesListView.Adapter = new NotesDetailsArrayAdapter( this, Messages, Series, SeriesBillboard );

                    // now add the messages
                    for ( int i = 0; i < Series.Messages.Count; i++ )
                    {
                        MessageEntry messageEntry = new MessageEntry();
                        Messages.Add( messageEntry );

                        // give each message entry its message and the default thumbnail
                        messageEntry.Message = Series.Messages[ i ];
                        //JHM 12-15-14: Don't set thumbnails, the latest design doesn't call for images on the entries.
                        //messageEntry.Thumbnail = ThumbnailPlaceholder;

                        // grab the thumbnail IF it has a podcast
                        /*if ( string.IsNullOrEmpty( Series.Messages[ i ].WatchUrl ) == false )
                        {
                            messageEntry.HasPodcast = true;

                            int requestedIndex = i;

                            // first see if the image is cached
                            MemoryStream image = FileCache.Instance.ReadImage( messageEntry.Message.Name );
                            if ( image != null )
                            {
                                ApplyBillboardImage( messageEntry, image );

                                image.Dispose( );
                            }
                            else
                            {
                                // sucky, it isn't. Download it.
                                VimeoManager.Instance.GetVideoThumbnail( Series.Messages[ requestedIndex ].WatchUrl, 
                                    delegate(System.Net.HttpStatusCode statusCode, string statusDescription, System.IO.MemoryStream imageBuffer )
                                    {
                                        if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                                        {
                                            // on the main thread, update the list
                                            Rock.Mobile.Threading.Util.PerformOnUIThread(delegate
                                                {
                                                    // whether we're still active or not, cache the image downloaded
                                                    FileCache.Instance.WriteImage( imageBuffer, messageEntry.Message.Name );

                                                    // only actually load it if we're still loaded
                                                    if ( FragmentActive == true )
                                                    {
                                                        ApplyBillboardImage( messageEntry, imageBuffer );
                                                    }

                                                    imageBuffer.Dispose( );
                                                });
                                        }
                                    } );
                            }
                        }*/
                    }
                }

                /*void ApplyBillboardImage( MessageEntry messageEntry, MemoryStream imageBuffer )
                {
                    Android.Graphics.Bitmap bitmap = BitmapFactory.DecodeStream( imageBuffer );

                    messageEntry.Thumbnail = bitmap;
                    ( MessagesListView.Adapter as NotesDetailsArrayAdapter ).NotifyDataSetChanged( );
                }*/

                public override void OnPause( )
                {
                    base.OnPause( );

                    FragmentActive = false;

                    // free all associated bitmaps
                    /*foreach ( MessageEntry messageEntry in Messages )
                    {
                        // don't dump our one placeholder image. We need that one in memory
                        if ( messageEntry.Thumbnail != ThumbnailPlaceholder )
                        {
                            messageEntry.Thumbnail.Dispose( );
                        }
                    }*/
                    Messages.Clear( );
                }
            }
        }
    }
}
