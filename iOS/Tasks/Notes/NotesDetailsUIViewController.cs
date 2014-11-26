using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using CCVApp.Shared.Notes.Model;
using System.Collections.Generic;
using System.Drawing;
using CCVApp.Shared;
using CCVApp.Shared.Config;
using Rock.Mobile.PlatformUI;
using System.IO;

namespace iOS
{
    partial class NotesDetailsUIViewController : TaskUIViewController
	{
        public class TableSource : UITableViewSource 
        {
            NotesDetailsUIViewController Parent { get; set; }

            List<MessageEntry> Messages { get; set; }

            string cellIdentifier = "TableCell";

            float PendingCellHeight { get; set; }

            public TableSource (NotesDetailsUIViewController parent, List<MessageEntry> messages )
            {
                Parent = parent;

                Messages = messages;            }

            public override int RowsInSection (UITableView tableview, int section)
            {
                return Messages.Count;
            }

            public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            {
                tableView.DeselectRow( indexPath, true );

                // notify our parent
                Parent.RowClicked( indexPath.Row, -1 );
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

            public void ButtonClicked( int rowIndex, int buttonIndex )
            {
                Parent.RowClicked( rowIndex, buttonIndex );
            }

            class MessageCell : UITableViewCell
            {
                public MessageCell( UITableViewCellStyle style, string cellIdentifier ) : base( style, cellIdentifier )
                {
                    Image = new UIImageView( );
                    Label = new UILabel( );

                    Watch = UIButton.FromType( UIButtonType.System );
                    Read = UIButton.FromType( UIButtonType.System );

                    Watch.TouchUpInside += (object sender, EventArgs e) => 
                        {
                            ParentTableSource.ButtonClicked( RowIndex, 0 );
                        };

                    Read.TouchUpInside += (object sender, EventArgs e) => 
                        {
                            ParentTableSource.ButtonClicked( RowIndex, 1 );
                        };


                    Watch.SetTitle( "Watch", UIControlState.Normal );
                    Watch.SizeToFit( );


                    Read.SetTitle( "Take Notes", UIControlState.Normal );
                    Read.SizeToFit( );

                    AddSubview( Image );
                    AddSubview( Label );

                    AddSubview( Watch );
                    AddSubview( Read );
                }

                public TableSource ParentTableSource { get; set; }

                public int RowIndex { get; set; }

                public UIImageView Image { get; set; }
                public UILabel Label { get; set; }

                public UIButton Watch { get; set; }
                public UIButton Read { get; set; }
            }

            const int CellVerticalPadding = 10;
            const int CellHorizontalPadding = 10;

            public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
            {
                MessageCell cell = tableView.DequeueReusableCell (cellIdentifier) as MessageCell;

                // if there are no cells to reuse, create a new one
                if (cell == null)
                {
                    cell = new MessageCell (UITableViewCellStyle.Default, cellIdentifier);
                    cell.ParentTableSource = this;

                    // take the parent table's width so we inherit its width constraint
                    cell.Bounds = new RectangleF( cell.Bounds.X, cell.Bounds.Y, tableView.Bounds.Width, cell.Bounds.Height );

                    // configure the cell colors
                    cell.BackgroundColor = PlatformBaseUI.GetUIColor( NoteConfig.Series_Details_Table_CellBackgroundColor );
                    cell.Label.TextColor = PlatformBaseUI.GetUIColor( NoteConfig.Series_Details_Table_CellTextColor );
                    cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                }

                cell.RowIndex = indexPath.Row;

                // setup the image
                UIImageView image = cell.Image;
                image.Image = Messages[ indexPath.Row ].Thumbnail;
                image.ContentMode = UIViewContentMode.ScaleAspectFit;
                image.Layer.AnchorPoint = new System.Drawing.PointF( 0, 0 );
                image.SizeToFit( );

                // foce the image to be sized according to the height of the cell
                image.Frame = new RectangleF( (cell.Frame.Width - NoteConfig.Series_Details_CellImageWidth) / 2, 
                                               0, 
                                               NoteConfig.Series_Details_CellImageWidth, 
                                               NoteConfig.Series_Details_CellImageHeight );

                // create a text label next to the image that allows word wrapping
                UILabel textLabel = cell.Label;
                textLabel.Text = Messages[ indexPath.Row ].Message.Name;
                textLabel.LineBreakMode = UILineBreakMode.TailTruncation;

                // set the allowed width for the text, then adjust the size, and then expand the height in case it's too small.
                textLabel.TextAlignment = UITextAlignment.Center;
                textLabel.Frame = new System.Drawing.RectangleF( 0, 0, cell.Frame.Width, 0 );
                textLabel.SizeToFit( );
                textLabel.Frame = new System.Drawing.RectangleF( 0, image.Frame.Bottom, cell.Frame.Width, textLabel.Frame.Height );

                cell.Watch.Frame = new RectangleF( CellHorizontalPadding, textLabel.Frame.Bottom, cell.Watch.Frame.Width, cell.Watch.Frame.Height );
                cell.Read.Frame = new RectangleF( cell.Frame.Width - cell.Read.Frame.Width - CellHorizontalPadding, textLabel.Frame.Bottom, cell.Read.Frame.Width, cell.Read.Frame.Height );

                // the watch button should only be enabled if the message has a podcast
                cell.Watch.Enabled = Messages[ indexPath.Row ].HasPodcast;

                PendingCellHeight = cell.Read.Frame.Bottom + CellVerticalPadding * 2;

                return cell;
            }
        }

