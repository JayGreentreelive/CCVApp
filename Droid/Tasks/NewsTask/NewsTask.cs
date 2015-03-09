using System;
using Android.App;
using Android.Content;
using Android.Views;
using CCVApp.Shared.Network;
using System.Collections.Generic;

namespace Droid
{
    namespace Tasks
    {
        namespace News
        {
            public class NewsTask : Task
            {
                NewsPrimaryFragment MainPage { get; set; }
                NewsDetailsFragment DetailsPage { get; set; }
                NewsWebFragment WebPage { get; set; }

                List<RockNews> News { get; set; }

                public NewsTask( NavbarFragment navFragment ) : base( navFragment )
                {
                    // create our fragments (which are basically equivalent to iOS ViewControllers)
                    MainPage = (NewsPrimaryFragment) NavbarFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.News.NewsPrimaryFragment" );
                    if( MainPage == null )
                    {
                        MainPage = new NewsPrimaryFragment( );
                    }
                    MainPage.ParentTask = this;

                    DetailsPage = (NewsDetailsFragment) NavbarFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.News.NewsDetailsFragment" );
                    if( DetailsPage == null )
                    {
                        DetailsPage = new NewsDetailsFragment( );
                    }
                    DetailsPage.ParentTask = this;

                    WebPage = (NewsWebFragment)NavbarFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.News.NewsWebFragment" );
                    if ( WebPage == null )
                    {
                        WebPage = new NewsWebFragment( );
                    }
                    WebPage.ParentTask = this;

                    // setup a list we can use to cache the news, so should it update we don't use the wrong set.
                    News = new List<RockNews>();
                    MainPage.SourceNews = News;
                }

                public override void Activate( bool forResume )
                {
                    base.Activate( forResume );

                    if ( forResume == false )
                    {
                        // provide the news to the viewer by COPYING it.
                        News.Clear( );
                        foreach ( RockNews newsItem in RockLaunchData.Instance.Data.News )
                        {
                            News.Add( new RockNews( newsItem ) );
                        }
                    }
                }

                public override TaskFragment StartingFragment()
                {
                    return MainPage;
                }

                public override void PerformTaskAction(string action)
                {
                    base.PerformTaskAction(action);

                    switch ( action )
                    {
                        case "Task.Init":
                        {
                            MainPage.DownloadImages( );
                            break;
                        }
                    }
                }

                public override void OnClick(Android.App.Fragment source, int buttonId, object context = null )
                {
                    // only handle input if the springboard is closed
                    if ( NavbarFragment.ShouldTaskAllowInput( ) )
                    {
                        // decide what to do.
                        if ( source == MainPage )
                        {
                            DetailsPage.NewsItem = News[ buttonId ];
                            PresentFragment( DetailsPage, true );
                        }
                        else if ( source == DetailsPage )
                        {
                            if ( buttonId == Resource.Id.news_details_launch_url )
                            {
                                WebPage.DisplayUrl( DetailsPage.NewsItem.ReferenceURL );
                                PresentFragment( WebPage, true );
                            }
                        }
                    }
                }

                public override void OnUp( MotionEvent e )
                {
                    base.OnUp( e );

                    NavbarFragment.NavToolbar.RevealForTime( 3.00f );
                }
            }
        }
    }
}

