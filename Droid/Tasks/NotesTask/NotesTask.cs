using System;

namespace Droid
{
    namespace Tasks
    {
        namespace Notes
        {
            public class NotesTask : Task
            {
                NotesFragment MainPage { get; set; }

                public NotesTask( NavbarFragment navFragment ) : base( navFragment )
                {
                    // create our fragments (which are basically equivalent to iOS ViewControllers)
                    MainPage = (NotesFragment) NavbarFragment.FragmentManager.FindFragmentByTag( "Droid.Tasks.Notes.NotesFragment" );
                    if( MainPage == null )
                    {
                        MainPage = new NotesFragment( );
                    }

                    // for now, let the note name be the previous saturday
                    DateTime time = DateTime.UtcNow;

                    // if it's not saturday, find the date of the past saturday
                    if( time.DayOfWeek != DayOfWeek.Saturday )
                    {
                        time = time.Subtract( new TimeSpan( (int)time.DayOfWeek + 1, 0, 0, 0 ) );
                    }
                    MainPage.NotePresentableName = string.Format( "Sermon Note - {0}.{1}.{2}", time.Month, time.Day, time.Year );

                    #if DEBUG
                    MainPage.NoteName = "sample_note";
                    #else
                    MainPage.NoteName = string.Format("{0}_{1}_{2}_{3}", CCVApp.Shared.Config.Note.NamePrefix, time.Month, time.Day, time.Year );
                    #endif
                    //

                    MainPage.ParentTask = this;
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

                            MainPage.TaskReadyForFragmentDisplay( );
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
                    return MainPage.MovingUserNote ? false : true;
                }

                public override TaskFragment StartingFragment()
                {
                    return MainPage;
                }
            }
        }
    }
}

