using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using CCVApp.Shared.Network;
using System.Drawing;
using Rock.Mobile.Network;
using CCVApp.Shared.Notes.Model;
using System.Xml;
using System.IO;
using RestSharp;
using CCVApp.Shared.Config;
using CCVApp.Shared.Strings;
using Rock.Mobile.PlatformUI;
using System.Net;
using CCVApp.Shared;

namespace iOS
{
    partial class NotesMainUIViewController : TaskUIViewController
    {
        public class TableSource : UITableViewSource 
        {
            /// <summary>
            /// Definition for each cell in this table
            /// </summary>
            class SeriesCell : UITableViewCell
            {
                public SeriesCell( UITableViewCellStyle style, string cellIdentifier ) : base( style, cellIdentifier )
                {
                    Image = new UIImageView( );
                    Label = new UILabel( );

                    AddSubview( Image );
                    AddSubview( Label );
                }

                public TableSource ParentTableSource { get; set; }

                public UIImageView Image { get; set; }
                public UILabel Label { get; set; }
            }

            NotesMainUIViewController Parent { get; set; }

            List<SeriesEntry> SeriesEntries { get; set; }

            string cellIdentifier = "TableCell";

            float PendingCellHeight { get; set; }

            public TableSource (NotesMainUIViewController parent, List<SeriesEntry> series )
            {
                Parent = parent;

                SeriesEntries = series;
            }

            public override int RowsInSection (UITableView tableview, int section)
            {
                return SeriesEntries.Count;
            }

            public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            {
                tableView.DeselectRow( indexPath, true );

                // notify our parent
                Parent.RowClicked( indexPath.Row );
            }

            public override float GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                if ( PendingCellHeight > 0 )
                {
                    return PendingCellHeight;
                }
                else
                {
                    // check the height of the image and let that be the height for this row
                    return tableView.Frame.Height;
                }
            }

            const int CellVerticalPadding = 25;

            public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
            {
                SeriesCell cell = tableView.DequeueReusableCell (cellIdentifier) as SeriesCell;
                // if there are no cells to reuse, create a new one
                if (cell == null)
                {
                    cell = new SeriesCell (UITableViewCellStyle.Default, cellIdentifier);

                    // take the parent table's width so we inherit its width constraint
                    cell.Bounds = new RectangleF( cell.Bounds.X, cell.Bounds.Y, tableView.Bounds.Width, cell.Bounds.Height );

                    // configure the cell colors
                    cell.BackgroundColor = PlatformBaseUI.GetUIColor( NoteConfig.Series_Main_Table_CellBackgroundColor );
                    cell.TextLabel.TextColor = PlatformBaseUI.GetUIColor( NoteConfig.Series_Main_Table_CellTextColor );
                    cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                }

                // setup the image
                UIImageView image = cell.Image;
                image.Image = SeriesEntries[ indexPath.Row ].Thumbnail;
                image.ContentMode = UIViewContentMode.ScaleAspectFit;
                image.Layer.AnchorPoint = new System.Drawing.PointF( 0, 0 );
                image.SizeToFit( );

                // foce the image to be sized according to the height of the cell
                image.Frame = new RectangleF( (cell.Frame.Width - NoteConfig.Series_Main_CellImageWidth) / 2, 
                                               0, 
                                               NoteConfig.Series_Main_CellImageWidth, 
                                               NoteConfig.Series_Main_CellImageHeight );



                // create a text label next to the image that allows word wrapping
                UILabel textLabel = cell.Label;
                textLabel.Text = SeriesEntries[ indexPath.Row ].Series.Name;
                textLabel.LineBreakMode = UILineBreakMode.TailTruncation;
                textLabel.TextColor = PlatformBaseUI.GetUIColor( NoteConfig.Series_Main_Table_CellTextColor );

                // set the allowed width for the text, then adjust the size, and then expand the height in case it's too small.
                textLabel.TextAlignment = UITextAlignment.Center;
                textLabel.Frame = new System.Drawing.RectangleF( 0, 0, cell.Frame.Width, 0 );
                textLabel.SizeToFit( );
                textLabel.Frame = new System.Drawing.RectangleF( 0, image.Frame.Bottom, cell.Frame.Width, textLabel.Frame.Height );

                PendingCellHeight = textLabel.Frame.Bottom + CellVerticalPadding;

                return cell;
            }

        }

        /// <summary>
        /// A wrapper class that consolidates the series and its image
        /// </summary>
        public class SeriesEntry
        {
            public Series Series { get; set; }
            public UIImage Thumbnail { get; set; }
        }
        List<SeriesEntry> SeriesEntries { get; set; }
        UIImage ThumbnailPlaceholder{ get; set; }

        UIActivityIndicatorView ActivityIndicator { get; set; }

        NotesDetailsUIViewController DetailsViewController { get; set; }

        bool RequestSeries { get; set; }

        bool IsVisible { get; set; }

