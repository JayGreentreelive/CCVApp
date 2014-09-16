using System;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace iOS
{
    public class Task
    {
        protected UINavigationController ParentViewController { get; set; }
        protected NavToolbar NavToolbar { get; set; }
        protected UIStoryboard Storyboard { get; set; }

        public Task( string storyboardName )
        {
            // activities don't HAVE to have a storyboard
            if( false == string.IsNullOrEmpty( storyboardName ) )
            {
                Storyboard = UIStoryboard.FromName( storyboardName, null );
            }
        }

        /// <summary>
        /// The root function to call when changing view controllers within a task. If changing in code,
        /// directly call this from the view controller. If using a storyboard, hook the segue to TaskTransition
        /// in ContainerViewController.cs, which will cause this function to be called.
        /// </summary>
        /// <param name="sourceViewController">Source view controller.</param>
        /// <param name="destinationViewController">Destination view controller.</param>
        public void PerformSegue( UIViewController sourceViewController, UIViewController destinationViewController )
        {
            // take this opportunity to give the presenting view controller a pointer to the active task
            // so it can receive callbacks.
            TaskUIViewController viewController = destinationViewController as TaskUIViewController;
            if( viewController == null )
            {
                throw new InvalidCastException( "View Controllers used by Activities must be of type TaskUIViewController" );
            }

            viewController.Task = this;
            ParentViewController.PushViewController( destinationViewController, true );
        }

        /// <summary>
        /// Called when the task is going to be the forefront task.
        /// Allows it to do any work necessary before being interacted with.
        /// Ex: Notes might disable the phone's sleep
        /// This is NOT called when the application comes into the foreground.
        /// </summary>
        /// <param name="parentViewController">Parent view controller.</param>
        public virtual void MakeActive( UINavigationController parentViewController, NavToolbar navToolbar )
        {
            ParentViewController = parentViewController;
            NavToolbar = navToolbar;
        }

        /// <summary>
        /// Called when the task is going away so another task can be interacted with.
        /// Allows it to undo any work done in MakeActive.
        /// Ex: Notes might RE-enable the phone's sleep.
        /// This is NOT called when the application goes into the background.
        /// </summary>
        public virtual void MakeInActive( )
        {
            // always clear our parent view controller when going inactive
            ParentViewController = null;
            NavToolbar = null;
        }

        /// <summary>
        /// Called when a new view controller is shown by the parent navigation controller.
        /// This is useful so the task can evaluate what viewcontroller was just shown
        /// and update itself or the toolbar accordingly.
        /// </summary>
        /// <param name="viewController">View controller.</param>
        public virtual void WillShowViewController( UIViewController viewController )
        {
        }

        /// <summary>
        /// Called by the active view controller when touches ended. Allows the task to perform any
        /// necessary actions, like revealing the nav bar.
        /// </summary>
        /// <param name="TaskUIViewController">Task user interface view controller.</param>
        /// <param name="touches">Touches.</param>
        /// <param name="evt">Evt.</param>
        public virtual void TouchesEnded( TaskUIViewController taskUIViewController, NSSet touches, UIEvent evt )
        {
        }

        /// <summary>
        /// Called by the active view controller when a user scrolls.
        /// This allows the task to perform any scroll dependent actions, like revealing the nav bar.
        /// </summary>
        /// <param name"scrollDelta">The change in scroll since the last call.</param>
        public virtual void ViewDidScroll( float scrollDelta )
        {
        }

        /// <summary>
        /// Called when the application will go into the background.
        /// This is NOT called when the task goes into the background.
        /// </summary>
        public virtual void AppOnResignActive( )
        {
        }

        public virtual void AppDidEnterBackground( )
        {
        }

        public virtual void AppWillTerminate( )
        {
        }
    }
}
