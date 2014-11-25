﻿using System;
using Android.App;
using Android.Views;

namespace Droid
{
    namespace Tasks
    {
        namespace About
        {
            public class AboutTask : Task
            {
                AboutPrimaryFragment MainPage { get; set; }

                public AboutTask( NavbarFragment navFragment ) : base( navFragment )
                {
                    // create our fragments (which are basically equivalent to iOS ViewControllers)
                    MainPage = (AboutPrimaryFragment) NavbarFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.About.AboutPrimaryFragment" );
                    if( MainPage == null )
                    {
                        MainPage = new AboutPrimaryFragment( );
                    }
                    MainPage.ParentTask = this;
                }

                public override TaskFragment StartingFragment()
                {
                    return MainPage;
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

