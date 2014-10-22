
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

namespace Droid
{
    namespace Tasks
    {
        namespace News
        {
            public class NewsArrayAdapter : BaseAdapter
            {
                List<Bitmap> NewsImage { get; set; }

                NewsPrimaryFragment ParentFragment { get; set; }

                public NewsArrayAdapter( NewsPrimaryFragment parentFragment, List<Bitmap> newsImage )
                {
                    ParentFragment = parentFragment;

                    NewsImage = newsImage;
                }

                public override int Count 
                {
                    get { return NewsImage.Count; }
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
                    ImageView view = (ImageView) convertView ?? new ImageView( ParentFragment.Activity.BaseContext );
                    view.SetImageBitmap( NewsImage[ position ] );

                    return view;
                }
            }

            public class NewsPrimaryFragment : TaskFragment
            {
                public List<RockNews> News { get; set; }
                List<Bitmap> NewsImage { get; set; }

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

                    View view = inflater.Inflate(Resource.Layout.PrimaryNews, container, false);
                    view.SetOnTouchListener( this );

                    NewsImage = new List<Bitmap>( );
                    foreach( RockNews item in News )
                    {
                        // load the stream from assets
                        System.IO.Stream assetStream = Activity.BaseContext.Assets.Open( item.ImageName );
                        NewsImage.Add( BitmapFactory.DecodeStream( assetStream ) );
                    }

                    ListView listView = view.FindViewById<ListView>( Resource.Id.news_primary_list );
                    listView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => 
                        {
                            ParentTask.OnClick( this, e.Position );
                        };

                    NewsArrayAdapter adapter = new NewsArrayAdapter( this, NewsImage );

                    listView.Adapter = adapter;

                    return view;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.DisplayShareButton( false, null );
                }

                protected override void TouchUpInside(View v)
                {
                    // reveal the nav bar temporarily
                    ParentTask.NavbarFragment.NavToolbar.RevealForTime( 3.00f );
                }
            }
        }
    }
}

