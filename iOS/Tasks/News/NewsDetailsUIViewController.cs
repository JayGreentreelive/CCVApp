using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using CCVApp.Shared.Network;
using CoreGraphics;
using CCVApp.Shared.Config;
using Rock.Mobile.PlatformUI;
using CCVApp.Shared.Strings;
using System.IO;
using CCVApp.Shared;

namespace iOS
{
	partial class NewsDetailsUIViewController : TaskUIViewController
	{
        public RockNews NewsItem { get; set; }

        bool IsVisible { get; set; }

		public NewsDetailsUIViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );

            // populate the details view with this news item.
            //NewsDescription.Layer.AnchorPoint = CGPoint.Empty;
            NewsDescription.Text = NewsItem.Description;
            NewsDescription.BackgroundColor = UIColor.Clear;
            NewsDescription.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor );
            NewsDescription.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Small_Font_Light, ControlStylingConfig.Small_FontSize );
            NewsDescription.TextContainerInset = UIEdgeInsets.Zero;
            NewsDescription.TextContainer.LineFragmentPadding = 0;

            // we should always assume images are in cache. If they aren't, show a placeholder.
            // It is not our job to download them.
            MemoryStream imageStream = (MemoryStream)FileCache.Instance.LoadFile( NewsItem.HeaderImageName );
            if ( imageStream != null )
            {
                try
                {
                    NSData imageData = NSData.FromStream( imageStream );
                    ImageBanner.Image = new UIImage( imageData );
                }
                catch( Exception )
                {
                    FileCache.Instance.RemoveFile( NewsItem.HeaderImageName );
                    Console.WriteLine( "Image {0} is corrupt. Removing.", NewsItem.HeaderImageName );
                }
                imageStream.Dispose( );
            }
            else
            {
                // otherwise use a placeholder and request the actual image
                ImageBanner.Image = new UIImage( NSBundle.MainBundle.BundlePath + "/" + GeneralConfig.NewsDetailsPlaceholder );

                FileCache.Instance.DownloadFileToCache( NewsItem.HeaderImageURL, NewsItem.HeaderImageName, delegate
                    {
                        NewsHeaderDownloaded( );
                    } );
            }

            // scale the image down to fit the contents of the window, but allow cropping.
            ImageBanner.BackgroundColor = UIColor.Green;
            ImageBanner.ContentMode = UIViewContentMode.ScaleAspectFill;

            LearnMoreButton.TouchUpInside += (object sender, EventArgs e) => 
                {
                    NewsWebViewController viewController = Storyboard.InstantiateViewController( "NewsWebViewController" ) as NewsWebViewController;
                    viewController.DisplayUrl = NewsItem.ReferenceURL;

                    Task.PerformSegue( this, viewController );
                };

            // if there's no URL associated with this news item, hide the learn more button.
            if ( string.IsNullOrEmpty( NewsItem.ReferenceURL ) == true )
            {
                LearnMoreButton.Hidden = true;
            }

            ControlStyling.StyleButton( LearnMoreButton, NewsStrings.LearnMore, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );

            ControlStyling.StyleUILabel( NewsTitle, ControlStylingConfig.Large_Font_Bold, ControlStylingConfig.Large_FontSize );
            NewsTitle.Text = NewsItem.Title;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            IsVisible = true;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            IsVisible = false;
        }

        void NewsHeaderDownloaded( )
        {
            // if they're still viewing this article
            if ( IsVisible == true )
            {
                Rock.Mobile.Threading.Util.PerformOnUIThread( delegate
                    {
                        MemoryStream imageStream = (System.IO.MemoryStream)FileCache.Instance.LoadFile( NewsItem.HeaderImageName );
                        if ( imageStream != null )
                        {
                            try
                            {
                                NSData imageData = NSData.FromStream( imageStream );
                                ImageBanner.Image = new UIImage( imageData, UIScreen.MainScreen.Scale );
                            }
                            catch( Exception )
                            {
                                FileCache.Instance.RemoveFile( NewsItem.HeaderImageName );
                                Console.WriteLine( "Image {0} is corrupt. Removing.", NewsItem.HeaderImageName );
                            }

                            imageStream.Dispose( );
                        }
                    });

            }
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // if the description needs to scroll, enable user interaction (which disables the nav toolbar)
            if ( NewsDescription.ContentSize.Height > NewsDescription.Frame.Height )
            {
                NewsDescription.UserInteractionEnabled = true;
            }
            else
            {
                NewsDescription.UserInteractionEnabled = false;
            }

            //nfloat imageBase = ( ImageBanner.Frame.Top + ImageBanner.Image.Size.Height );

            // adjust the news title to have padding on the left and right.
            NewsTitle.Layer.AnchorPoint = CGPoint.Empty;
            NewsTitle.SizeToFit( );
            NewsTitle.Frame = new CGRect( NewsDescription.Frame.Left, ImageBanner.Frame.Bottom + (40 - NewsTitle.Frame.Height) / 2, View.Bounds.Width - 30, NewsTitle.Bounds.Height );

            NewsDescription.Frame = new CGRect( NewsDescription.Frame.Left, ImageBanner.Frame.Bottom + 40, NewsDescription.Frame.Width, NewsDescription.Frame.Height );
        }
	}
}
