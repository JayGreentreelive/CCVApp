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
                PrayerCreateFragment CreatePage { get; set; }

                public PrayerTask( NavbarFragment navFragment ) : base( navFragment )
                {
                    // create our fragments (which are basically equivalent to iOS ViewControllers)
                    MainPage = (PrayerPrimaryFragment) NavbarFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Prayer.PrayerPrimaryFragment" );
                    if( MainPage == null )
                    {
                        MainPage = new PrayerPrimaryFragment( );
                    }
                    MainPage.ParentTask = this;

                    CreatePage = (PrayerCreateFragment)NavbarFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Prayer.PrayerCreateFragment" );
                    if ( CreatePage == null )
                    {
                        CreatePage = new PrayerCreateFragment();
                    }
                    CreatePage.ParentTask = this;
                }

                public override void Activate( bool forResume )
                {
                    base.Activate( forResume );
                }

                public override TaskFragment StartingFragment()
                {
                    return MainPage;
                }

                public override void OnClick(Android.App.Fragment source, int buttonId, object context = null)
                {
                    // only handle input if the springboard is closed
                    if ( NavbarFragment.ShouldTaskAllowInput( ) )
                    {
                        // if the main page is the source
                        if ( source == MainPage )
                        {
                            // and it's button id 0, goto the create page
                            if ( buttonId == 0 )
                            {
                                PresentFragment( CreatePage, true );
                            }
                        }
                    }
                }

                public override void SpringboardDidAnimate(bool springboardRevealed)
                {
                    base.SpringboardDidAnimate(springboardRevealed);

                    if ( springboardRevealed == false )
                    {
                        // let the main page know the springboard closed.
                        MainPage.SpringboardClosed( );
                    }
                }
            }
        }
    }
}

