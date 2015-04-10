using System;
using Android.App;
using Android.Views;
using CCVApp.Shared.Strings;

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
                JoinGroupFragment JoinGroup { get; set; }
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

                    JoinGroup = (JoinGroupFragment)NavbarFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Connect.JoinGroup" );
                    if ( JoinGroup == null )
                    {
                        JoinGroup = new JoinGroupFragment( );
                    }
                    JoinGroup.ParentTask = this;

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
                            if ( buttonId == 0 )
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
                        else if ( source == GroupFinder )
                        {
                            CCVApp.Shared.GroupFinder.GroupEntry entry = (CCVApp.Shared.GroupFinder.GroupEntry)context;

                            JoinGroup.GroupTitle = entry.Title;
                            JoinGroup.Distance = string.Format( "{0:##.0} {1}", entry.Distance, ConnectStrings.GroupFinder_MilesSuffix );
                            JoinGroup.GroupID = entry.Id;
                            JoinGroup.MeetingTime = string.IsNullOrEmpty( entry.MeetingTime) == false ? entry.MeetingTime : ConnectStrings.GroupFinder_ContactForTime;

                            PresentFragment( JoinGroup, true );
                        }
                    }
                }
            }
        }
    }
}

