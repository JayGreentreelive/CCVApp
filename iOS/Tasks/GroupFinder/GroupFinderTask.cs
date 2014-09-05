using System;
using MonoTouch.UIKit;
using System.Drawing;

namespace iOS
{
    public class GroupFinderTask : Task
    {
        public GroupFinderTask( string storyboardName ) : base( storyboardName )
        {
        }

        public override void MakeActive( UINavigationController parentViewController, NavToolbar navToolbar )
        {
            base.MakeActive( parentViewController, navToolbar );
        }

        public override void MakeInActive( )
        {
            base.MakeInActive( );
        }

        public override void AppOnResignActive( )
        {
            base.AppOnResignActive( );
        }
    }
}

