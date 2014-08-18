using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace iOS
{
    public class Activity
    {
        protected UIViewController ParentViewController { get; set; }
        protected UIStoryboard Storyboard { get; set; }

        public Activity( string storyboardName )
        {
            // activities don't HAVE to have a storyboard
            if( false == string.IsNullOrEmpty( storyboardName ) )
            {
                Storyboard = UIStoryboard.FromName( storyboardName, null );
            }
        }

        public virtual void MakeActive( UIViewController parentViewController, PointF position )
        {
            ParentViewController = parentViewController;
        }

        public virtual void OnResignActive( )
        {
            // always clear our parent view controller when resigning
            ParentViewController = null;
        }

        public virtual void DidEnterBackground( )
        {
        }

        public virtual void WillTerminate( )
        {
        }
    }
}

