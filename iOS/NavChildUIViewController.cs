using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;

namespace iOS
{
	partial class NavChildUIViewController : UIViewController
	{
        public NavChildUIViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            if( NavigationController != null )
            {
                // create the cheese burger button
                UIButton cheeseburgerButton = ( (MainUINavigationController)NavigationController).CreateCheeseburgerButton( );
                cheeseburgerButton.TouchUpInside += (object sender, EventArgs e) => 
                    {
                        (ParentViewController as MainUINavigationController).CheeseburgerTouchUp( );
                    };

                // create the back button
                UIButton backButton = ( (MainUINavigationController)NavigationController).CreateBackButton( );
                backButton.TouchUpInside += (object sender, EventArgs e) => 
                    {
                        NavigationController.PopViewControllerAnimated( true );
                    };

                NavigationItem.SetLeftBarButtonItems( new UIBarButtonItem[] 
                    { 
                        new UIBarButtonItem( cheeseburgerButton ),

                        new UIBarButtonItem( backButton )
                    }, 

                    false);
            }
        }
	}
}
