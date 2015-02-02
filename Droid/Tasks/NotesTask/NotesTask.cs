using System;
using Android.Views;

namespace Droid
{
    namespace Tasks
    {
        namespace Notes
        {
            public class NotesTask : Task
            {
                NotesFragment NotesPage { get; set; }
                NotesPrimaryFragment MainPage { get; set; }
                NotesDetailsFragment DetailsPage { get; set; }
                NotesWatchFragment WatchPage { get; set; }
                NotesWebViewFragment WebViewPage { get; set; }

                public NotesTask( NavbarFragment navFragment ) : base( navFragment )
                {
                    // create our fragments (which are basically equivalent to iOS ViewControllers)
                    MainPage = (NotesPrimaryFragment) NavbarFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Notes.NotesPrimaryFragment" );
                    if( MainPage == null )
                    {
                        MainPage = new NotesPrimaryFragment( );
                    }

                    DetailsPage = (NotesDetailsFragment)NavbarFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Notes.NotesDetailsFragment" );
                    if ( DetailsPage == null )
                    {
                        DetailsPage = new NotesDetailsFragment( );
                    }

                    NotesPage = (NotesFragment) NavbarFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Notes.NotesFragment" );
                    if( NotesPage == null )
                    {
                        NotesPage = new NotesFragment( );
                    }

                    WatchPage = (NotesWatchFragment)NavbarFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Notes.NotesWatchFragment" );
                    if ( WatchPage == null )
                    {
                        WatchPage = new NotesWatchFragment( );
                    }

                    WebViewPage = (NotesWebViewFragment)NavbarFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Notes.NotesWebViewFragment" );
                    if ( WebViewPage == null )
                    {
                        WebViewPage = new NotesWebViewFragment( );
                    }

                    MainPage.ParentTask = this;
                    DetailsPage.ParentTask = this;
                    NotesPage.ParentTask = this;
                    WatchPage.ParentTask = this;
                    WebViewPage.ParentTask = this;
                }

                public override void SpringboardDidAnimate( bool springboardRevealed )
                {
                    // did the springboard just close?
                    if( springboardRevealed == false )
                    {
                        // if we weren't ready, let the notes know we now are.
                        if( TaskReadyForFragmentDisplay == false )
                        {
                            TaskReadyForFragmentDisplay = true;

                            NotesPage.TaskReadyForFragmentDisplay( );
                        }
                    }
                }

                public override void PerformTaskAction( string action )
                {
                    base.PerformTaskAction( action );

                    switch ( action )
                    {
                        case "Page.Read":
                        {
                            //TODO: We need to get the latest sermon XML from Rock Data. For now,
                            //I know what it'll be named. (it's the date of the Saturday weekend day.)
                            // for now, let the note name be the previous saturday
                            DateTime time = DateTime.UtcNow;

                            // if it's not saturday, find the date of the past saturday
                            if( time.DayOfWeek != DayOfWeek.Saturday )
                            {
                                time = time.Subtract( new TimeSpan( (int)time.DayOfWeek + 1, 0, 0, 0 ) );
                            }

                            NotesPage.NotePresentableName = string.Format( "Sermon Note - {0}.{1}.{2}", time.Month, time.Day, time.Year );
                            NotesPage.NoteName = string.Format("http://www.jeredmcferron.com/ccv/{0}_{1}_{2}_{3}.xml", "message", time.Month, time.Day, time.Year );
                            //

                            PresentFragment( NotesPage, true );
                            break;
                        }
                    }
                }

                public override void Activate( bool forResume )
                {
                    base.Activate( forResume );

                    // if the springboard is already closed, set ourselves as ready.
                    // This is always called before any fragment methods, so the fragment
                    // will be able to know if it can display or not.

                    // alternatively, if we're simply resuming from a pause, it's ok to allow the note to show.
                    if( NavbarFragment.ShouldTaskAllowInput( ) || forResume == true)
                    {
                        TaskReadyForFragmentDisplay = true;
                    }
                    else
                    {
                        TaskReadyForFragmentDisplay = false;
                    }
                }

                public override void Deactivate( bool forPause )
                {
                    base.Deactivate( forPause );

                    TaskReadyForFragmentDisplay = false;
                }

                public override bool CanContainerPan()
                {
                    if ( NotesPage.IsVisible == true )
                    {
                        return NotesPage.MovingUserNote ? false : true;
                    }
                    else
                    {
                        return true;
                    }
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
                        // decide what to do.
                        if ( source == MainPage )
                        {
                            // on the main page, if the buttonId was -1, the user tapped the header,
                            // so we need to either go to the Watch or Take Notes page
                            if ( buttonId == -1 )
                            {
                                // the context is the button they clicked (watch or take notes)
                                int buttonChoice = (int)context;

                                // 0 is watch
                                if ( buttonChoice == 0 )
                                {
                                    WatchPage.VideoUrl = MainPage.SeriesEntries[ 0 ].Series.Messages[ 0 ].WatchUrl;
                                    WatchPage.ShareUrl = MainPage.SeriesEntries[ 0 ].Series.Messages[ 0 ].ShareUrl;
                                    PresentFragment( WatchPage, true );
                                }
                                else if ( buttonChoice == 1 )
                                {
                                    NotesPage.NoteName = MainPage.SeriesEntries[ 0 ].Series.Messages[ 0 ].NoteUrl;
                                    NotesPage.NotePresentableName = MainPage.SeriesEntries[ 0 ].Series.Messages[ 0 ].Name;

                                    PresentFragment( NotesPage, true );
                                }
                            }
                            else
                            {
                                DetailsPage.Series = MainPage.SeriesEntries[ buttonId ].Series;
                                DetailsPage.SeriesBillboard = MainPage.SeriesEntries[ buttonId ].Billboard;
                                PresentFragment( DetailsPage, true );
                            }
                        }
                        else if ( source == DetailsPage )
                        {
                            // the context is the button they clicked (watch or take notes)
                            int buttonChoice = (int)context;

                            if ( buttonChoice == 0 )
                            {
                                WatchPage.VideoUrl = DetailsPage.Messages[ buttonId ].Message.WatchUrl;
                                WatchPage.ShareUrl = DetailsPage.Messages[ buttonId ].Message.ShareUrl;
                                PresentFragment( WatchPage, true );
                            }
                            else if ( buttonChoice == 1 )
                            {
                                NotesPage.NoteName = DetailsPage.Messages[ buttonId ].Message.NoteUrl;
                                NotesPage.NotePresentableName = DetailsPage.Messages[ buttonId ].Message.Name;

                                PresentFragment( NotesPage, true );
                            }
                        }
                        else if ( source == NotesPage )
                        {
                            // the context is the activeURL to visit.
                            string activeUrl = (string)context;
                            WebViewPage.ActiveUrl = activeUrl;

                            PresentFragment( WebViewPage, true );
                        }
                    }
                }

                public override void OnUp( MotionEvent e )
                {
                    base.OnUp( e );

                    // for the notes page, the navbar should show up when we scroll
                    if ( NotesPage.IsVisible == false )
                    {
                        NavbarFragment.NavToolbar.RevealForTime( 3.00f );
                    }
                }
            }
        }
    }
}

