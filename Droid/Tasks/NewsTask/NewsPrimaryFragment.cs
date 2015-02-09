
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
using CCVApp.Shared;

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
                    Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView view = (Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView) convertView ?? new Rock.Mobile.PlatformSpecific.Android.Graphics.AspectScaledImageView( ParentFragment.Activity.BaseContext );
                    view.LayoutParameters = new AbsListView.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );

                    view.SetImageBitmap( NewsImage[ position ] );
                    view.SetScaleType( ImageView.ScaleType.CenterCrop );

                    return view;
                }
            }

            public class NewsPrimaryFragment : TaskFragment
            {
                public List<RockNews> News { get; set; }
                List<Bitmap> NewsImage { get; set; }

                public NewsPrimaryFragment( ) : base( )
                {
                    NewsImage = new List<Bitmap>();
                }

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

					View view = inflater.Inflate(Resource.Layout.News_Primary, container, false);
                    view.SetOnTouchListener( this );

                    // load the news images for each item
                    NewsImage.Clear( );

                    foreach( RockNews item in News )
                    {
                        // attempt to load the image from cache. If that doesn't work, use a placeholder
                        Bitmap imageBanner = null;

                        System.IO.MemoryStream assetStream = (System.IO.MemoryStream)FileCache.Instance.LoadFile( item.ImageName );
                        if ( assetStream!= null )
                        {
                            imageBanner = BitmapFactory.DecodeByteArray( assetStream.GetBuffer( ), 0, (int)assetStream.Length );
                        }
                        else
                        {
                            imageBanner = BitmapFactory.DecodeResource( Rock.Mobile.PlatformSpecific.Android.Core.Context.Resources, Resource.Drawable.thumbnailPlaceholder );
                        }
                        NewsImage.Add( imageBanner );
                    }

                    ListView listView = view.FindViewById<ListView>( Resource.Id.news_primary_list );
                    listView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => 
                        {
                            ParentTask.OnClick( this, e.Position );
                        };
                    listView.SetOnTouchListener( this );
                    listView.Adapter = new NewsArrayAdapter( this, NewsImage );

                    return view;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );
                }
            }
        }
    }
}

