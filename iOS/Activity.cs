using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace iOS
{
    public class Activity
    {
        UIViewController ParentViewController { get; set; }
        protected UIStoryboard Storyboard { get; set; }

        public Activity( string storyboardName )
        {
            // activities don't HAVE to have a storyboard
            if( false == string.IsNullOrEmpty( storyboardName ) )
            {
                Storyboard = UIStoryboard.FromName( storyboardName, null );
            }
        }

        public virtual void Present( UIViewController parentViewController, PointF position )
        {
        }

        public virtual void OnResignActive( )
        {
        }

        public virtual void DidEnterBackground( )
        {
        }

        public virtual void WillTerminate( )
        {
        }
    }
}

