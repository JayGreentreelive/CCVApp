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
                    // only handle input if the springboard is open
                    if ( NavbarFragment.ShouldTaskAllowInput( ) )
                    {
                        if ( buttonId == Resource.Id.prayer_primary_createPrayerButton )
                        {
                            PresentFragment( CreatePage, true );
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

