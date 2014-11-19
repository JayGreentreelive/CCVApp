
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
using Rock.Mobile.PlatformCommon;
using Rock.Mobile.PlatformUI;
using System.Net;
using System.IO;
using CCVApp.Shared;

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

                public NotesArrayAdapter( NotesPrimaryFragment parentFragment, List<SeriesEntry> series )
                {
                    ParentFragment = parentFragment;

                    SeriesEntries = series;
                }

                public override int Count 
                {
                    get { return SeriesEntries.Count; }
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
                    SeriesListItem seriesItem = (SeriesListItem)convertView ?? new SeriesListItem( ParentFragment.Activity.BaseContext );
                    seriesItem.Thumbnail.SetImageBitmap( SeriesEntries[ position ].Thumbnail );
                    seriesItem.Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );

                    seriesItem.Label.Text = SeriesEntries[ position ].Series.Name;

                    return seriesItem;
                }
            }

            public class SeriesListItem : LinearLayout
            {
                public DroidScaledImageView Thumbnail { get; set; }
                public TextView Label { get; set; }

                public SeriesListItem( Context context ) : base( context )
                {
                    Orientation = Orientation.Vertical;

                    Thumbnail = new DroidScaledImageView( Rock.Mobile.PlatformCommon.Droid.Context );
                    Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );
                    AddView( Thumbnail );

                    Label = new TextView( Rock.Mobile.PlatformCommon.Droid.Context );
                    Label.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)Label.LayoutParameters ).Gravity = GravityFlags.Center;
                    ( (LinearLayout.LayoutParams)Label.LayoutParameters ).BottomMargin = (int)PlatformBaseUI.UnitToPx( 25 );
                    AddView( Label );
                }
            }

            /// <summary>
            /// A wrapper class that consolidates the message, it's thumbnail and podcast status
            /// </summary>
            public class SeriesEntry
            {
                public Series Series { get; set; }
                public Bitmap Thumbnail { get; set; }
            }

            public class NotesPrimaryFragment : TaskFragment
            {
                public List<SeriesEntry> SeriesEntries { get; set; }

                ProgressBar ProgressBar { get; set; }
                ListView ListView { get; set; }

                Bitmap ThumbnailPlaceholder { get; set; }
                bool RequestingSeries { get; set; }

                bool FragmentActive { get; set; }

                public NotesPrimaryFragment( ) : base( )
                {
                    SeriesEntries = new List<SeriesEntry>();
                    ThumbnailPlaceholder = BitmapFactory.DecodeResource( Rock.Mobile.PlatformCommon.Droid.Context.Resources, Resource.Drawable.thumbnailPlaceholder );
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
                            ParentTask.OnClick( this, e.Position );
                        };
                    ListView.SetOnTouchListener( this );

                    return view;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    FragmentActive = true;

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    // if we haven't already, request the series
                    if ( SeriesEntries.Count == 0 && RequestingSeries == false )
                    {
                        ProgressBar.Visibility = ViewStates.Visible;

                        RequestingSeries = true;

                        // grab the series info
                        Rock.Mobile.Network.HttpRequest request = new HttpRequest();
                        RestRequest restRequest = new RestRequest( Method.GET );
                        restRequest.RequestFormat = DataFormat.Xml;

                        request.ExecuteAsync<List<Series>>( NoteConfig.BaseURL + "series.xml", restRequest, 
                            delegate(System.Net.HttpStatusCode statusCode, string statusDescription, List<Series> model )
                            {
                                // on the main thread, update the list
                                Rock.Mobile.Threading.UIThreading.PerformOnUIThread(delegate
                                    {
                                        RequestingSeries = false;
                                        ProgressBar.Visibility = ViewStates.Gone;

                                        if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) )
                                        {
                                            if( FragmentActive == true )
                                            {
                                                ListView.Adapter = new NotesArrayAdapter( this, SeriesEntries );
                                            }

                                            // setup the series entries either way, because that doesn't require the fragment to be active
                                            SetupSeriesEntries( model );
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
                            } );
                    }
                    else
                    {
                        ProgressBar.Visibility = ViewStates.Gone;
                        ListView.Adapter = new NotesArrayAdapter( this, SeriesEntries );
                    }
                }

                void SetupSeriesEntries( List<Series> seriesList )
                {
                    foreach ( Series series in seriesList )
                    {
                        // add the entry to our list
                        SeriesEntry entry = new SeriesEntry();
                        SeriesEntries.Add( entry );

                        // copy over the series and give it a placeholder image
                        entry.Series = series;
                        entry.Thumbnail = ThumbnailPlaceholder;

                        // first see if the image is cached
                        MemoryStream image = ImageCache.Instance.ReadImage( entry.Series.Name );
                        if ( image != null )
                        {
                            entry.Thumbnail = BitmapFactory.DecodeStream( image );
                            image.Dispose( );

                            // if we're still active, update the list
                            if ( FragmentActive == true )
                            {
                                ( ListView.Adapter as NotesArrayAdapter ).NotifyDataSetChanged( );
                            }
                        }
                        else
                        {
                            // it ain't, so we need to download it

                            // request the thumbnail image for the series
                            HttpRequest webRequest = new HttpRequest();
                            RestRequest restRequest = new RestRequest( Method.GET );

                            // don't worry about protecting against multiple calls, because if they leave this page and return it will 
                            // abandon these series entry objects and create new ones.
                            webRequest.ExecuteAsync( series.BillboardUrl, restRequest, 
                                delegate(HttpStatusCode statusCode, string statusDescription, byte[] model )
                                {
                                    if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                                    {
                                        // on the main thread, update the images
                                        Rock.Mobile.Threading.UIThreading.PerformOnUIThread(delegate
                                            {
                                                // get it as a stream
                                                MemoryStream memoryStream = new MemoryStream( model );

                                                // write it to cache
                                                ImageCache.Instance.WriteImage( memoryStream, entry.Series.Name );

                                                // apply it to our entry
                                                entry.Thumbnail = BitmapFactory.DecodeStream( memoryStream );
                                                memoryStream.Dispose( );

                                                // if we're still active, update the list
                                                if( FragmentActive == true )
                                                {
                                                    (ListView.Adapter as NotesArrayAdapter).NotifyDataSetChanged( );
                                                }
                                            });
                                    }
                                } );
                        }
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

