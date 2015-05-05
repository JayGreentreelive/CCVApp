using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using App.Shared.Network;
using CoreGraphics;
using App.Shared.Config;
using Rock.Mobile.PlatformUI;
using App.Shared;
using System.IO;
using App.Shared.PrivateConfig;

namespace iOS
{
	partial class NewsMainUIViewController : TaskUIViewController
	{
        public class PortraitTableSource : UITableViewSource 
        {
            NewsMainUIViewController Parent { get; set; }

            List<NewsEntry> News { get; set; }
            UIImage ImagePlaceholder { get; set; }

            string cellIdentifier = "TableCell";

            nfloat PendingCellHeight { get; set; }

            public PortraitTableSource (NewsMainUIViewController parent, List<NewsEntry> newsList, UIImage imagePlaceholder)
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
                    cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                    cell.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
                }
                cell.Bounds = new CGRect( cell.Bounds.X, cell.Bounds.Y, tableView.Bounds.Width, cell.Bounds.Height );

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
                nfloat imageWidth = cell.ContentView.Layer.Contents.Width;
                nfloat imageHeight = cell.ContentView.Layer.Contents.Height;

                nfloat aspectRatio = (float) (imageHeight / imageWidth);
                cell.Bounds = new CGRect( 0, 0, tableView.Bounds.Width, tableView.Bounds.Width * aspectRatio );

