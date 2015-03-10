using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using CCVApp.Shared.Network;
using CoreGraphics;
using CCVApp.Shared.Config;
using Rock.Mobile.PlatformUI;
using CCVApp.Shared;
using System.IO;

namespace iOS
{
	partial class NewsMainUIViewController : TaskUIViewController
	{
        public class TableSource : UITableViewSource 
        {
            NewsMainUIViewController Parent { get; set; }

            List<NewsEntry> News { get; set; }
            UIImage ImagePlaceholder { get; set; }

            string cellIdentifier = "TableCell";

            nfloat PendingCellHeight { get; set; }

            public TableSource (NewsMainUIViewController parent, List<NewsEntry> newsList, UIImage imagePlaceholder)
            {
                Parent = parent;

                News = newsList;

                ImagePlaceholder = imagePlaceholder;
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
                if ( News[ indexPath.Row ].Image != null )
                {
                    cell.ContentView.Layer.Contents = News[ indexPath.Row ].Image.CGImage;
                }
                else
                {
                    cell.ContentView.Layer.Contents = ImagePlaceholder.CGImage;
                }

                // scale down the image to the width of the device
                float imageWidth = cell.ContentView.Layer.Contents.Width;
                float imageHeight = cell.ContentView.Layer.Contents.Height;

                float aspectRatio = (float) (imageHeight / imageWidth);
                cell.Bounds = new CGRect( 0, 0, tableView.Bounds.Width, tableView.Bounds.Width * aspectRatio );

                PendingCellHeight = cell.Bounds.Height;
                return cell;
            }
        }

        public class NewsEntry
        {
            public RockNews News { get; set; }
            public UIImage Image { get; set; }
        }
        List<NewsEntry> News { get; set; }

        /// <summary>
        /// Store the source news so that if they pick a news item to view,
        /// when they return, it shows the same news instead of potentially newly downloaded news.
        /// </summary>
        /// <value>The source rock news.</value>
        public List<RockNews> SourceRockNews { get; set; }

        bool IsVisible { get; set; }
        UIImage ImagePlaceholder { get; set; }

		public NewsMainUIViewController (IntPtr handle) : base (handle)
		{
            News = new List<NewsEntry>( );

            string imagePath = NSBundle.MainBundle.BundlePath + "/" + GeneralConfig.NewsMainPlaceholder;
            ImagePlaceholder = new UIImage( imagePath );
		}

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            IsVisible = true;

            ReloadNews( );

            // populate our table
            TableSource source = new TableSource( this, News, ImagePlaceholder );
            NewsTableView.Source = source;

            NewsTableView.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );
            NewsTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            IsVisible = false;
        }

        public void ReloadNews( )
        {
            News.Clear( );

            foreach ( RockNews rockEntry in SourceRockNews )
            {
                NewsEntry newsEntry = new NewsEntry();
                News.Add( newsEntry );

                newsEntry.News = rockEntry;

                // since we're in setup, go ahead and try loading any images we want.
                // If thye don't exist, we'll use a placeholder until DownloadImages is called by our parent task.
                TryLoadCachedImage( newsEntry );
            }

            NewsTableView.ReloadData( );
        }

        public void DownloadImages( )
        {
            // this will be called by our parent task when we're clear to download images.
            foreach ( NewsEntry newsItem in News )
            {
                // if no image is set, we couldn't load one, so download it.
                if ( newsItem.Image == null )
                {
                    FileCache.Instance.DownloadImageToCache( newsItem.News.ImageURL, newsItem.News.ImageName, 
                        delegate
                        {
                            SeriesImageDownloaded( );
                        } );
                }
            }

        }

        bool TryLoadCachedImage( NewsEntry entry )
        {
            bool needImage = false;

            // check the billboard
            if ( entry.Image == null )
            {
                MemoryStream imageStream = (MemoryStream)FileCache.Instance.LoadFile( entry.News.ImageName );
                if ( imageStream != null )
                {
                    try
                    {
                        NSData imageData = NSData.FromStream( imageStream );
                        entry.Image = new UIImage( imageData, UIScreen.MainScreen.Scale );
                    }
                    catch( Exception )
                    {
                        FileCache.Instance.RemoveFile( entry.News.ImageName );
                        Console.WriteLine( "Image {0} is corrupt. Removing.", entry.News.ImageName );
                    }
                    imageStream.Dispose( );
                }
                else
                {
                    needImage = true;
                }
            }

            return needImage;
        }

        void SeriesImageDownloaded( )
        {
            if ( IsVisible == true )
            {
                InvokeOnMainThread( delegate
                    {
                        // using only the cache, try to load any image that isn't loaded
                        foreach ( NewsEntry entry in News )
                        {
                            TryLoadCachedImage( entry );
                        }

                        NewsTableView.ReloadData( );
                    } );
            }
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
            viewController.NewsItem = News[ row ].News;

            Task.PerformSegue( this, viewController );
        }
	}
}
