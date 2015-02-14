using System;
using Android.App;
using Android.Views;

namespace Droid
{
    namespace Tasks
    {
        namespace Connect
        {
            public class ConnectTask : Task
            {
                ConnectPrimaryFragment MainPage { get; set; }
                GroupFinderFragment GroupFinder { get; set; }
                ConnectWebFragment WebPage { get; set; }

                public ConnectTask( NavbarFragment navFragment ) : base( navFragment )
                {
                    // create our fragments (which are basically equivalent to iOS ViewControllers)
                    MainPage = (ConnectPrimaryFragment) NavbarFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Connect.ConnectPrimaryFragment" );
                    if( MainPage == null )
                    {
                        MainPage = new ConnectPrimaryFragment( );
                    }
                    MainPage.ParentTask = this;

                    GroupFinder = (GroupFinderFragment) NavbarFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Connect.GroupFinderFragment" );
                    if( GroupFinder == null )
                    {
                        GroupFinder = new GroupFinderFragment( );
                    }
                    GroupFinder.ParentTask = this;

                    WebPage = (ConnectWebFragment) NavbarFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Connect.ConnectWebFragment" );
                    if( WebPage == null )
                    {
                        WebPage = new ConnectWebFragment( );
                    }
                    WebPage.ParentTask = this;
                }

                public override TaskFragment StartingFragment()
                {
                    return MainPage;
                }

                public override void OnUp( MotionEvent e )
                {
                    base.OnUp( e );

                    if ( GroupFinder.IsVisible == false )
                    {
                        NavbarFragment.NavToolbar.RevealForTime( 3.00f );
                    }
                }

                public override void OnClick(Fragment source, int buttonId, object context)
                {
                    base.OnClick(source, buttonId, context);

                    // only handle input if the springboard is closed
                    if ( NavbarFragment.ShouldTaskAllowInput( ) )
                    {
                        // decide what to do.
                        if ( source == MainPage )
                        {
                            if ( buttonId == -1 )
                            {
                                // launch group finder
                                PresentFragment( GroupFinder, true );
                            }
                            else
                            {
                                // launch the ConnectWebFragment.
                                WebPage.DisplayUrl( (string)context );
                                PresentFragment( WebPage, true );
                            }
                        }
                        else if ( source == WebPage )
                        {
                            NavbarFragment.NavToolbar.RevealForTime( 3.00f );
                        }
                    }
                }
            }
        }
    }
}

