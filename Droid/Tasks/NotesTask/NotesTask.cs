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
                    MainPage = new NotesFragment( this );
                }

                public override Android.App.Fragment StartingFragment()
                {
                    return MainPage;
                }
            }
        }
    }
}

