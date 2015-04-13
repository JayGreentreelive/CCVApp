using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using CoreGraphics;

namespace iOS
{
    /// <summary>
    /// Helper class for UIViewControllers that simply want the nav bar to reveal
    /// based on scrolling.
    /// </summary>
    public class NavBarRevealHelperDelegate : UITableViewDelegate
    {
        public NavToolbar NavToolbar { get; set; }

        public NavBarRevealHelperDelegate( NavToolbar toolbar )
        {
            NavToolbar = toolbar;
        }

        CGPoint LastPos { get; set; }
        double LastTime { get; set; }

        public override void DraggingStarted( UIScrollView scrollView )
        {
            LastTime = NSDate.Now.SecondsSinceReferenceDate;
            LastPos = scrollView.ContentOffset;
        }

        public override void Scrolled( UIScrollView scrollView )
        {
            double timeLapsed = NSDate.Now.SecondsSinceReferenceDate - LastTime;

            if( timeLapsed > .10f )
            {
                // notify our parent
                // guard against this callback being received after we've switched away from this task.
                if ( NavToolbar != null )
                {
                    nfloat scrollPerc = scrollView.ContentOffset.Y / scrollView.ContentSize.Height;
                    if ( scrollPerc < .10f )
                    {
                        NavToolbar.Reveal( true );
                    }
                    else
                    {
                        NavToolbar.Reveal( false );
                    }
                }
            }
        }
    }

	public partial class TaskUIViewController : UIViewController
	{
        /// <summary>
        /// The owning task
        /// </summary>
        /// <value>The task.</value>
        public Task Task { get; set; }

		public TaskUIViewController (IntPtr handle) : base (handle)
		{
		}

        public TaskUIViewController () : base ()
        {
        }

        public virtual void LayoutChanging( )
        {
        }

        public virtual void LayoutChanged( )
        {
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            Task.TouchesEnded( this, touches, evt );
        }

        public virtual void OnActivated( )
        {
        }

        public virtual void WillEnterForeground( )
        {
        }

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
