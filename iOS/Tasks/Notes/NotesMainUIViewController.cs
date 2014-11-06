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

namespace iOS
{
    partial class NotesMainUIViewController : TaskUIViewController
    {
        public class TableSource : UITableViewSource 
        {
            NotesMainUIViewController Parent { get; set; }

            List<Series> Series { get; set; }
            List<UIImageView> SeriesImage { get; set; }

            string cellIdentifier = "TableCell";

            float PendingCellHeight { get; set; }

            public TableSource (NotesMainUIViewController parent, List<Series> seriesList, List<UIImageView> seriesImage )
            {
                Parent = parent;

                Series = seriesList;

                SeriesImage = seriesImage;
            }

            public override int RowsInSection (UITableView tableview, int section)
            {
                return Series.Count;
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
                //UIImageView imageView = SeriesImage[ indexPath.Row ];
                //cell.ContentView.AddSubview( imageView );
                //cell.Bounds = imageView.Bounds;

                cell.TextLabel.Text = Series[ indexPath.Row ].Name + "\n" + Series[ indexPath.Row ].DateRanges + "\n" + Series[ indexPath.Row ].Description;
                cell.TextLabel.Lines = 0;
                cell.TextLabel.LineBreakMode = UILineBreakMode.WordWrap;
                cell.TextLabel.SizeToFit( );

                PendingCellHeight = cell.Frame.Height;

                return cell;
            }
        }

        public List<Series> Series { get; set; }
        List<UIImageView> SeriesImage { get; set; }
        UIActivityIndicatorView ActivityIndicator { get; set; }

        public NotesMainUIViewController (IntPtr handle) : base (handle)
        {
            Series = new List<Series>( );
            SeriesImage = new List<UIImageView>( );
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // setup our table
            NotesTableView.BackgroundColor = Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( CCVApp.Shared.Config.News.Table_BackgroundColor );
            NotesTableView.SeparatorColor = Rock.Mobile.PlatformUI.PlatformBaseUI.GetUIColor( 0x444444FF );
            //NotesTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;

            ActivityIndicator = new UIActivityIndicatorView( new RectangleF( View.Frame.Width / 2, NavigationController.NavigationBar.Frame.Height, 0, 0 ) );
            ActivityIndicator.StartAnimating( );
            ActivityIndicator.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.White;
            ActivityIndicator.SizeToFit( );

            View.AddSubview( ActivityIndicator );
            View.BringSubviewToFront( ActivityIndicator );

            ActivityIndicator.Hidden = false;
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // adjust the table height for our navbar.
            // We MUST do it here, and we also have to set ContentType to Top, as opposed to ScaleToFill, on the view itself,
            // or our changes will be overwritten
            NotesTableView.Frame = new RectangleF( 0, NavigationController.NavigationBar.Frame.Height, View.Bounds.Width, View.Bounds.Height - NavigationController.NavigationBar.Frame.Height );
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // grab the series info
            Rock.Mobile.Network.HttpRequest request = new HttpRequest();
            RestRequest restRequest = new RestRequest( Method.GET );
            restRequest.RequestFormat = DataFormat.Xml;

            request.ExecuteAsync<List<Series>>( CCVApp.Shared.Config.Note.BaseURL + "series.xml", restRequest, 
                delegate(System.Net.HttpStatusCode statusCode, string statusDescription, List<Series> model )
                {
                    ActivityIndicator.Hidden = true;

                    if( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) )
                    {
                        Series = model;

                        // on the main thread, update the list
                        InvokeOnMainThread( delegate
                            {
                                TableSource source = new TableSource( this, Series, null );
                                NotesTableView.Source = source;
                                NotesTableView.ReloadData( );
                            });
                    }
                    else
                    {
                        // error
                    }
                } );
        }

        public void RowClicked( int row )
        {
            NotesDetailsUIViewController viewController = Storyboard.InstantiateViewController( "NotesDetailsUIViewController" ) as NotesDetailsUIViewController;
            viewController.Series = Series[ row ];
            Task.PerformSegue( this, viewController );
        }
    }
}
