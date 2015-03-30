using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using CoreGraphics;

namespace iOS
{
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
