using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using CCVApp.Shared.Network;
using CoreGraphics;
using CCVApp.Shared.Config;
using Rock.Mobile.PlatformUI;

namespace iOS
{
	partial class NewsMainUIViewController : TaskUIViewController
	{
        public class TableSource : UITableViewSource 
        {
            NewsMainUIViewController Parent { get; set; }

            List<RockNews> News { get; set; }
            List<UIImage> NewsImage { get; set; }

            string cellIdentifier = "TableCell";

            nfloat PendingCellHeight { get; set; }

            public TableSource (NewsMainUIViewController parent, List<RockNews> newsList, List<UIImage> newsImage )
            {
                Parent = parent;

                News = newsList;

                NewsImage = newsImage;
            }

            public override nint RowsInSection (UITableView tableview, nint section)
            {
                return News.Count;
            }

            public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            {
                tableView.DeselectRow( indexPath, true );

                // notify our parent
                Parent.RowClicked( indexPath.Row );
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                // check the height of the image and let that be the height for this row
                if ( PendingCellHeight > 0 )
                {
                    return PendingCellHeight;
                }
                else
                {
                    return tableView.Bounds.Height;
                }
            }

            public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
            {
                UITableViewCell cell = tableView.DequeueReusableCell (cellIdentifier);
                // if there are no cells to reuse, create a new one
                if (cell == null)
                {
                    cell = new UITableViewCell (UITableViewCellStyle.Default, cellIdentifier);
                    cell.SelectionStyle = UITableViewCellSelectionStyle.Default;
                }

                // set the image for the cell
                cell.ContentView.Layer.Contents = NewsImage[ indexPath.Row ].CGImage;

                // scale down the image to the width of the device
                float aspectRatio = (float) (NewsImage[ indexPath.Row ].Size.Height / NewsImage[ indexPath.Row ].Size.Width);
                cell.Bounds = new CGRect( 0, 0, tableView.Bounds.Width, tableView.Bounds.Width * aspectRatio );

                PendingCellHeight = cell.Bounds.Height;
                return cell;
            }
        }

        public List<RockNews> News { get; set; }
        List<UIImage> NewsImage { get; set; }

		public NewsMainUIViewController (IntPtr handle) : base (handle)
		{
            News = new List<RockNews>( );
            NewsImage = new List<UIImage>( );
		}

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // load the news images for each item
            NewsImage.Clear( );

            foreach( RockNews news in News )
            {
                UIImage image = null;
                if ( string.IsNullOrEmpty( news.ImageName ) == false )
                {
                    image = new UIImage( NSBundle.MainBundle.BundlePath + "/" + news.ImageName );
                }
                else
                {
                    image = new UIImage( NSBundle.MainBundle.BundlePath + "/" + "podcastThumbnailPlaceholder.png" );
                }
                NewsImage.Add( image );
            }

            // populate our table
            TableSource source = new TableSource( this, News, NewsImage );
            NewsTableView.Source = source;

            NewsTableView.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );
            NewsTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // adjust the table height for our navbar.
            // We MUST do it here, and we also have to set ContentType to Top, as opposed to ScaleToFill, on the view itself,
            // or our changes will be overwritten
            NewsTableView.Frame = new CGRect( 0, 0, View.Bounds.Width, View.Bounds.Height );
        }

        public void RowClicked( int row )
        {
            NewsDetailsUIViewController viewController = Storyboard.InstantiateViewController( "NewsDetailsUIViewController" ) as NewsDetailsUIViewController;
            viewController.NewsItem = News[ row ];

            Task.PerformSegue( this, viewController );
        }
	}
}
