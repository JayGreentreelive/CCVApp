using System;

namespace Droid
{
    namespace Tasks
    {
        namespace Notes
        {
            public class NotesTask : Task
            {
                TaskFragment ActiveFragment { get; set; }

                NotesFragment NotesPage { get; set; }
                NotesPrimaryFragment MainPage { get; set; }
                NotesDetailsFragment DetailsPage { get; set; }

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

                    // for now, let the note name be the previous saturday
                    /*DateTime time = DateTime.UtcNow;

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
                    #endif*/
                    //

                    MainPage.ParentTask = this;
                    DetailsPage.ParentTask = this;
                    NotesPage.ParentTask = this;
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

                public override void Activate( bool forResume )
                {
                    base.Activate( forResume );

                    // we'll always start at the main page
                    ActiveFragment = MainPage;

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
                    NotesFragment notesFragment = ActiveFragment as NotesFragment;
                    if ( notesFragment != null )
                    {
                        return notesFragment.MovingUserNote ? false : true;
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

                public override void OnClick(Android.App.Fragment source, int buttonId)
                {
                    // only handle input if the springboard is closed
                    if ( NavbarFragment.ShouldTaskAllowInput( ) )
                    {
                        // decide what to do.
                        if ( source == MainPage )
                        {
                            DetailsPage.Messages = MainPage.Series[ buttonId ].Messages;
                            PresentFragment( DetailsPage, true );

                            ActiveFragment = DetailsPage;
                        }
                        else if ( source == DetailsPage )
                        {
                            NotesPage.NoteName = DetailsPage.Messages[ buttonId ].NoteUrl;
                            NotesPage.NotePresentableName = DetailsPage.Messages[ buttonId ].Name;

                            PresentFragment( NotesPage, true );
                            ActiveFragment = NotesPage;
                        }
                    }
                }
            }
        }
    }
}

