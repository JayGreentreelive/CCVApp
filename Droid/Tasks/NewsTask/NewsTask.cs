using System;

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
                    MainPage = new NewsPrimaryFragment( this );
                    DetailsPage = new NewsDetailsFragment( this );
                }

                public override Android.App.Fragment StartingFragment()
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
                            //Console.WriteLine( "Would switch to details fragment" );
                            NavbarFragment.PresentFragment( DetailsPage, true );
                        }
                    }
                }
            }
        }
    }
}

