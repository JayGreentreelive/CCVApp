using System;
using Android.App;

namespace Droid
{
    namespace Tasks
    {
        namespace Give
        {
            public class GiveTask : Task
            {
                GivePrimaryFragment MainPage { get; set; }

                public GiveTask( NavbarFragment navFragment ) : base( navFragment )
                {
                    // create our fragments (which are basically equivalent to iOS ViewControllers)
                    MainPage = (GivePrimaryFragment) NavbarFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Give.GivePrimaryFragment" );
                    if( MainPage == null )
                    {
                        MainPage = new GivePrimaryFragment( );
                    }
                    MainPage.ParentTask = this;
                }

                public override TaskFragment StartingFragment()
                {
                    return MainPage;
                }

                public override void OnClick(Android.App.Fragment source, int buttonId)
                {
                    // only handle input if the springboard is open
                    if ( NavbarFragment.ShouldTaskAllowInput( ) )
                    {
                    }
                }
            }
        }
    }
}

