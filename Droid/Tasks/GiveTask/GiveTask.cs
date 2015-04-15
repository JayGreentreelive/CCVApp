using System;
using Android.App;
using Android.Views;
using CCVApp.Shared.Config;

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
                    MainPage = new GivePrimaryFragment( );
                    MainPage.ParentTask = this;
                }

                public override TaskFragment StartingFragment()
                {
                    return MainPage;
                }

                public override void OnUp( MotionEvent e )
                {
                    base.OnUp( e );
                }

                public override void Activate(bool forResume)
                {
                    base.Activate(forResume);

                    if ( forResume == false )
                    {
                        // temp hack to see if we like Give auto launching or not
                        MainPage.LaunchGive( );
                    }
                }
            }
        }
    }
}

