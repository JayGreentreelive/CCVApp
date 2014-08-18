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

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            NavigationItem.SetLeftBarButtonItems( new UIBarButtonItem[] 
                { 
                    new UIBarButtonItem(UIBarButtonSystemItem.Rewind, (sender,args) => 
                        {
                            NavigationController.PopViewControllerAnimated( true );
                        }), 

                    new UIBarButtonItem(UIBarButtonSystemItem.Action, (sender,args) => 
                        {
                            (ParentViewController as MainUINavigationController).CheeseburgerTouchUp( );
                        }) 
                }, 

                true);

            NavigationItem.Title = "CCV App Logo";
        }
	}
}
