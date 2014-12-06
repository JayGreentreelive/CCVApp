
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
                    SeriesPrimaryListItem primaryItem = (SeriesPrimaryListItem)convertView ?? new SeriesPrimaryListItem( ParentFragment.Activity.BaseContext );

                    primaryItem.Thumbnail.SetImageBitmap( SeriesEntries[ 0 ].Thumbnail );
                    primaryItem.Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );

                    primaryItem.Title.Text = SeriesEntries[ 0 ].Series.Messages[ 0 ].Name;
                    primaryItem.Speaker.Text = SeriesEntries[ 0 ].Series.Messages[ 0 ].Speaker;
                    primaryItem.Date.Text = SeriesEntries[ 0 ].Series.Messages[ 0 ].Date;

                    return primaryItem;
                }

                View GetStandardView( int position, View convertView, ViewGroup parent )
                {
                    SeriesListItem seriesItem = (SeriesListItem)convertView ?? new SeriesListItem( ParentFragment.Activity.BaseContext );
                    seriesItem.Thumbnail.SetImageBitmap( SeriesEntries[ position ].Thumbnail );
                    seriesItem.Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );

                    seriesItem.Title.Text = SeriesEntries[ position ].Series.Name;
                    seriesItem.DateRange.Text = SeriesEntries[ position ].Series.DateRanges;

                    return seriesItem;
                }
            }

            public class SeriesPrimaryListItem : LinearLayout
            {
                public TextView Header { get; set; }
                public DroidScaledImageView Thumbnail { get; set; }

                public LinearLayout DetailsLayout { get; set; }
                public TextView Title { get; set; }
                public TextView Date { get; set; }
                public TextView Speaker { get; set; }

                public TextView Footer { get; set; }

                public SeriesPrimaryListItem( Context context ) : base( context )
                {
                    SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BG_Layer_Color ) );

                    Orientation = Orientation.Vertical;

                    Header = new TextView( Rock.Mobile.PlatformCommon.Droid.Context );
                    Header.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Header.Text = MessagesStrings.Series_TopBanner;
                    Header.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Series_Table_Medium_FontSize );
                    Header.SetTextColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    ( (LinearLayout.LayoutParams)Header.LayoutParameters ).Gravity = GravityFlags.Center;
                    AddView( Header );

                    Thumbnail = new DroidScaledImageView( Rock.Mobile.PlatformCommon.Droid.Context );
                    Thumbnail.LayoutParameters = new ViewGroup.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );
                    Thumbnail.Id = 999;
                    AddView( Thumbnail );

                    Title = new TextView( Rock.Mobile.PlatformCommon.Droid.Context );
                    Title.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Title.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Series_Table_Medium_FontSize );
                    Title.SetTextColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).TopMargin = 25;
                    ( (LinearLayout.LayoutParams)Title.LayoutParameters ).LeftMargin = 25;
                    AddView( Title );


                    DetailsLayout = new LinearLayout( Rock.Mobile.PlatformCommon.Droid.Context );
                    DetailsLayout.Orientation = Orientation.Horizontal;
                    DetailsLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.WrapContent, LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)DetailsLayout.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    ( (LinearLayout.LayoutParams)DetailsLayout.LayoutParameters ).LeftMargin = 25;
                    ( (LinearLayout.LayoutParams)DetailsLayout.LayoutParameters ).RightMargin = 25;
                    ( (LinearLayout.LayoutParams)DetailsLayout.LayoutParameters ).BottomMargin = 25;
                    AddView( DetailsLayout );


                    Date = new TextView( Rock.Mobile.PlatformCommon.Droid.Context );
                    Date.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Date.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Series_Table_Small_FontSize );
                    Date.SetTextColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    DetailsLayout.AddView( Date );

                    // fill the remaining space with a dummy view, and that will align our speaker to the right
                    View dummyView = new View( Rock.Mobile.PlatformCommon.Droid.Context );
                    dummyView.LayoutParameters = new LinearLayout.LayoutParams( 0, 0 );
                    ( (LinearLayout.LayoutParams)dummyView.LayoutParameters ).Weight = 1;
                    DetailsLayout.AddView( dummyView );

                    Speaker = new TextView( Rock.Mobile.PlatformCommon.Droid.Context );
                    Speaker.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Speaker.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Series_Table_Small_FontSize );
                    Speaker.SetTextColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    DetailsLayout.AddView( Speaker );


                    Footer = new TextView( Rock.Mobile.PlatformCommon.Droid.Context );
                    Footer.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.WrapContent );
                    Footer.Text = MessagesStrings.Series_Table_PreviousMessages;
                    Footer.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Series_Table_Medium_FontSize );
                    Footer.SetTextColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    Footer.SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.Table_Footer_Color ) );
                    Footer.Gravity = GravityFlags.Center;
                    AddView( Footer );
                }
            }

            public class SeriesListItem : LinearLayout
            {
                public DroidScaledImageView Thumbnail { get; set; }

                public LinearLayout TitleLayout { get; set; }
                public TextView Title { get; set; }
                public TextView DateRange { get; set; }
                public TextView Chevron { get; set; }

                public SeriesListItem( Context context ) : base( context )
                {
                    Orientation = Orientation.Horizontal;

                    SetBackgroundColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.BackgroundColor ) );

                    Thumbnail = new DroidScaledImageView( Rock.Mobile.PlatformCommon.Droid.Context );
                    Thumbnail.LayoutParameters = new LinearLayout.LayoutParams( 280, 280 );
                    ( (LinearLayout.LayoutParams)Thumbnail.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    Thumbnail.SetScaleType( ImageView.ScaleType.CenterCrop );
                    Thumbnail.Id = 777;
                    AddView( Thumbnail );

                    TitleLayout = new LinearLayout( Rock.Mobile.PlatformCommon.Droid.Context );
                    TitleLayout.Orientation = Orientation.Vertical;
                    TitleLayout.LayoutParameters = new LinearLayout.LayoutParams( LayoutParams.WrapContent, LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).LeftMargin = 25;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).TopMargin = 50;
                    ( (LinearLayout.LayoutParams)TitleLayout.LayoutParameters ).BottomMargin = 50;
                    AddView( TitleLayout );

                    Title = new TextView( Rock.Mobile.PlatformCommon.Droid.Context );
                    Title.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    Title.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Series_Table_Medium_FontSize );
                    Title.SetTextColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    TitleLayout.AddView( Title );

                    DateRange = new TextView( Rock.Mobile.PlatformCommon.Droid.Context );
                    DateRange.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    DateRange.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Series_Table_Small_FontSize );
                    DateRange.SetTextColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    TitleLayout.AddView( DateRange );

                    // fill the remaining space with a dummy view, and that will align our chevron to the right
                    View dummyView = new View( Rock.Mobile.PlatformCommon.Droid.Context );
                    dummyView.LayoutParameters = new LinearLayout.LayoutParams( 0, 0 );
                    ( (LinearLayout.LayoutParams)dummyView.LayoutParameters ).Weight = 1;
                    AddView( dummyView );

                    Chevron = new TextView( Rock.Mobile.PlatformCommon.Droid.Context );
                    Chevron.LayoutParameters = new LinearLayout.LayoutParams( ViewGroup.LayoutParams.WrapContent, ViewGroup.LayoutParams.WrapContent );
                    ( (LinearLayout.LayoutParams)Chevron.LayoutParameters ).Gravity = GravityFlags.CenterVertical;
                    Typeface fontFace = DroidFontManager.Instance.GetFont( ControlStylingConfig.Icon_Font_Primary );
                    Chevron.SetTypeface(  fontFace, TypefaceStyle.Normal );
                    Chevron.SetTextSize( Android.Util.ComplexUnitType.Dip, NoteConfig.Series_Table_IconSize );
                    Chevron.SetTextColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor ) );
                    Chevron.Text = NoteConfig.Series_Table_Navigate_Icon;
                    AddView( Chevron );
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

