
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
using System.IO;
using Rock.Mobile.PlatformSpecific.Android.Graphics;
using CCVApp.Shared.Config;

namespace Droid
{
    namespace Tasks
    {
        namespace News
        {
            public class NewsArrayAdapter : BaseAdapter
            {
                List<NewsEntry> News { get; set; }
                Bitmap Placeholder { get; set; }

                NewsPrimaryFragment ParentFragment { get; set; }
                public NewsArrayAdapter( NewsPrimaryFragment parentFragment, List<NewsEntry> news, Bitmap placeholder )
                {
                    ParentFragment = parentFragment;

                    News = news;
                    Placeholder = placeholder;
                }

                public override int Count 
                {
                    get { return News.Count; }
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
                    AspectScaledImageView scaledImageView = convertView as AspectScaledImageView;
                    if ( scaledImageView == null )
                    {
                        scaledImageView = new AspectScaledImageView( Rock.Mobile.PlatformSpecific.Android.Core.Context );
                        scaledImageView.LayoutParameters = new AbsListView.LayoutParams( ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent );
                    }

                    if ( News[ position ].Image != null )
                    {
                        scaledImageView.SetImageBitmap( News[ position ].Image );
                    }
                    else
                    {
                        scaledImageView.SetImageBitmap( Placeholder );
                    }
                    scaledImageView.SetScaleType( ImageView.ScaleType.CenterCrop );

                    return scaledImageView;
                }
            }

            public class NewsEntry
            {
                public RockNews News { get; set; }
                public Bitmap Image { get; set; }
            }

            public class NewsPrimaryFragment : TaskFragment
            {
                public List<NewsEntry> News { get; set; }
                public List<RockNews> SourceNews { get; set; }

                ListView ListView { get; set; }

                Bitmap Placeholder { get; set; }

                bool FragmentActive { get; set; }

                /// <summary>
                /// Used so that in our first OnResume we initialize the news ONCE
                /// so we have something to show while the real news is being downloaded.
                /// </summary>
                bool DidInitNews { get; set; }

                public NewsPrimaryFragment( ) : base( )
                {
                    News = new List<NewsEntry>();

                    DidInitNews = false;
                }

                public override void OnCreate( Bundle savedInstanceState )
                {
                    base.OnCreate( savedInstanceState );

                    System.IO.Stream thumbnailStream = Activity.BaseContext.Assets.Open( GeneralConfig.NewsMainPlaceholder );
                    Placeholder = BitmapFactory.DecodeStream( thumbnailStream );
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

                    ListView = view.FindViewById<ListView>( Resource.Id.news_primary_list );
                    ListView.ItemClick += (object sender, AdapterView.ItemClickEventArgs e) => 
                        {
                            ParentTask.OnClick( this, e.Position );
                        };
                    ListView.SetOnTouchListener( this );

                    return view;
                }

                public override void OnResume()
                {
                    base.OnResume();

                    ParentTask.NavbarFragment.NavToolbar.SetBackButtonEnabled( false );
                    ParentTask.NavbarFragment.NavToolbar.SetCreateButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.SetShareButtonEnabled( false, null );
                    ParentTask.NavbarFragment.NavToolbar.Reveal( false );

                    FragmentActive = true;

                    if ( DidInitNews == false )
                    {
                        ReloadNews( );
                        DidInitNews = true;
                    }

                    ListView.Adapter = new NewsArrayAdapter( this, News, Placeholder );
                }

                public void ReloadNews( )
                {
                    // be sure to dump the existing news images so
                    // Dalvik knows it can use the memory
                    foreach ( NewsEntry newsEntry in News )
                    {
                        if ( newsEntry.Image != null )
                        {
                            newsEntry.Image.Dispose( );
                            newsEntry.Image = null;
                        }
                    }

                    News.Clear( );

                    foreach ( RockNews rockEntry in SourceNews )
                    {
                        NewsEntry newsEntry = new NewsEntry();
                        News.Add( newsEntry );

                        newsEntry.News = rockEntry;

                        TryLoadCachedImage( newsEntry );
                    }

                    // if we've already created the list source, refresh it
                    if ( ListView.Adapter != null )
                    {
                        ( ListView.Adapter as NewsArrayAdapter ).NotifyDataSetChanged( );
                    }
                }

                public void DownloadImages( )
                {
                    foreach ( NewsEntry newsEntry in News )
                    {
                        if ( newsEntry.Image == null )
                        {
                            FileCache.Instance.DownloadFileToCache( newsEntry.News.ImageURL, newsEntry.News.ImageName, delegate { SeriesImageDownloaded( ); } );
                            FileCache.Instance.DownloadFileToCache( newsEntry.News.HeaderImageURL, newsEntry.News.HeaderImageName, null );
                        }
                    }
                }

                bool TryLoadCachedImage( NewsEntry entry )
                {
                    bool needImage = false;

                    // check the billboard
                    if ( entry.Image == null )
                    {
                        MemoryStream imageStream = (MemoryStream)FileCache.Instance.LoadFile( entry.News.ImageName );
                        if ( imageStream != null )
                        {
                            try
                            {
                                entry.Image = BitmapFactory.DecodeStream( imageStream );
                            }
                            catch( Exception )
                            {
                                FileCache.Instance.RemoveFile( entry.News.ImageName );
                                Console.WriteLine( "Image {0} was corrupt. Removing.", entry.News.ImageName );
                            }

                            imageStream.Dispose( );
                            imageStream = null;
                        }
                        else
                        {
                            needImage = true;
                        }
                    }

                    return needImage;
                }

                void SeriesImageDownloaded( )
                {
                    Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                        {
                            // using only the cache, try to load any image that isn't loaded
                            foreach ( NewsEntry entry in News )
                            {
                                TryLoadCachedImage( entry );
                            }

                            if ( FragmentActive == true )
                            {
                                ( ListView.Adapter as NewsArrayAdapter ).NotifyDataSetChanged( );
                            }
                        } );
                }

                public override void OnPause()
                {
                    base.OnPause();

                    FragmentActive = false;
                }
            }
        }
    }
}

