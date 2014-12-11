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
            /// <summary>
            /// Definition for the primary (top) cell, which advertises the current series
            /// more prominently
            /// </summary>
            class SeriesPrimaryCell : UITableViewCell
            {
                public static string Identifier = "SeriesPrimaryCell";

                public TableSource Parent { get; set; }

                public UIImageView Image { get; set; }

                public UILabel Title { get; set; }
                public UITextView Desc { get; set; }
                public UILabel Date { get; set; }

                public UILabel Footer { get; set; }

                public SeriesPrimaryCell( UITableViewCellStyle style, string cellIdentifier ) : base( style, cellIdentifier )
                {
                    BackgroundColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.BG_Layer_Color );

                    // anything that's constant can be set here once in the constructor
                    Image = new UIImageView( );
                    Image.ContentMode = UIViewContentMode.ScaleAspectFit;
                    Image.Layer.AnchorPoint = PointF.Empty;
                    AddSubview( Image );

                    Title = new UILabel( );
                    Title.Font = Rock.Mobile.PlatformCommon.iOSCommon.LoadFontDynamic( NoteConfig.Series_Table_Large_Font, NoteConfig.Series_Table_Large_FontSize );
                    Title.Layer.AnchorPoint = PointF.Empty;
                    Title.TextColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
                    Title.BackgroundColor = UIColor.Clear;
                    Title.LineBreakMode = UILineBreakMode.TailTruncation;
                    AddSubview( Title );

                    Date = new UILabel( );
                    Date.Font = Rock.Mobile.PlatformCommon.iOSCommon.LoadFontDynamic( NoteConfig.Series_Table_Small_Font, NoteConfig.Series_Table_Small_FontSize );
                    Date.Layer.AnchorPoint = PointF.Empty;
                    Date.TextColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    Date.BackgroundColor = UIColor.Clear;
                    Date.LineBreakMode = UILineBreakMode.TailTruncation;
                    AddSubview( Date );

                    Desc = new UITextView( );
                    Desc.Font = Rock.Mobile.PlatformCommon.iOSCommon.LoadFontDynamic( NoteConfig.Series_Table_Small_Font, NoteConfig.Series_Table_Small_FontSize );
                    Desc.Layer.AnchorPoint = PointF.Empty;
                    Desc.TextColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    Desc.BackgroundColor = UIColor.Clear;
                    Desc.TextContainerInset = UIEdgeInsets.Zero;
                    Desc.TextContainer.LineFragmentPadding = 0;
                    Desc.Editable = false;
                    Desc.UserInteractionEnabled = false;
                    AddSubview( Desc );

                    Footer = new UILabel( );
                    Footer.BackgroundColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.Table_Footer_Color );
                    AddSubview( Footer );
                }
            }

            /// <summary>
            /// Definition for each cell in this table
            /// </summary>
            class SeriesCell : UITableViewCell
            {
                public static string Identifier = "SeriesCell";

                public TableSource Parent { get; set; }

                public UILabel Title { get; set; }
                public UILabel Date { get; set; }
                public UILabel Speaker { get; set; }
                public UIButton WatchButton { get; set; }
                public UIButton TakeNotesButton { get; set; }

                public UIView Seperator { get; set; }

                public int RowIndex { get; set; }

                public SeriesCell( UITableViewCellStyle style, string cellIdentifier ) : base( style, cellIdentifier )
                {
                    Title = new UILabel( );
                    Title.Font = Rock.Mobile.PlatformCommon.iOSCommon.LoadFontDynamic( NoteConfig.Series_Table_Medium_Font, NoteConfig.Series_Table_Medium_FontSize );
                    Title.Layer.AnchorPoint = PointF.Empty;
                    Title.TextColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
                    Title.BackgroundColor = UIColor.Clear;
                    Title.LineBreakMode = UILineBreakMode.TailTruncation;
                    AddSubview( Title );

                    Date = new UILabel( );
                    Date.Font = Rock.Mobile.PlatformCommon.iOSCommon.LoadFontDynamic( NoteConfig.Series_Table_Small_Font, NoteConfig.Series_Table_Small_FontSize );
                    Date.Layer.AnchorPoint = PointF.Empty;
                    Date.TextColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    Date.BackgroundColor = UIColor.Clear;
                    Date.LineBreakMode = UILineBreakMode.TailTruncation;
                    AddSubview( Date );

                    Speaker = new UILabel( );
                    Speaker.Font = Rock.Mobile.PlatformCommon.iOSCommon.LoadFontDynamic( NoteConfig.Series_Table_Small_Font, NoteConfig.Series_Table_Small_FontSize );
                    Speaker.Layer.AnchorPoint = PointF.Empty;
                    Speaker.TextColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    Speaker.BackgroundColor = UIColor.Clear;
                    Speaker.LineBreakMode = UILineBreakMode.TailTruncation;
                    AddSubview( Speaker );

                    WatchButton = new UIButton( UIButtonType.Custom );
                    WatchButton.TouchUpInside += (object sender, EventArgs e) => { Parent.RowButtonClicked( RowIndex, 0 ); };
                    WatchButton.Layer.AnchorPoint = PointF.Empty;
                    WatchButton.Font = Rock.Mobile.PlatformCommon.iOSCommon.LoadFontDynamic( ControlStylingConfig.Icon_Font_Secondary, NoteConfig.Series_Table_IconSize );
                    WatchButton.SetTitle( NoteConfig.Series_Table_Watch_Icon, UIControlState.Normal );
                    WatchButton.BackgroundColor = UIColor.Clear;
                    WatchButton.SizeToFit( );
                    AddSubview( WatchButton );

                    TakeNotesButton = new UIButton( UIButtonType.Custom );
                    TakeNotesButton.TouchUpInside += (object sender, EventArgs e) => { Parent.RowButtonClicked( RowIndex, 1 ); };
                    TakeNotesButton.Font = Rock.Mobile.PlatformCommon.iOSCommon.LoadFontDynamic( ControlStylingConfig.Icon_Font_Secondary, NoteConfig.Series_Table_IconSize );
                    TakeNotesButton.SetTitle( NoteConfig.Series_Table_TakeNotes_Icon, UIControlState.Normal );
                    TakeNotesButton.Layer.AnchorPoint = PointF.Empty;
                    TakeNotesButton.BackgroundColor = UIColor.Clear;
                    TakeNotesButton.SizeToFit( );
                    AddSubview( TakeNotesButton );

                    Seperator = new UIView( );
                    AddSubview( Seperator );
                    Seperator.Layer.BorderWidth = 1;
                    Seperator.Layer.BorderColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.BG_Layer_Color ).CGColor;
                }

                public void ToggleWatchButton( bool enabled )
                {
                    if ( enabled == true )
                    {
                        WatchButton.Enabled = true;
                        WatchButton.SetTitleColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ), UIControlState.Normal );
                    }
                    else
                    {
                        WatchButton.Enabled = false;
                        WatchButton.SetTitleColor( UIColor.DarkGray, UIControlState.Normal );
                    }
                }

                public void ToggleTakeNotesButton( bool enabled )
                {
                    if ( enabled == true )
                    {
                        TakeNotesButton.Enabled = true;
                        TakeNotesButton.SetTitleColor( PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor ), UIControlState.Normal );
                    }
                    else
                    {
                        TakeNotesButton.Enabled = false;
                        TakeNotesButton.SetTitleColor( UIColor.DarkGray, UIControlState.Normal );
                    }
                }
            }

            NotesDetailsUIViewController Parent { get; set; }
            List<MessageEntry> MessageEntries { get; set; }
            Series Series { get; set; }
            UIImage Banner { get; set; }

            float PendingPrimaryCellHeight { get; set; }
            float PendingCellHeight { get; set; }

            public TableSource (NotesDetailsUIViewController parent, List<MessageEntry> messages, Series series, UIImage banner )
            {
                Parent = parent;
                MessageEntries = messages;
                Series = series;
                Banner = banner;
            }

            public override int RowsInSection (UITableView tableview, int section)
            {
                return MessageEntries.Count + 1;
            }

            public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            {
                tableView.DeselectRow( indexPath, true );

                // let the parent know it should reveal the nav bar
                Parent.RowClicked( 0, -1 );
            }

            public override float GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return GetCachedRowHeight( tableView, indexPath );
            }

            public override float EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
            {
                return GetCachedRowHeight( tableView, indexPath );
            }

            float GetCachedRowHeight( UITableView tableView, NSIndexPath indexPath )
            {
                // Depending on the row, we either want the primary cell's height,
                // or a standard row's height.
                switch ( indexPath.Row )
                {
                    case 0:
                    {
                        if ( PendingPrimaryCellHeight > 0 )
                        {
                            return PendingPrimaryCellHeight;
                        }
                        break;
                    }

                    default:
                    {

                        if ( PendingCellHeight > 0 )
                        {
                            return PendingCellHeight;
                        }
                        break;
                    }
                }

                // If we don't have the cell's height yet (first render), return the table's height
                return tableView.Frame.Height;
            }

            public override UITableViewCell GetCell (UITableView tableView, MonoTouch.Foundation.NSIndexPath indexPath)
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
                SeriesPrimaryCell cell = tableView.DequeueReusableCell( SeriesPrimaryCell.Identifier ) as SeriesPrimaryCell;

                // if there are no cells to reuse, create a new one
                if (cell == null)
                {
                    cell = new SeriesPrimaryCell( UITableViewCellStyle.Default, SeriesCell.Identifier );
                    cell.Parent = this;

                    // take the parent table's width so we inherit its width constraint
                    cell.Bounds = new RectangleF( cell.Bounds.X, cell.Bounds.Y, tableView.Bounds.Width, cell.Bounds.Height );

                    // configure the cell colors
                    cell.BackgroundColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.BG_Layer_Color );
                    cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                }

                // Banner Image
                cell.Image.Image = Banner;
                cell.Image.SizeToFit( );

                // resize the image to fit the width of the device
                float imageAspect = cell.Image.Bounds.Height / cell.Image.Bounds.Width;
                cell.Image.Frame = new RectangleF( 0, 0, cell.Bounds.Width, cell.Bounds.Width * imageAspect );

                // Title
                cell.Title.Text = Series.Name;
                cell.Title.SizeToFit( );
                cell.Title.Frame = new RectangleF( 5, cell.Image.Frame.Bottom + 5, cell.Frame.Width - 5, cell.Title.Frame.Height + 5 );

                // Date
                cell.Date.Text = Series.DateRanges;
                cell.Date.SizeToFit( );
                cell.Date.Frame = new RectangleF( 5, cell.Title.Frame.Bottom, cell.Frame.Width - 5, cell.Date.Frame.Height + 5 );

                // Description
                cell.Desc.Text = Series.Description;
                cell.Desc.Bounds = new RectangleF( 0, 0, cell.Frame.Width - 10, float.MaxValue );
                cell.Desc.SizeToFit( );
                cell.Desc.Frame = new RectangleF( 5, cell.Date.Frame.Bottom, cell.Frame.Width - 10, cell.Desc.Frame.Height + 5 );

                // Footer
                cell.Footer.Frame = new RectangleF( 0, cell.Desc.Frame.Bottom, cell.Bounds.Width, 10 );

                PendingPrimaryCellHeight = cell.Footer.Frame.Bottom;

                return cell;
            }

            UITableViewCell GetStandardCell( UITableView tableView, int row )
            {
                SeriesCell cell = tableView.DequeueReusableCell( SeriesPrimaryCell.Identifier ) as SeriesCell;

                // if there are no cells to reuse, create a new one
                if ( cell == null )
                {
                    cell = new SeriesCell( UITableViewCellStyle.Default, SeriesCell.Identifier );
                    cell.Parent = this;

                    // take the parent table's width so we inherit its width constraint
                    cell.Bounds = new RectangleF( cell.Bounds.X, cell.Bounds.Y, tableView.Bounds.Width, cell.Bounds.Height );

                    // configure the cell colors
                    cell.BackgroundColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.BackgroundColor );
                    cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                }

                // update the cell's row index so on button taps we know which one was tapped
                cell.RowIndex = row;

                float rowHeight = 100;

                // Buttons
                cell.TakeNotesButton.Frame = new RectangleF( cell.Bounds.Width - cell.TakeNotesButton.Bounds.Width, 
                    (rowHeight - cell.TakeNotesButton.Bounds.Height) / 2, 
                                                             cell.TakeNotesButton.Bounds.Width, 
                                                             cell.TakeNotesButton.Bounds.Height );

                cell.WatchButton.Frame = new RectangleF( cell.TakeNotesButton.Frame.Left - cell.WatchButton.Bounds.Width - 20, 
                    (rowHeight - cell.WatchButton.Bounds.Height) / 2, 
                                                         cell.WatchButton.Bounds.Width, 
                                                         cell.WatchButton.Bounds.Height );

                float availableWidth = cell.Bounds.Width - cell.WatchButton.Bounds.Width - cell.TakeNotesButton.Bounds.Width - 5 - 20;

                // disable the button if there's no watch URL
                if ( string.IsNullOrEmpty( Series.Messages[ 0 ].WatchUrl ) )
                {
                    cell.ToggleWatchButton( false );
                }
                else
                {
                    cell.ToggleWatchButton( true );
                }

                // disable the button if there's no note URL
                if ( string.IsNullOrEmpty( Series.Messages[ 0 ].NoteUrl ) )
                {
                    cell.ToggleTakeNotesButton( false );
                }
                else
                {
                    cell.ToggleTakeNotesButton( true );
                }

                // Create the title
                cell.Title.Text = Series.Messages[ row ].Name;
                cell.Title.SizeToFit( );

                // Date
                cell.Date.Text = Series.Messages[ row ].Date;
                cell.Date.SizeToFit( );

                cell.Speaker.Text = Series.Messages[ row ].Speaker;
                cell.Speaker.SizeToFit( );

                // Position the Title & Date in the center to the right of the image
                float totalTextHeight = cell.Title.Bounds.Height + cell.Date.Bounds.Height + cell.Speaker.Bounds.Height + 5;

                cell.Title.Frame = new RectangleF( 5, (rowHeight - totalTextHeight) / 2, availableWidth, cell.Title.Frame.Height );
                cell.Date.Frame = new RectangleF( cell.Title.Frame.Left, cell.Title.Frame.Bottom, availableWidth, cell.Date.Frame.Height );
                cell.Speaker.Frame = new RectangleF( cell.Title.Frame.Left, cell.Date.Frame.Bottom, availableWidth, cell.Speaker.Frame.Height + 5 );

                // add the seperator to the bottom
                cell.Seperator.Frame = new RectangleF( 0, rowHeight - 1, cell.Bounds.Width, 1 );

                PendingCellHeight = rowHeight;

                return cell;
            }

            public void RowButtonClicked( int row, int buttonIndex )
            {
                Parent.RowClicked( row, buttonIndex );
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
        public UIImage SeriesBillboard { get; set; }
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
            SeriesTable.BackgroundColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.BackgroundColor );
            SeriesTable.SeparatorStyle = UITableViewCellSeparatorStyle.None;

            // setup the messages list
            Messages = new List<MessageEntry>();
            SeriesTable.Source = new TableSource( this, Messages, Series, SeriesBillboard );

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
                viewController.ShareUrl = Series.Messages[ row ].ShareUrl;

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
