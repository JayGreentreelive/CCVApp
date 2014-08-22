using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;

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

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            Task.TouchesEnded( this, touches, evt );
        }
	}
}
