using System;
using Android.App;

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

                public override TaskFragment StartingFragment()
                {
                    return MainPage;
                }

                public override void OnClick(Android.App.Fragment source, int buttonId)
                {
                    // decide what to do.
                    if( source == MainPage )
                    {
                        if( buttonId == Resource.Id.detailsButton )
                        {
                            PresentFragment( DetailsPage, true );
                        }
                    }
                }
            }
        }
    }
}

