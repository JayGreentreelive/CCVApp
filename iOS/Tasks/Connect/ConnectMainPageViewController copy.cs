using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using System.Collections.Generic;

namespace iOS
{
	partial class ConnectMainPageViewController : TaskUIViewController
	{
        class Link
        {
            public UIButton Button { get; set; }
            public string Url { get; set; }
        }
        List<Link> Links { get; set; }

		public ConnectMainPageViewController (IntPtr handle) : base (handle)
		{
            Links = new List<Link>();
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            float nextYPos = GroupFinderButton.Frame.Bottom + 33;

            // parse the config and see how many additional links we need.
            for ( int i = 0; i < CCVApp.Shared.Config.Connect.WebViews.Length; i += 2 )
            {
                Link link = new Link();
                Links.Add( link );
                link.Url = CCVApp.Shared.Config.Connect.WebViews[ i + 1 ];
                link.Button = UIButton.FromType( UIButtonType.System );
                link.Button.SetTitle( CCVApp.Shared.Config.Connect.WebViews[ i ], UIControlState.Normal );
                link.Button.TouchUpInside += (object sender, EventArgs e ) =>
                {
                    ConnectWebViewController viewController = Storyboard.InstantiateViewController( "ConnectWebViewController" ) as ConnectWebViewController;
                    viewController.DisplayUrl = link.Url;
                    Task.PerformSegue( this, viewController );
                };

                link.Button.SizeToFit( );
                link.Button.Frame = new System.Drawing.RectangleF( (View.Bounds.Width - link.Button.Bounds.Width) / 2, nextYPos, link.Button.Bounds.Width, link.Button.Bounds.Height );
                View.AddSubview( link.Button );

                nextYPos = link.Button.Frame.Bottom + 33;
            }

            GroupFinderButton.TouchUpInside += (object sender, EventArgs e ) =>
            {
                TaskUIViewController viewController = Storyboard.InstantiateViewController( "GroupFinderViewController" ) as TaskUIViewController;
                Task.PerformSegue( this, viewController );
            };
        }
	}
}