        /// <summary>
        /// A wrapper class that consolidates the message, it's thumbnail and podcast status
        /// </summary>
        public class MessageEntry
        {
            public Series.Message Message { get; set; }
            public UIImage Thumbnail { get; set; }
            public bool HasPodcast { get; set; }
        }

        public Series Series { get; set; }
        public List<MessageEntry> Messages { get; set; }
        public UIImage ThumbnailPlaceholder{ get; set; }
        bool IsVisible { get; set; }

		public NotesDetailsUIViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad( );

            // setup the table view and general background view colors
            View.BackgroundColor = PlatformBaseUI.GetUIColor( NoteConfig.Series_Details_Table_BackgroundColor );
            SeriesTable.BackgroundColor = PlatformBaseUI.GetUIColor( NoteConfig.Series_Details_Table_BackgroundColor );
            SeriesTable.SeparatorColor = PlatformBaseUI.GetUIColor( NoteConfig.Series_Details_Table_SeperatorBackgroundColor );

            // setup the messages list
            Messages = new List<MessageEntry>();
            SeriesTable.Source = new TableSource( this, Messages );

            IsVisible = true;

            for ( int i = 0; i < Series.Messages.Count; i++ )
            {
                MessageEntry messageEntry = new MessageEntry();
                Messages.Add( messageEntry );

                // give each message entry its message and the default thumbnail, which is the series billboard
                messageEntry.Message = Series.Messages[ i ];
                messageEntry.Thumbnail = ThumbnailPlaceholder;

                // grab the thumbnail IF it has a podcast
                if ( string.IsNullOrEmpty( Series.Messages[ i ].WatchUrl ) == false )
                {
                    messageEntry.HasPodcast = true;

                    int requestedIndex = i;

                    // first see if the image is cached
                    MemoryStream image = ImageCache.Instance.ReadImage( messageEntry.Message.Name );
                    if ( image != null )
                    {
                        ApplyBillboardImage( messageEntry, image );

                        image.Dispose( );
                    }
                    else
                    {
                        // it isn't, so we'll need to download it
                        VimeoManager.Instance.GetVideoThumbnail( Series.Messages[ requestedIndex ].WatchUrl, 
                            delegate(System.Net.HttpStatusCode statusCode, string statusDescription, System.IO.MemoryStream imageBuffer )
                            {
                                if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                                {
                                    // update the image on the UI Thread ONLY!
                                    InvokeOnMainThread( delegate
                                        {
                                            ImageCache.Instance.WriteImage( imageBuffer, messageEntry.Message.Name );

                                            if( IsVisible == true )
                                            {
                                                ApplyBillboardImage( messageEntry, imageBuffer );
                                            }

                                            imageBuffer.Dispose( );
                                        });
                                }
                            } );
                    }
                }
            }
        }

        void ApplyBillboardImage( MessageEntry messageEntry, MemoryStream imageBuffer )
        {
            // show the image
            NSData imageData = NSData.FromStream( imageBuffer );
            UIImage uiImage = new UIImage( imageData );

            messageEntry.Thumbnail = uiImage;
            SeriesTable.ReloadData( );
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            IsVisible = false;
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // adjust the table height for our navbar.
            // We MUST do it here, and we also have to set ContentType to Top, as opposed to ScaleToFill, on the view itself,
            // or our changes will be overwritten
            SeriesTable.Frame = new RectangleF( 0, 0, View.Bounds.Width, View.Bounds.Height );
        }

        public void RowClicked( int row, int buttonIndex )
        {
            // passing in -1 means they tapped an empty area of a cell. Use 
            // that to reveal the navbar
            if ( buttonIndex == -1 )
            {
                Task.NavToolbar.RevealForTime( 3.0f );
            }
            // 0 would be the first button, which is Watch
            else if ( buttonIndex == 0 )
            {
                NotesWatchUIViewController viewController = Storyboard.InstantiateViewController( "NotesWatchUIViewController" ) as NotesWatchUIViewController;
                viewController.WatchUrl = Series.Messages[ row ].WatchUrl;

                Task.PerformSegue( this, viewController );
            }
            // and 1 would be the second button, which is Notes
            else if ( buttonIndex == 1 )
            {
                NotesViewController viewController = new NotesViewController();
                viewController.NotePresentableName = string.Format( "Message - {0}", Series.Messages[ row ].Name );
                viewController.NoteName = Series.Messages[ row ].NoteUrl;

                Task.PerformSegue( this, viewController );
            }
        }
	}
}
