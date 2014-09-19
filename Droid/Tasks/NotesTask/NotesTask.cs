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
                    MainPage.NoteName = string.Format("{0}_{1}_{2}_{3}", CCVApp.Shared.Config.Note.NamePrefix, time.Month, time.Day, time.Year );
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

                public override void Activate( )
                {
                    base.Activate( );

                    // if the springboard is already closed, set ourselves as ready.
                    // This is always called before any fragment methods, so the fragment
                    // will be able to know if it can display or not.
                    if( NavbarFragment.SpringboardRevealed == false )
                    {
                        TaskReadyForFragmentDisplay = true;
                    }
                    else
                    {
                        TaskReadyForFragmentDisplay = false;
                    }
                }

                public override void Deactivate( )
                {
                    base.Deactivate( );

                    TaskReadyForFragmentDisplay = false;
                }

                public override TaskFragment StartingFragment()
                {
                    return MainPage;
                }
            }
        }
    }
}

