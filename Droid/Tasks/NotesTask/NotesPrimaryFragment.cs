
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

namespace Droid
{
    namespace Tasks
    {
        namespace Notes
        {
            /// <summary>
            /// Subclass ImageView so we can override OnMeasure and scale up the image 
            /// maintaining aspect ratio
            /// </summary>
            class ScaledImageView : ImageView
            {
                public ScaledImageView( Context context ) : base( context )
                {
                }

                protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
                {
                    if ( Drawable != null )
                    {
                        int width = MeasureSpec.GetSize( widthMeasureSpec );
                        int height = (int)Math.Ceiling( width * ( (float)Drawable.IntrinsicHeight / (float)Drawable.IntrinsicWidth ) );

                        SetMeasuredDimension( width, height );
                    }
                    else
                    {
                        base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
                    }
                }
            }

            public class NotesArrayAdapter : BaseAdapter
            {
                //List<Bitmap> NewsImage { get; set; }
                List<Series> Series { get; set; }

                NotesPrimaryFragment ParentFragment { get; set; }

                public NotesArrayAdapter( NotesPrimaryFragment parentFragment, List<Series> series )
                {
                    ParentFragment = parentFragment;

                    Series = series;
                }

                public override int Count 
                {
                    get { return Series.Count; }
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
                    //ScaledImageView view = (ScaledImageView) convertView ?? new ScaledImageView( ParentFragment.Activity.BaseContext );
                    TextView view = (TextView) convertView ?? new TextView( ParentFragment.Activity.BaseContext );
                    view.LayoutParameters = new AbsListView.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );

                    //view.SetImageBitmap( NewsImage[ position ] );
                    //view.SetScaleType( ImageView.ScaleType.CenterCrop );
                    view.Text = Series[ position ].Name + "\n" + Series[ position ].Description + "\n" + Series[ position ].DateRanges;

                    return view;
                }
            }

            public class NotesPrimaryFragment : TaskFragment
            {
                public List<Series> Series { get; set; }
                //List<Bitmap> NewsImage { get; set; }

                ProgressBar ProgressBar { get; set; }
                ListView ListView { get; set; }

                public override void OnCreate( Bundle savedInstanceState )
                {
                    base.OnCreate( savedInstanceState );
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

                    return view;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ProgressBar.Visibility = ViewStates.Visible;

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.DisplayShareButton( false, null );

                    // grab the series info
                    Rock.Mobile.Network.HttpRequest request = new HttpRequest();
                    RestRequest restRequest = new RestRequest( Method.GET );
                    restRequest.RequestFormat = DataFormat.Xml;

                    request.ExecuteAsync<List<Series>>( CCVApp.Shared.Config.Note.BaseURL + "series.xml", restRequest, 
                        delegate(System.Net.HttpStatusCode statusCode, string statusDescription, List<Series> model )
                        {
                            ProgressBar.Visibility = ViewStates.Gone;

                            if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) )
                            {
                                Series = model;

                                // on the main thread, update the list
                                Activity.RunOnUiThread( delegate
                                    {

                                        ListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => 
                                            {
                                                ParentTask.OnClick( this, e.Position );
                                            };
                                        ListView.SetOnTouchListener( this );
                                        ListView.Adapter = new NotesArrayAdapter( this, Series );
                                    });
                            }
                            else
                            {
                                // error
                            }
                        } );
                }
            }
        }
    }
}