                PendingCellHeight = cell.Bounds.Height;
                return cell;
            }
        }

        public class LandscapeTableSource : UITableViewSource 
        {
            NewsMainUIViewController Parent { get; set; }

            List<NewsEntry> News { get; set; }
            UIImage ImagePlaceholder { get; set; }

            string primaryCellIdentifier = "PrimaryTableCell";
            string standardCellIdentifier = "StandardTableCell";

            nfloat PendingCellHeight { get; set; }

            public LandscapeTableSource (NewsMainUIViewController parent, List<NewsEntry> newsList, UIImage imagePlaceholder)
            {
                Parent = parent;

                News = newsList;

                ImagePlaceholder = imagePlaceholder;
            }

            public override nint RowsInSection (UITableView tableview, nint section)
            {
                // start with a top row
                int numItems = 1;

                // each row after will show two items
                double remainingItems = News.Count - 1;
                if ( remainingItems > 0 )
                {
                    // take the rows we'll need and round up
                    double rowsNeeded = remainingItems / 2.0f;

                    rowsNeeded = Math.Ceiling( rowsNeeded );

                    numItems += (int)rowsNeeded;
                }

                return numItems;
            }

            public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            {
                tableView.DeselectRow( indexPath, true );

                // notify our parent ONLY if the top row is clicked (as it's a single image)
                // the other images will manually call RowClicked
                if ( indexPath.Row == 0 )
                {    
                    RowClicked( indexPath.Row );
                }
            }

            public void RowClicked( int index )
            {
                Parent.RowClicked( index );
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
                if ( indexPath.Row == 0 )
                {
                    return GetPrimaryCell( tableView );
                }
                else
                {
                    return GetStandardCell( tableView, indexPath.Row - 1 );
                }
            }

            UITableViewCell GetPrimaryCell( UITableView tableView )
            {
                UITableViewCell cell = tableView.DequeueReusableCell (primaryCellIdentifier);
                // if there are no cells to reuse, create a new one
                if (cell == null)
                {
                    cell = new UITableViewCell (UITableViewCellStyle.Default, primaryCellIdentifier);
                    cell.SelectionStyle = UITableViewCellSelectionStyle.Default;
                    cell.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
                }
                cell.Bounds = new CGRect( cell.Bounds.X, cell.Bounds.Y, tableView.Bounds.Width, cell.Bounds.Height );

                // set the image for the cell
                if ( News[ 0 ].Image != null )
                {
                    cell.ContentView.Layer.Contents = News[ 0 ].Image.CGImage;
                }
                else
                {
                    cell.ContentView.Layer.Contents = ImagePlaceholder.CGImage;
                }

                // scale down the image to the width of the device
                nfloat imageWidth = cell.ContentView.Layer.Contents.Width;
                nfloat imageHeight = cell.ContentView.Layer.Contents.Height;

                nfloat aspectRatio = (float) (imageHeight / imageWidth);
                cell.Bounds = new CGRect( 0, 0, tableView.Bounds.Width, tableView.Bounds.Width * aspectRatio );

                PendingCellHeight = cell.Bounds.Height;
                return cell;
            }


            UITableViewCell GetStandardCell( UITableView tableView, int rowIndex )
            {
                StandardCell cell = tableView.DequeueReusableCell(standardCellIdentifier) as StandardCell;

                // convert the position to the appropriate image index.
                int leftImageIndex = 1 + ( rowIndex * 2 );

                // if there are no cells to reuse, create a new one
                if (cell == null)
                {
                    cell = new StandardCell (UITableViewCellStyle.Default, standardCellIdentifier);
                    cell.SelectionStyle = UITableViewCellSelectionStyle.Default;
                    cell.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
                    cell.ParentSource = this;
                }
                cell.Bounds = new CGRect( cell.Bounds.X, cell.Bounds.Y, tableView.Bounds.Width, cell.Bounds.Height );

                cell.LeftImageIndex = leftImageIndex;


                // first set the left item
                if ( leftImageIndex < News.Count )
                {
                    if ( News[ leftImageIndex ].Image != null )
                    {
                        cell.LeftImage.Image = News[ leftImageIndex ].Image;
                    }
                    else
                    {
                        cell.LeftImage.Image = ImagePlaceholder;
                    }
                }
                else
                {
                    cell.LeftImage.Image = ImagePlaceholder;
                }

                // now if there's a right item, set it
                int rightImageIndex = leftImageIndex + 1;
                if ( rightImageIndex < News.Count )
                {
                    if ( News[ rightImageIndex ].Image != null )
                    {
                        cell.RightImage.Image = News[ rightImageIndex ].Image;
                    }
                    else
                    {
                        cell.RightImage.Image = ImagePlaceholder;
                    }    
                }
                else
                {
                    cell.RightImage.Image = ImagePlaceholder;
                }

                // scale down the image to the width of the device
                nfloat imageWidth = cell.LeftImage.Image.Size.Width;
                nfloat imageHeight = cell.LeftImage.Image.Size.Height;

                // set the cell and child bounds
                nfloat aspectRatio = (float) (imageHeight / imageWidth);
                cell.Bounds = new CGRect( 0, 0, tableView.Bounds.Width, (tableView.Bounds.Width / 2) * aspectRatio );

                cell.LeftImage.Frame = new CGRect( cell.Bounds.X, cell.Bounds.Y, cell.Bounds.Width / 2, cell.Bounds.Height );
                cell.LeftButton.Frame = cell.LeftImage.Frame;

                cell.RightImage.Frame = new CGRect( cell.LeftImage.Frame.Right, cell.Bounds.Y, cell.Bounds.Width / 2, cell.Bounds.Height );
                cell.RightButton.Frame = cell.RightImage.Frame;

                PendingCellHeight = cell.Bounds.Height;
                return cell;
            }
        }

        /// <summary>
        /// Definition for each cell in this table
        /// </summary>
        class StandardCell : UITableViewCell
        {
            public static string Identifier = "SeriesCell";

            public LandscapeTableSource ParentSource { get; set; }

            public int LeftImageIndex { get; set; }

            public UIImageView LeftImage { get; set; }
            public UIButton LeftButton { get; set; }


            public UIImageView RightImage { get; set; }
            public UIButton RightButton { get; set; }

            public StandardCell( UITableViewCellStyle style, string cellIdentifier ) : base( style, cellIdentifier )
            {
                LeftImage = new UIImageView( );
                LeftImage.ContentMode = UIViewContentMode.ScaleAspectFit;
                LeftImage.Layer.AnchorPoint = CGPoint.Empty;
                AddSubview( LeftImage );

                LeftButton = UIButton.FromType( UIButtonType.Custom );
                LeftButton.Layer.AnchorPoint = CGPoint.Empty;
                LeftButton.TouchUpInside += (object sender, EventArgs e) => 
                    {
                        ParentSource.RowClicked( LeftImageIndex );
                    };
                AddSubview( LeftButton );


                RightImage = new UIImageView( );
                RightImage.ContentMode = UIViewContentMode.ScaleAspectFit;
                RightImage.Layer.AnchorPoint = CGPoint.Empty;
                AddSubview( RightImage );

                RightButton = UIButton.FromType( UIButtonType.Custom );
                RightButton.Layer.AnchorPoint = CGPoint.Empty;
                RightButton.TouchUpInside += (object sender, EventArgs e) => 
                    {
                        ParentSource.RowClicked( LeftImageIndex + 1 );
                    };
                AddSubview( RightButton );
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

        UITableView NewsTableView { get; set; }

		public NewsMainUIViewController (IntPtr handle) : base (handle)
		{
            News = new List<NewsEntry>( );

            string imagePath = NSBundle.MainBundle.BundlePath + "/" + PrivateGeneralConfig.NewsMainPlaceholder;
            ImagePlaceholder = new UIImage( imagePath );
		}

        LandscapeTableSource LandscapeSource { get; set; }
        PortraitTableSource PortraitSource { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            NewsTableView = new UITableView( );
            View.AddSubview( NewsTableView );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            IsVisible = true;

            ReloadNews( );

            // populate our table
            LandscapeSource = new LandscapeTableSource( this, News, ImagePlaceholder );
            PortraitSource = new PortraitTableSource( this, News, ImagePlaceholder );

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
            if ( NewsTableView != null )
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
        }

        public void DownloadImages( )
        {
            // this will be called by our parent task when we're clear to download images.
            foreach ( NewsEntry newsItem in News )
            {
                // if no image is set, we couldn't load one, so download it.
                if ( newsItem.Image == null )
                {
                    FileCache.Instance.DownloadFileToCache( newsItem.News.ImageURL, newsItem.News.ImageName, delegate { SeriesImageDownloaded( ); } );
                    FileCache.Instance.DownloadFileToCache( newsItem.News.HeaderImageURL, newsItem.News.HeaderImageName, null );
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

        public override void LayoutChanged()
        {
            base.LayoutChanged();

            if ( SpringboardViewController.SupportsLandscapeWide( ) )
            {
                NewsTableView.Source = LandscapeSource;
            }
            else
            {
                NewsTableView.Source = PortraitSource;
            }

            // adjust the table height for our navbar.
            // We MUST do it here, and we also have to set ContentType to Top, as opposed to ScaleToFill, on the view itself,
            // or our changes will be overwritten
            NewsTableView.Frame = new CGRect( 0, 0, View.Bounds.Width, View.Bounds.Height );
            NewsTableView.ReloadData( );
        }

        public void RowClicked( int row )
        {
            if ( row < News.Count )
            {
                NewsDetailsUIViewController viewController = new NewsDetailsUIViewController();
                viewController.NewsItem = News[ row ].News;

                Task.PerformSegue( this, viewController );
            }
        }
	}
}
