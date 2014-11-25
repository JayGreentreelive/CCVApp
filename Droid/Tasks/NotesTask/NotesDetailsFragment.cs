
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

namespace Droid
{
    namespace Tasks
    {
        namespace Notes
        {
            public class NotesDetailsArrayAdapter : BaseAdapter
            {
                List<MessageEntry> Messages { get; set; }

                NotesDetailsFragment ParentFragment { get; set; }

                public NotesDetailsArrayAdapter( NotesDetailsFragment parentFragment, List<MessageEntry> messages )
                {
                    ParentFragment = parentFragment;

                    Messages = messages;
                }

                public override int Count 
                {
                    get { return Messages.Count; }
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
                    MessageListItem messageItem = (MessageListItem)convertView ?? new MessageListItem( ParentFragment.Activity.BaseContext );
                    messageItem.ParentAdapter = this;
                    messageItem.Position = position;

                    messageItem.Thumbnail.SetImageBitmap( Messages[ position ].Thumbnail );
                    messageItem.Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );

                    messageItem.Label.Text = Messages[ position ].Message.Name;

                    messageItem.WatchButton.Enabled = Messages[ position ].HasPodcast;

                    return messageItem;
                }

                public void OnClick( int position, int buttonIndex )
                {
                    ParentFragment.OnClick( position, buttonIndex );
                }
            }

            public class MessageListItem : LinearLayout
            {
                public DroidScaledImageView Thumbnail { get; set; }
                public TextView Label { get; set; }

                RelativeLayout ButtonFrameLayout { get; set; }
                public Button WatchButton { get; set; }
                Button TakeNotesButton { get; set; }

                public NotesDetailsArrayAdapter ParentAdapter { get; set; }
                public int Position { get; set; }

                public MessageListItem( Context context ) : base( context )
                {
                    Orientation = Orientation.Vertical;

                    Thumbnail = new DroidScaledImageView( Rock.Mobile.PlatformCommon.Droid.Context );
                    Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );
                    AddView( Thumbnail );

                    Label = new TextView( Rock.Mobile.PlatformCommon.Droid.Context );
                    Label.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)Label.LayoutParameters ).Gravity = GravityFlags.Center;
                    AddView( Label );

                    // setup the buttons in a relative layout
                    ButtonFrameLayout = new RelativeLayout( Rock.Mobile.PlatformCommon.Droid.Context );
                    ButtonFrameLayout.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    ButtonFrameLayout.SetPadding( 0, 0, 0, (int)PlatformBaseUI.UnitToPx( 25 ) );

                    AddView( ButtonFrameLayout );

                    WatchButton = new Button( Rock.Mobile.PlatformCommon.Droid.Context );
                    WatchButton.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    WatchButton.Text = "Watch";
                    ButtonFrameLayout.AddView( WatchButton );

                    TakeNotesButton = new Button( Rock.Mobile.PlatformCommon.Droid.Context );
                    TakeNotesButton.LayoutParameters = new RelativeLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (RelativeLayout.LayoutParams)TakeNotesButton.LayoutParameters ).AddRule( LayoutRules.AlignParentRight );
                    TakeNotesButton.Text = "Take Notes";
                    ButtonFrameLayout.AddView( TakeNotesButton );

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
                    MessagesListView.Adapter = new NotesDetailsArrayAdapter( this, Messages );

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
