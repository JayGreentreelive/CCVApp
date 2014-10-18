using System;
using Android.App;
using Android.Content;

namespace Droid
{
    namespace Tasks
    {
        namespace Prayer
        {
            public class PrayerTask : Task
            {
                PrayerPrimaryFragment MainPage { get; set; }

                public PrayerTask( NavbarFragment navFragment ) : base( navFragment )
                {
                    // create our fragments (which are basically equivalent to iOS ViewControllers)
                    MainPage = (PrayerPrimaryFragment) NavbarFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Prayer.PrayerPrimaryFragment" );
                    if( MainPage == null )
                    {
                        MainPage = new PrayerPrimaryFragment( );
                    }
                    MainPage.ParentTask = this;
                }

                public override void Activate( )
                {
                    base.Activate( );
                }

                public override TaskFragment StartingFragment()
                {
                    return MainPage;
                }

                public override void OnClick(Android.App.Fragment source, int buttonId)
                {
                    // decide what to do.
                }
            }
        }
    }
}

