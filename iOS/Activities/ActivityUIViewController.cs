using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;

namespace iOS
{
	public partial class ActivityUIViewController : UIViewController
	{
        /// <summary>
        /// The owning activity
        /// </summary>
        /// <value>The activity.</value>
        public Activity Activity { get; set; }

		public ActivityUIViewController (IntPtr handle) : base (handle)
		{
		}

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);

            Activity.TouchesEnded( this, touches, evt );
        }
	}
}
