using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using System.Drawing;

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

            this.NavigationItem.SetLeftBarButtonItem
            (
                new UIBarButtonItem(UIBarButtonSystemItem.Action, (sender,args) => 
                {
                    (ParentViewController as MainUINavigationController).CheeseburgerTouchUp( );
                }), true
            );
        }

        public void ActivateActivity( Activity activity )
        {
            if( CurrentActivity != null )
            {
                CurrentActivity.OnResignActive( );
            }

            CurrentActivity = activity;
            CurrentActivity.MakeActive( this );
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