        public NotesMainUIViewController (IntPtr handle) : base (handle)
        {
            SeriesEntries = new List<SeriesEntry>();

            string imagePath = NSBundle.MainBundle.BundlePath + "/" + "podcastThumbnailPlaceholder.png";
            ThumbnailPlaceholder = new UIImage( imagePath );
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // setup our table
            NotesTableView.BackgroundColor = PlatformBaseUI.GetUIColor( NoteConfig.Series_Main_Table_BackgroundColor );
            NotesTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;

            ActivityIndicator = new UIActivityIndicatorView( new RectangleF( View.Frame.Width / 2, View.Frame.Height / 2, 0, 0 ) );
            ActivityIndicator.StartAnimating( );
            ActivityIndicator.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.White;
            ActivityIndicator.SizeToFit( );
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // adjust the table height for our navbar.
            // We MUST do it here, and we also have to set ContentType to Top, as opposed to ScaleToFill, on the view itself,
            // or our changes will be overwritten
            NotesTableView.Frame = new RectangleF( 0, NavigationController.NavigationBar.Frame.Height, View.Bounds.Width, View.Bounds.Height - NavigationController.NavigationBar.Frame.Height );
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            IsVisible = false;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            DetailsViewController = null;

            IsVisible = true;

            // if we haven't already kicked off a request, do so now
            if ( SeriesEntries.Count == 0 && RequestSeries == false )
            {
                View.AddSubview( ActivityIndicator );
                View.BringSubviewToFront( ActivityIndicator );
                ActivityIndicator.Hidden = false;

                RequestSeries = true;

                // grab the series info
                Rock.Mobile.Network.HttpRequest request = new HttpRequest();
                RestRequest restRequest = new RestRequest( Method.GET );
                restRequest.RequestFormat = DataFormat.Xml;

                request.ExecuteAsync<List<Series>>( NoteConfig.BaseURL + "series.xml", restRequest, 
                    delegate(System.Net.HttpStatusCode statusCode, string statusDescription, List<Series> model )
                    {
                        // only do image work on the main thread
                        InvokeOnMainThread( delegate
                            {
                                ActivityIndicator.Hidden = true;
                                ActivityIndicator.RemoveFromSuperview( );

                                RequestSeries = false;

                                if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) )
                                {
                                    if ( model != null )
                                    {
                                        // setup each series entry in our table
                                        SetupSeriesEntries( model );

                                        // only update the table if we're still visible
                                        if ( IsVisible == true )
                                        {
                                            TableSource source = new TableSource( this, SeriesEntries );
                                            NotesTableView.Source = source;
                                            NotesTableView.ReloadData( );
                                        }
                                    }
                                    else
                                    {
                                        if ( IsVisible == true )
                                        {
                                            SpringboardViewController.DisplayError( MessagesStrings.Error_Title, MessagesStrings.Error_Message );
                                        }
                                    }
                                }
                                else
                                {
                                    if ( IsVisible == true )
                                    {
                                        SpringboardViewController.DisplayError( MessagesStrings.Error_Title, MessagesStrings.Error_Message );
                                    }
                                }
                            });
                    } );
            }
            else
            {
                TableSource source = new TableSource( this, SeriesEntries );
                NotesTableView.Source = source;
                NotesTableView.ReloadData( );
            }
        }

        void SetupSeriesEntries( List<Series> seriesList )
        {
            foreach ( Series series in seriesList )
            {
                // add the entry to our list
                SeriesEntry entry = new SeriesEntry();
                SeriesEntries.Add( entry );

                // copy over the series and give it a placeholder image
                entry.Series = series;
                entry.Thumbnail = ThumbnailPlaceholder;

                // first see if the image is cached
                MemoryStream image = ImageCache.Instance.ReadImage( entry.Series.Name );
                if ( image != null )
                {
                    ApplyBillboardImage( entry, image );

                    image.Dispose( );

                    if ( IsVisible == true )
                    {
                        NotesTableView.ReloadData( );
                    }
                }
                else
                {
                    // darn, it isn't here, so we'll need to download it.

                    // request the thumbnail image for the series
                    HttpRequest webRequest = new HttpRequest();
                    RestRequest restRequest = new RestRequest( Method.GET );

                    webRequest.ExecuteAsync( series.BillboardUrl, restRequest, 
                        delegate(HttpStatusCode statusCode, string statusDescription, byte[] model )
                        {
                            if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                // update the image on the UI Thread ONLY!
                                InvokeOnMainThread( delegate
                                    {
                                        MemoryStream imageBuffer = new MemoryStream( model );

                                        // write it to cache
                                        ImageCache.Instance.WriteImage( imageBuffer, entry.Series.Name );

                                        ApplyBillboardImage( entry, imageBuffer );

                                        if ( IsVisible == true )
                                        {
                                            NotesTableView.ReloadData( );
                                        }

                                        // dump the memory stream
                                        imageBuffer.Dispose( );
                                    });
                            }
                        } );
                }
            }
        }

        void ApplyBillboardImage( SeriesEntry entry, MemoryStream imageBuffer )
        {
            // create a UIImage out of the stream
            NSData imageData = NSData.FromStream( imageBuffer );
            UIImage uiImage = new UIImage( imageData );

            entry.Thumbnail = uiImage;
        }

        public void RowClicked( int row )
        {
            DetailsViewController = Storyboard.InstantiateViewController( "NotesDetailsUIViewController" ) as NotesDetailsUIViewController;
            DetailsViewController.Series = SeriesEntries[ row ].Series;

            // Note - if they are fast enough, they will end up going to the details of a series before
            // the series banner comes down, resulting in them seeing the generic thumbnail.
            // This isn't really a bug, more just a design issue. Ultimately it'll go away when we
            // start caching images
            DetailsViewController.ThumbnailPlaceholder = SeriesEntries[ row ].Thumbnail;

            Task.PerformSegue( this, DetailsViewController );
        }
    }
}
