
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
using Rock.Mobile.PlatformCommon;
using Rock.Mobile.PlatformUI;
using System.IO;
using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;

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

                    messageItem.WatchButton.Enabled = Messages[ position ].HasPodcast;

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
                public DroidScaledImageView Thumbnail { get; set; }
                public TextView Title { get; set; }
                public TextView DateRange { get; set; }
                public TextView Desc { get; set; }
                //


                TextView Footer { get; set; }

                public MessagePrimaryListItem( Context context ) : base( context )
                {
                    SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

                    Orientation = Orientation.Vertical;

                    Thumbnail = new DroidScaledImageView( Rock.Mobile.PlatformCommon.Droid.Context );
                    Thumbnail.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );
                    AddView( Thumbnail );

                    Title = new TextView( Rock.Mobile.PlatformCommon.Droid.Context );
                    Title.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Title.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Series_Table_Medium_FontSize );
                    Title.SetTextColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).TopMargin = 25;
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).LeftMargin = 25;
                    AddView( Title );

                    DateRange = new TextView( Rock.Mobile.PlatformCommon.Droid.Context );
                    DateRange.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    DateRange.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Series_Table_Small_FontSize );
                    DateRange.SetTextColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    ( (LinearLayout.LayoutParams)DateRange.LayoutParameters ).TopMargin = 25;
                    ( (LinearLayout.LayoutParams)DateRange.LayoutParameters ).LeftMargin = 25;
                    AddView( DateRange );

                    Desc = new TextView( Rock.Mobile.PlatformCommon.Droid.Context );
                    Desc.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Desc.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Series_Table_Small_FontSize );
                    Desc.SetTextColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    ( (LinearLayout.LayoutParams)Desc.LayoutParameters ).TopMargin = 25;
                    ( (LinearLayout.LayoutParams)Desc.LayoutParameters ).LeftMargin = 25;
                    ( (LinearLayout.LayoutParams)Desc.LayoutParameters ).RightMargin = 25;
                    ( (LinearLayout.LayoutParams)Desc.LayoutParameters ).BottomMargin = 50;
                    AddView( Desc );

                    Footer = new TextView( Rock.Mobile.PlatformCommon.Droid.Context );
                    Footer.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    Footer.SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.Table_Footer_Color ) );
                    Footer.Gravity = GravityFlags.Center;
                    AddView( Footer );
                }
            }

            public class MessageListItem : LinearLayout
            {
                public LinearLayout TitleLayout { get; set; }
                public TextView Title { get; set; }
                public TextView Date { get; set; }
                public TextView Speaker { get; set; }

                RelativeLayout ButtonFrameLayout { get; set; }
                public Button WatchButton { get; set; }
                Button TakeNotesButton { get; set; }

                public NotesDetailsArrayAdapter ParentAdapter { get; set; }
                public int Position { get; set; }

                public MessageListItem( Context context ) : base( context )
                {
                    Orientation = Orientation.Horizontal;

                    SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BackgroundColor ) );
                    LayoutParameters = new AbsListView.LayoutParams( LayoutParams.MatchParent, LayoutParams.MatchParent );

                    TitleLayout = new LinearLayout( Rock.Mobile.PlatformCommon.Droid.Context );
                    TitleLayout.Orientation = Orientation.Vertical;
                    TitleLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.WrapContent, LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).Weight = 1;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).LeftMargin = 25;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).TopMargin = 50;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).BottomMargin = 50;
                    AddView( TitleLayout );

                    Title = new TextView( Rock.Mobile.PlatformCommon.Droid.Context );
                    Title.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Title.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Series_Table_Medium_FontSize );
                    Title.SetSingleLine( );
                    Title.Ellipsize = Android.Text.TextUtils.TruncateAt.End;
                    Title.SetTextColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    TitleLayout.AddView( Title );

                    Date = new TextView( Rock.Mobile.PlatformCommon.Droid.Context );
                    Date.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Date.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Series_Table_Small_FontSize );
                    Date.SetTextColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    TitleLayout.AddView( Date );

                    Speaker = new TextView( Rock.Mobile.PlatformCommon.Droid.Context );
                    Speaker.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Speaker.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Series_Table_Small_FontSize );
                    Speaker.SetTextColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    TitleLayout.AddView( Speaker );


                    // setup the buttons
                    Typeface buttonFontFace = DroidFontManager.Instance.GetFont( ControlStylingConfig.Icon_Font_Primary );

                    WatchButton = new Button( Rock.Mobile.PlatformCommon.Droid.Context );
                    WatchButton.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)WatchButton.LayoutParameters ).Weight = 0;
                    ( (LinearLayout.LayoutParams)WatchButton.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    WatchButton.SetTypeface( buttonFontFace, TypefaceStyle.Normal );
                    WatchButton.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Series_Table_IconSize );
                    WatchButton.Text = NoteConfig.Series_Table_Watch_Icon;
                    WatchButton.SetBackgroundDrawable( null );
                    AddView( WatchButton );

                    TakeNotesButton = new Button( Rock.Mobile.PlatformCommon.Droid.Context );
                    TakeNotesButton.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)TakeNotesButton.LayoutParameters ).Weight = 0;
                    ( (LinearLayout.LayoutParams)TakeNotesButton.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    TakeNotesButton.SetTypeface( buttonFontFace, TypefaceStyle.Normal );
                    TakeNotesButton.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Series_Table_IconSize );
                    TakeNotesButton.Text = NoteConfig.Series_Table_TakeNotes_Icon;
                    TakeNotesButton.SetBackgroundDrawable( null );
                    AddView( TakeNotesButton );

                    WatchButton.Click += (object sender, EventArgs e ) =>
                        {
                            ParentAdapter.OnClick( Position, 0 );
                        };

                    TakeNotesButton.Click += (object sender, EventArgs e ) =>
                        {
                            ParentAdapter.OnClick( Position, 1 );
                        };
                }
            }

            /// <summary>
            /// A wrapper class that consolidates the message, it's thumbnail and podcast status
            /// </summary>
            public class MessageEntry
            {
                public Series.Message Message { get; set; }
                public Bitmap Thumbnail { get; set; }
                public bool HasPodcast { get; set; }
            }

            public class NotesDetailsFragment : TaskFragment
            {
                public Series Series { get; set; }
                public List<MessageEntry> Messages { get; set; }
                public Bitmap ThumbnailPlaceholder { get; set; }

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
                    ParentTask.NavbarFragment.NavToolbar.RevealForTime( 3.0f );

                    Messages.Clear( );

                    // setup the messages list
                    MessagesListView.Adapter = new NotesDetailsArrayAdapter( this, Messages, Series, ThumbnailPlaceholder );

                    // now add the messages
                    for ( int i = 0; i < Series.Messages.Count; i++ )
                    {
                        MessageEntry messageEntry = new MessageEntry();
                        Messages.Add( messageEntry );

                        // give each message entry its message and the default thumbnail
                        messageEntry.Message = Series.Messages[ i ];
                        messageEntry.Thumbnail = ThumbnailPlaceholder;

                        // grab the thumbnail IF it has a podcast
                        if ( string.IsNullOrEmpty( Series.Messages[ i ].WatchUrl ) == false )
                        {
                            messageEntry.HasPodcast = true;

                            int requestedIndex = i;

                            // first see if the image is cached
                            MemoryStream image = ImageCache.Instance.ReadImage( messageEntry.Message.Name );
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
                                            Rock.Mobile.Threading.UIThreading.PerformOnUIThread(delegate
                                                {
                                                    // whether we're still active or not, cache the image downloaded
                                                    ImageCache.Instance.WriteImage( imageBuffer, messageEntry.Message.Name );

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
                        }
                    }
                }

                void ApplyBillboardImage( MessageEntry messageEntry, MemoryStream imageBuffer )
                {
                    Android.Graphics.Bitmap bitmap = BitmapFactory.DecodeStream( imageBuffer );

                    messageEntry.Thumbnail = bitmap;
                    ( MessagesListView.Adapter as NotesDetailsArrayAdapter ).NotifyDataSetChanged( );
                }

                public override void OnPause( )
                {
                    base.OnPause( );

                    FragmentActive = false;

                    // free all associated bitmaps
                    foreach ( MessageEntry messageEntry in Messages )
                    {
                        // don't dump our one placeholder image. We need that one in memory
                        if ( messageEntry.Thumbnail != ThumbnailPlaceholder )
                        {
                            messageEntry.Thumbnail.Dispose( );
                        }
                    }
                    Messages.Clear( );
                }
            }
        }
    }
}
