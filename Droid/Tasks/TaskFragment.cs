
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
            public Task ParentTask { get; set; }

            public virtual bool OnTouch( View v, MotionEvent e )
            {
                switch( e.Action )
                {
                    case MotionEventActions.Up:
                    {
                        TouchUpInside( v );
                        break;
                    }
                }
                return true;
            }

            protected virtual void TouchUpInside( View v )
            {
            }
        }
    }
}
