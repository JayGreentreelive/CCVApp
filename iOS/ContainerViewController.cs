using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;

namespace iOS
{
	partial class ContainerViewController : UIViewController
	{
        Activity CurrentActivity { get; set; }

		public ContainerViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            this.NavigationItem.SetLeftBarButtonItem(
                new UIBarButtonItem(UIBarButtonSystemItem.Action, (sender,args) => 
                {
                    (ParentViewController as MainUINavigationController).CheeseburgerTouchUp( );
                }), true);
        }

        public void PresentActivity( Activity activity )
        {
            if( CurrentActivity != null )
            {
                CurrentActivity.OnResignActive( );
            }

            CurrentActivity = activity;
            CurrentActivity.Present( this, new System.Drawing.PointF( View.Layer.Position.X, View.Layer.Position.Y + NavigationController.NavigationBar.Frame.Height ) );
        }

        public void OnResignActive()
        {
            if( CurrentActivity != null )
            {
                CurrentActivity.OnResignActive( );
            }
        }

        public void DidEnterBackground( )
        {
            if( CurrentActivity != null )
            {
                CurrentActivity.DidEnterBackground( );
            }
        }

        public void WillTerminate( )
        {
            if( CurrentActivity != null )
            {
                CurrentActivity.WillTerminate( );
            }
        }
	}
}
