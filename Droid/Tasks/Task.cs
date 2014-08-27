using System;
using Android.App;
using Android.Widget;

namespace Droid
{
    namespace Tasks
    {
        /// <summary>
        /// A task represents a "section" of the app, like the news, group finder,
        /// notes, etc. It contains all of the pages that make up that particular section.
        /// </summary>
        public class Task
        {
            /// <summary>
            /// Reference to the parent navbar fragment
            /// </summary>
            /// <value>The navbar fragment.</value>
            public NavbarFragment NavbarFragment { get; set; }

            public Task( NavbarFragment navFragment )
            {
                NavbarFragment = navFragment;
            }

            public virtual TaskFragment StartingFragment( )
            {
                return null;
            }

            public virtual void Activate( )
            {
                // present our starting fragment, and don't allow back navigation
                PresentFragment( StartingFragment( ), false );
            }

            public virtual void Deactivate( )
            {
                // nothing we need to do for deactivation
            }

            protected void PresentFragment( TaskFragment fragment, bool allowBack )
            {
                // get the fragment manager
                var ft = NavbarFragment.FragmentManager.BeginTransaction();

                // set this as the active visible fragment in the task frame.
                ft.Replace(Resource.Id.activetask, fragment );

                // do a nice crossfade
                ft.SetTransition(FragmentTransit.FragmentFade);

                // if back was requested, put it in our stack
                if( allowBack )
                {
                    ft.AddToBackStack( fragment.ToString() );
                }

                // do the transaction
                ft.Commit();
            }

            public virtual void OnClick( Fragment source, int buttonId )
            {
            }
        }
    }
}

