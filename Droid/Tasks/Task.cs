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

            /// <summary>
            /// True when the task is ready for the fragment to display.
            /// This could be false if, say, the task wants the fragment to wait
            /// for the springboard to close.
            /// </summary>
            /// <value><c>true</c> if task ready; otherwise, <c>false</c>.</value>
            public bool TaskReadyForFragmentDisplay { get; protected set; }

            public Task( NavbarFragment navFragment )
            {
                NavbarFragment = navFragment;
            }

            public virtual TaskFragment StartingFragment( )
            {
                return null;
            }

            public virtual void Activate( bool forResume )
            {
                /*FragmentManager fm = NavbarFragment.FragmentManager;
                int count = fm.BackStackEntryCount;
                for( int i = 0; i < count; i++ ) 
                {
                    fm.PopBackStackImmediate( );
                }*/

                //FragmentManager.popBackStack(String name,
                  //  FragmentManager.POP_BACK_STACK_INCLUSIVE)


                NavbarFragment.FragmentManager.PopBackStack( null, PopBackStackFlags.Inclusive );

                // present our starting fragment, and don't allow back navigation
                PresentFragment( StartingFragment( ), false );
            }

            public virtual void Deactivate( bool forPause )
            {
                // nothing we need to do for deactivation
            }

            protected void PresentFragment( TaskFragment fragment, bool allowBack )
            {
                // get the fragment manager
                var ft = NavbarFragment.FragmentManager.BeginTransaction();

                // set this as the active visible fragment in the task frame.
                string typestr = fragment.GetType().ToString();
                ft.Replace(Resource.Id.activetask, fragment, typestr );

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

            public virtual bool CanContainerPan()
            {
                return true;
            }

            public virtual void SpringboardDidAnimate( bool springboardRevealed )
            {
            }

            public virtual void OnClick( Fragment source, int buttonId, object context = null )
            {
            }
        }
    }
}
