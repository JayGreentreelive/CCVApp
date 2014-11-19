using System;
using Android.App;
using Android.Content;

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
                }

                public override void Activate( bool forResume )
                {
                    base.Activate( forResume );

                    MainPage.News = CCVApp.Shared.Network.RockGeneralData.Instance.Data.News;
                }

                public override TaskFragment StartingFragment()
                {
                    return MainPage;
                }

                public override void OnClick(Android.App.Fragment source, int buttonId, object context = null )
                {
                    // only handle input if the springboard is closed
                    if ( NavbarFragment.ShouldTaskAllowInput( ) )
                    {
                        // decide what to do.
                        if ( source == MainPage )
                        {
                            DetailsPage.NewsItem = CCVApp.Shared.Network.RockGeneralData.Instance.Data.News[ buttonId ];
                            PresentFragment( DetailsPage, true );
                        }
                        else if ( source == DetailsPage )
                        {
                            if ( buttonId == Resource.Id.news_details_launch_url )
                            {
                                Intent browserIntent = new Intent( Intent.ActionView, Android.Net.Uri.Parse( DetailsPage.NewsItem.ReferenceURL ) );
                                Rock.Mobile.PlatformCommon.Droid.Context.StartActivity( browserIntent );
                            }
                        }
                    }
                }
            }
        }
    }
}

