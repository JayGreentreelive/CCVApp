using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using CCVApp.Shared.Network;

namespace iOS
{
	partial class NewsMainUIViewController : TaskUIViewController
	{
        public class TableSource : UITableViewSource 
        {
            NewsMainUIViewController Parent { get; set; }

            public List<RockNews> News { get; set; }
            string cellIdentifier = "TableCell";

            public TableSource (NewsMainUIViewController parent, List<RockNews> newsList)
            {
                Parent = parent;

                News = newsList;
            }

            public override int RowsInSection (UITableView tableview, int section)
            {
                return News.Count;
            }

            public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            {
                //new UIAlertView("Row Selected", tableItems[indexPath.Row], null, "OK", null).Show();
                tableView.DeselectRow(indexPath, true); // normal iOS behaviour is to remove the blue highlight

                // notify our parent
                Parent.RowClicked( indexPath.Row );
            }

            public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
            {
                UITableViewCell cell = tableView.DequeueReusableCell (cellIdentifier);
                // if there are no cells to reuse, create a new one
                if (cell == null)
                {
                    cell = new UITableViewCell (UITableViewCellStyle.Default, cellIdentifier);
                }

                cell.TextLabel.Text = News[indexPath.Row].Title;
                return cell;
            }
        }

        public List<RockNews> News { get; set; }

		public NewsMainUIViewController (IntPtr handle) : base (handle)
		{
            News = new List<RockNews>( );
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // populate our table
            TableSource source = new TableSource( this, News );
            NewsTableView.Source = source;
        }

        public void RowClicked( int row )
        {
            NewsDetailsUIViewController viewController = Storyboard.InstantiateViewController( "NewsDetailsUIViewController" ) as NewsDetailsUIViewController;
            viewController.NewsItem = News[ row ];

            Task.PerformSegue( this, viewController );
        }
	}
}
