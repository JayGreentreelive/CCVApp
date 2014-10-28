
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace Droid
{
    namespace Tasks
    {
        /// <summary>
        /// A task fragment is simply a fragment for a page of a task.
        /// This provides a common interface that allows us
        /// to work with the fragments of tasks in an abstract manner.
        /// </summary>
        public class TaskFragment : Fragment, View.IOnTouchListener
        {
            /// <summary>
            /// Manages forwarding gestures to the carousel
            /// </summary>
            public class TaskFragmentGestureDetector : GestureDetector.SimpleOnGestureListener
            {
                public TaskFragment Parent { get; set; }

                public TaskFragmentGestureDetector( TaskFragment parent )
                {
                    Parent = parent;
                }

                public override bool OnDown(MotionEvent e)
                {
                    // Make the TaskFragment handle this
                    Parent.OnDownGesture( e );
                    return true;
                }

                public override bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
                {
                    Parent.OnFlingGesture( e1, e2, velocityX, velocityY );
                    return base.OnFling(e1, e2, velocityX, velocityY);
                }

                public override bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
                {
                    Parent.OnScrollGesture( e1, e2, distanceX, distanceY );
                    return base.OnScroll(e1, e2, distanceX, distanceY);
                }

                public override bool OnDoubleTap(MotionEvent e)
                {
                    Parent.OnDoubleTap( e );
                    return base.OnDoubleTap( e );
                }
            }

            public Task ParentTask { get; set; }

            GestureDetector GestureDetector { get; set; }

            public TaskFragment( ) : base( )
            {
                GestureDetector = new GestureDetector( Rock.Mobile.PlatformCommon.Droid.Context, new TaskFragmentGestureDetector( this ) );
            }

            public virtual bool OnDownGesture( MotionEvent e )
            {
                ParentTask.NavbarFragment.OnDown( e );
                return false;
            }

            public virtual bool OnDoubleTap(MotionEvent e)
            {
                return false;
            }

            public virtual bool OnFlingGesture( MotionEvent e1, MotionEvent e2, float velocityX, float velocityY )
            {
                // let the navbar know we're flicking
                ParentTask.NavbarFragment.OnFlick( e1, e2, velocityX, velocityY );
                return false;
            }

            public virtual bool OnScrollGesture( MotionEvent e1, MotionEvent e2, float distanceX, float distanceY )
            {
                // let the navbar know we're scrolling
                ParentTask.NavbarFragment.OnScroll( e1, e2, distanceX, distanceY );
                return false;
            }

            /// <summary>
            /// Called by the OnTouchListener. This is the only method OnTouch calls.
            /// If you override this, you need to acknowledge it returning true and
            /// return true as well
            /// </summary>
            /// <param name="v">V.</param>
            /// <param name="e">E.</param>
            public virtual bool OnTouch( View v, MotionEvent e )
            {
                if ( GestureDetector.OnTouchEvent( e ) == true )
                {
                    return true;
                }
                else
                {
                    switch ( e.Action )
                    {
                        case MotionEventActions.Up:
                        {
                            ParentTask.NavbarFragment.OnUp( e );
                            break;
                        }
                    }

                    return false;
                }
            }
        }
    }
}
