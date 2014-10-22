using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using CCVApp.Shared.Network;
using System.Drawing;

namespace iOS
{
	partial class NewsMainUIViewController : TaskUIViewController
	{
        public class TableSource : UITableViewSource 
        {
            NewsMainUIViewController Parent { get; set; }

            List<RockNews> News { get; set; }
            List<UIImageView> NewsImage { get; set; }

            string cellIdentifier = "TableCell";

            public TableSource (NewsMainUIViewController parent, List<RockNews> newsList, List<UIImageView> newsImage )
            {
                Parent = parent;

                News = newsList;

                NewsImage = newsImage;
            }

            public override int RowsInSection (UITableView tableview, int section)
            {
                return News.Count;
            }

            public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            {
                tableView.DeselectRow( indexPath, true );

                // notify our parent
                Parent.RowClicked( indexPath.Row );
            }

            public override float GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                // check the height of the image and let that be the height for this row
                return NewsImage[ indexPath.Row ].Bounds.Height;
            }

            public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
            {
                UITableViewCell cell = tableView.DequeueReusableCell (cellIdentifier);
                // if there are no cells to reuse, create a new one
                if (cell == null)
                {
                    cell = new UITableViewCell (UITableViewCellStyle.Default, cellIdentifier);

                    // configure the cell colors
                    cell.BackgroundColor = Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Shared.Config.News.Table_CellBackgroundColor );
                    cell.TextLabel.TextColor = Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Shared.Config.News.Table_CellTextColor );
                    cell.SelectionStyle = UITableViewCellSelectionStyle.Default;
                }

                // set the image for the cell
                UIImageView imageView = NewsImage[ indexPath.Row ];
                cell.ContentView.AddSubview( imageView );
                cell.Bounds = imageView.Bounds;

                return cell;
            }
        }

        public List<RockNews> News { get; set; }
        List<UIImageView> NewsImage { get; set; }

		public NewsMainUIViewController (IntPtr handle) : base (handle)
		{
            News = new List<RockNews>( );
            NewsImage = new List<UIImageView>( );
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // load the news images for each item
            foreach( RockNews news in News )
            {
                UIImageView imageView = new UIImageView( new UIImage( NSBundle.MainBundle.BundlePath + "/" + news.ImageName ) );

                NewsImage.Add( imageView );
            }

            // populate our table
            TableSource source = new TableSource( this, News, NewsImage );
            NewsTableView.Source = source;

            NewsTableView.BackgroundColor = Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Shared.Config.News.Table_BackgroundColor );
            NewsTableView.SeparatorColor = Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Shared.Config.News.Table_SeperatorBackgroundColor );
            NewsTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // adjust the table height for our navbar.
            // We MUST do it here, and we also have to set ContentType to Top, as opposed to ScaleToFill, on the view itself,
            // or our changes will be overwritten
            NewsTableView.Frame = new RectangleF( 0, NavigationController.NavigationBar.Frame.Height, View.Bounds.Width, View.Bounds.Height - NavigationController.NavigationBar.Frame.Height );
        }

        public void RowClicked( int row )
        {
            NewsDetailsUIViewController viewController = Storyboard.InstantiateViewController( "NewsDetailsUIViewController" ) as NewsDetailsUIViewController;
            viewController.NewsItem = News[ row ];

            Task.PerformSegue( this, viewController );
        }
	}
}
