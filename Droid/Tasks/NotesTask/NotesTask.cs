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
                    MainPage.ParentTask = this;
                }

                public override TaskFragment StartingFragment()
                {
                    return MainPage;
                }
            }
        }
    }
}

