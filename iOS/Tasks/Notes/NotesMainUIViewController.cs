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
            /// Definition for the primary (top) cell, which advertises the current series
            /// more prominently
            /// </summary>
            class SeriesPrimaryCell : UITableViewCell
            {
                public static string Identifier = "SeriesPrimaryCell";

                public TableSource Parent { get; set; }

                public UILabel TopBanner { get; set; }

                public UIImageView Image { get; set; }


                public UILabel Title { get; set; }
                public UILabel Date { get; set; }
                public UILabel Speaker { get; set; }

                public UIButton WatchButton { get; set; }
                public UILabel WatchButtonIcon { get; set; }
                public UILabel WatchButtonLabel { get; set; }

                public UIButton TakeNotesButton { get; set; }
                public UILabel TakeNotesButtonIcon { get; set; }
                public UILabel TakeNotesButtonLabel { get; set; }

                public UILabel BottomBanner { get; set; }

                public SeriesPrimaryCell( UITableViewCellStyle style, string cellIdentifier ) : base( style, cellIdentifier )
                {
                    BackgroundColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.BG_Layer_Color );

                    // anything that's constant can be set here once in the constructor
                    TopBanner = new UILabel( );
                    TopBanner.Font = Rock.Mobile.PlatformCommon.iOSCommon.LoadFontDynamic( NoteConfig.Series_Table_Small_Font, NoteConfig.Series_Table_Small_FontSize );
                    TopBanner.Layer.AnchorPoint = PointF.Empty;
                    TopBanner.Text = MessagesStrings.Series_TopBanner;
                    TopBanner.TextColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    TopBanner.BackgroundColor = UIColor.Clear;
                    TopBanner.TextAlignment = UITextAlignment.Center;
                    AddSubview( TopBanner );

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

                    Speaker = new UILabel( );
                    Speaker.Font = Rock.Mobile.PlatformCommon.iOSCommon.LoadFontDynamic( NoteConfig.Series_Table_Small_Font, NoteConfig.Series_Table_Small_FontSize );
                    Speaker.Layer.AnchorPoint = PointF.Empty;
                    Speaker.TextColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    Speaker.BackgroundColor = UIColor.Clear;
                    Speaker.LineBreakMode = UILineBreakMode.TailTruncation;
                    AddSubview( Speaker );

                    WatchButton = new UIButton( UIButtonType.Custom );
                    WatchButton.TouchUpInside += (object sender, EventArgs e) => { Parent.WatchButtonClicked( ); };
                    WatchButton.Layer.AnchorPoint = PointF.Empty;
                    WatchButton.BackgroundColor = UIColor.Clear;
                    WatchButton.Layer.BorderColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ).CGColor;
                    WatchButton.Layer.BorderWidth = 1;
                    WatchButton.SizeToFit( );
                    AddSubview( WatchButton );

                    WatchButtonIcon = new UILabel( );
                    WatchButton.AddSubview( WatchButtonIcon );
                    WatchButtonIcon.Font = Rock.Mobile.PlatformCommon.iOSCommon.LoadFontDynamic( ControlStylingConfig.Icon_Font_Primary, NoteConfig.Series_Table_IconSize );
                    WatchButtonIcon.Text = NoteConfig.Series_Table_Watch_Icon;
                    WatchButtonIcon.SizeToFit( );

                    WatchButtonLabel = new UILabel( );
                    WatchButton.AddSubview( WatchButtonLabel );
                    WatchButtonLabel.Font = Rock.Mobile.PlatformCommon.iOSCommon.LoadFontDynamic( NoteConfig.Series_Table_Small_Font, NoteConfig.Series_Table_Small_FontSize );
                    WatchButtonLabel.Text = MessagesStrings.Series_Table_Watch;
                    WatchButtonLabel.SizeToFit( );
                    

                    TakeNotesButton = new UIButton( UIButtonType.Custom );
                    TakeNotesButton.TouchUpInside += (object sender, EventArgs e) => { Parent.TakeNotesButtonClicked( ); };
                    TakeNotesButton.Layer.AnchorPoint = PointF.Empty;
                    TakeNotesButton.BackgroundColor = UIColor.Clear;
                    TakeNotesButton.Layer.BorderColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.BG_Layer_BorderColor ).CGColor;
                    TakeNotesButton.Layer.BorderWidth = 1;
                    TakeNotesButton.SizeToFit( );
                    AddSubview( TakeNotesButton );


                    TakeNotesButtonIcon = new UILabel( );
                    TakeNotesButton.AddSubview( TakeNotesButtonIcon );
                    TakeNotesButtonIcon.Font = Rock.Mobile.PlatformCommon.iOSCommon.LoadFontDynamic( ControlStylingConfig.Icon_Font_Primary, NoteConfig.Series_Table_IconSize );
                    TakeNotesButtonIcon.Text = NoteConfig.Series_Table_TakeNotes_Icon;
                    TakeNotesButtonIcon.SizeToFit( );

                    TakeNotesButtonLabel = new UILabel( );
                    TakeNotesButton.AddSubview( TakeNotesButtonLabel );
                    TakeNotesButtonLabel.Font = Rock.Mobile.PlatformCommon.iOSCommon.LoadFontDynamic( NoteConfig.Series_Table_Small_Font, NoteConfig.Series_Table_Small_FontSize );
                    TakeNotesButtonLabel.Text = MessagesStrings.Series_Table_TakeNotes;
                    TakeNotesButtonLabel.SizeToFit( );


                    BottomBanner = new UILabel( );
                    BottomBanner.Font = Rock.Mobile.PlatformCommon.iOSCommon.LoadFontDynamic( NoteConfig.Series_Table_Small_Font, NoteConfig.Series_Table_Small_FontSize );
                    BottomBanner.Layer.AnchorPoint = new System.Drawing.PointF( 0, 0 );
                    BottomBanner.Text = MessagesStrings.Series_Table_PreviousMessages;
                    BottomBanner.TextColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    BottomBanner.BackgroundColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.Table_Footer_Color );
                    BottomBanner.TextAlignment = UITextAlignment.Center;
                    AddSubview( BottomBanner );
                }

                public void ToggleWatchButton( bool enabled )
                {
                    if ( enabled == true )
                    {
                        WatchButton.Enabled = true;
                        WatchButtonIcon.TextColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
                        WatchButtonLabel.TextColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
                    }
                    else
                    {
                        WatchButton.Enabled = false;
                        WatchButtonIcon.TextColor = UIColor.DarkGray;
                        WatchButtonLabel.TextColor = UIColor.DarkGray;
                    }
                }

                public void ToggleTakeNotesButton( bool enabled )
                {
                    if ( enabled == true )
                    {
                        TakeNotesButton.Enabled = true;
                        TakeNotesButtonIcon.TextColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
                        TakeNotesButtonLabel.TextColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_ActiveTextColor );
                    }
                    else
                    {
                        TakeNotesButton.Enabled = false;
                        TakeNotesButtonIcon.TextColor = UIColor.DarkGray;
                        TakeNotesButtonLabel.TextColor = UIColor.DarkGray;
                    }
                }
            }

            /// <summary>
            /// Definition for each cell in this table
            /// </summary>
            class SeriesCell : UITableViewCell
            {
                public static string Identifier = "SeriesCell";

                public TableSource Parent { get; set; }

                public UIImageView Image { get; set; }
                public UILabel Title { get; set; }
                public UILabel Date { get; set; }
                public UILabel Chevron { get; set; }

                public UIView Seperator { get; set; }

                public SeriesCell( UITableViewCellStyle style, string cellIdentifier ) : base( style, cellIdentifier )
                {
                    Image = new UIImageView( );
                    Image.ContentMode = UIViewContentMode.ScaleAspectFit;
                    Image.Layer.AnchorPoint = PointF.Empty;
                    AddSubview( Image );

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

                    Chevron = new UILabel( );
                    AddSubview( Chevron );
                    Chevron.Font = Rock.Mobile.PlatformCommon.iOSCommon.LoadFontDynamic( ControlStylingConfig.Icon_Font_Primary, NoteConfig.Series_Table_IconSize );
                    Chevron.TextColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    Chevron.Text = NoteConfig.Series_Table_Navigate_Icon;
                    Chevron.SizeToFit( );

                    Seperator = new UIView( );
                    AddSubview( Seperator );
                    Seperator.Layer.BorderWidth = 1;
                    Seperator.Layer.BorderColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.BG_Layer_Color ).CGColor;
                }
            }

            NotesMainUIViewController Parent { get; set; }
            List<SeriesEntry> SeriesEntries { get; set; }

            float PendingPrimaryCellHeight { get; set; }
            float PendingCellHeight { get; set; }

            public TableSource (NotesMainUIViewController parent, List<SeriesEntry> series )
            {
                Parent = parent;
                SeriesEntries = series;
            }

            public override int RowsInSection (UITableView tableview, int section)
            {
                return SeriesEntries.Count + 1;
            }

            public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            {
                tableView.DeselectRow( indexPath, true );

                // notify our parent if it isn't the primary row.
                // The primary row only responds to its two buttons
                if ( indexPath.Row > 0 )
                {
                    Parent.RowClicked( indexPath.Row - 1 );
                }
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

                // // Top Banner
                cell.TopBanner.SizeToFit( );
                cell.TopBanner.Bounds = new RectangleF( 0, 0, cell.Bounds.Width, cell.TopBanner.Bounds.Height + 10 );


                // Banner Image
                cell.Image.Image = SeriesEntries[ 0 ].Thumbnail;
                cell.Image.SizeToFit( );

                // resize the image to fit the width of the device
                float imageAspect = cell.Image.Bounds.Height / cell.Image.Bounds.Width;
                cell.Image.Frame = new RectangleF( 0, 
                                                   cell.TopBanner.Frame.Bottom, 
                                                   cell.Bounds.Width, 
                                                   cell.Bounds.Width * imageAspect );


                // Create the title
                cell.Title.Text = SeriesEntries[ 0 ].Series.Messages[ 0 ].Name;
                cell.Title.SizeToFit( );
                cell.Title.Frame = new RectangleF( 5, cell.Image.Frame.Bottom + 5, cell.Frame.Width, cell.Title.Frame.Height + 5 );


                // Date & Speaker
                cell.Date.Text = SeriesEntries[ 0 ].Series.Messages[ 0 ].Date;
                cell.Date.SizeToFit( );
                cell.Date.Frame = new RectangleF( 5, cell.Title.Frame.Bottom, cell.Frame.Width, cell.Date.Frame.Height + 5 );

                cell.Speaker.Text = SeriesEntries[ 0 ].Series.Messages[ 0 ].Speaker;
                cell.Speaker.SizeToFit( );
                cell.Speaker.Frame = new RectangleF( cell.Frame.Width - cell.Speaker.Bounds.Width - 5, cell.Title.Frame.Bottom, cell.Frame.Width, cell.Speaker.Frame.Height + 5 );


                // Watch Button & Labels
                cell.WatchButton.Bounds = new RectangleF( 0, 0, cell.Frame.Width / 2 + 6, cell.WatchButton.Bounds.Height + 10 );
                cell.WatchButton.Layer.Position = new PointF( -5, cell.Speaker.Frame.Bottom + 15 );

                float labelTotalWidth = cell.WatchButtonIcon.Bounds.Width + cell.WatchButtonLabel.Bounds.Width + 5;
                cell.WatchButtonIcon.Layer.Position = new PointF( (cell.WatchButton.Bounds.Width - labelTotalWidth) / 2 + (cell.WatchButtonIcon.Bounds.Width / 2), cell.WatchButton.Bounds.Height / 2 );
                cell.WatchButtonLabel.Layer.Position = new PointF( cell.WatchButtonIcon.Frame.Right + (cell.WatchButtonLabel.Bounds.Width / 2), cell.WatchButton.Bounds.Height / 2 );

                // disable the button if there's no watch URL
                if ( string.IsNullOrEmpty( SeriesEntries[ 0 ].Series.Messages[ 0 ].WatchUrl ) )
                {
                    cell.ToggleWatchButton( false );
                }
                else
                {
                    cell.ToggleWatchButton( true );
                }


                // Take Notes Button & Labels
                cell.TakeNotesButton.Bounds = new RectangleF( 0, 0, cell.Frame.Width / 2 + 5, cell.TakeNotesButton.Bounds.Height + 10 );
                cell.TakeNotesButton.Layer.Position = new PointF( (cell.Frame.Width + 5) - cell.TakeNotesButton.Bounds.Width, cell.Speaker.Frame.Bottom + 15 );

                labelTotalWidth = cell.TakeNotesButtonIcon.Bounds.Width + cell.TakeNotesButtonLabel.Bounds.Width + 5;
                cell.TakeNotesButtonIcon.Layer.Position = new PointF( (cell.TakeNotesButton.Bounds.Width - labelTotalWidth) / 2 + (cell.TakeNotesButtonIcon.Bounds.Width / 2), cell.TakeNotesButton.Bounds.Height / 2 );
                cell.TakeNotesButtonLabel.Layer.Position = new PointF( cell.TakeNotesButtonIcon.Frame.Right + (cell.TakeNotesButtonLabel.Bounds.Width / 2), cell.TakeNotesButton.Bounds.Height / 2 );

                // disable the button if there's no note URL
                if ( string.IsNullOrEmpty( SeriesEntries[ 0 ].Series.Messages[ 0 ].NoteUrl ) )
                {
                    cell.ToggleTakeNotesButton( false );
                }
                else
                {
                    cell.ToggleTakeNotesButton( true );
                }


                // Bottom Banner
                cell.BottomBanner.SizeToFit( );
                cell.BottomBanner.Bounds = new RectangleF( 0, 0, cell.Bounds.Width, cell.BottomBanner.Bounds.Height + 10 );
                cell.BottomBanner.Layer.Position = new PointF( 0, cell.TakeNotesButton.Frame.Bottom - 1 );

                PendingPrimaryCellHeight = cell.BottomBanner.Frame.Bottom;

                return cell;
            }

            UITableViewCell GetStandardCell( UITableView tableView, int row )
            {
                SeriesCell cell = tableView.DequeueReusableCell( SeriesPrimaryCell.Identifier ) as SeriesCell;

                // if there are no cells to reuse, create a new one
                if (cell == null)
                {
                    cell = new SeriesCell( UITableViewCellStyle.Default, SeriesCell.Identifier );
                    cell.Parent = this;

                    // take the parent table's width so we inherit its width constraint
                    cell.Bounds = new RectangleF( cell.Bounds.X, cell.Bounds.Y, tableView.Bounds.Width, cell.Bounds.Height );

                    // configure the cell colors
                    cell.BackgroundColor = PlatformBaseUI.GetUIColor( ControlStylingConfig.BackgroundColor );
                    cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                }

                // Thumbnail Image
                cell.Image.Image = SeriesEntries[ row ].Thumbnail;
                cell.Image.SizeToFit( );

                // force the image to be sized according to the height of the cell
                cell.Image.Frame = new RectangleF( 0, 
                                                   0, 
                                                   NoteConfig.Series_Main_CellImageWidth, 
                                                   NoteConfig.Series_Main_CellImageHeight );

                float availableTextWidth = cell.Bounds.Width - cell.Chevron.Bounds.Width - cell.Image.Bounds.Width - 5;

                // Chevron
                cell.Chevron.Layer.Position = new PointF( cell.Bounds.Width - (cell.Chevron.Bounds.Width / 2) - 5, (NoteConfig.Series_Main_CellImageHeight) / 2 );

                // Create the title
                cell.Title.Text = SeriesEntries[ row ].Series.Name;
                cell.Title.SizeToFit( );

                // Date Range
                cell.Date.Text = SeriesEntries[ row ].Series.DateRanges;
                cell.Date.SizeToFit( );

                // Position the Title & Date in the center to the right of the image
                float totalTextHeight = cell.Title.Bounds.Height + cell.Date.Bounds.Height + 5;
                cell.Title.Frame = new RectangleF( cell.Image.Frame.Right + 5, (NoteConfig.Series_Main_CellImageHeight - totalTextHeight) / 2, availableTextWidth - 5, cell.Title.Frame.Height );
                cell.Date.Frame = new RectangleF( cell.Title.Frame.Left, cell.Title.Frame.Bottom, availableTextWidth - 5, cell.Date.Frame.Height + 5 );

                // add the seperator to the bottom
                cell.Seperator.Frame = new RectangleF( 0, cell.Image.Frame.Bottom - 1, cell.Bounds.Width, 1 );

                PendingCellHeight = cell.Image.Frame.Bottom;

                return cell;
            }

            public void TakeNotesButtonClicked( )
            {
                Parent.TakeNotesClicked( );
            }

            public void WatchButtonClicked( )
            {
                Parent.WatchButtonClicked( );
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
            NotesTableView.Frame = new RectangleF( 0, 0, View.Bounds.Width, View.Bounds.Height );
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
            UIImage uiImage = new UIImage( imageData, UIScreen.MainScreen.Scale );

            entry.Thumbnail = uiImage;
        }

        /// <summary>
        /// Called when the user pressed the 'Watch' button in the primary cell
        /// </summary>
        public void WatchButtonClicked( )
        {
            NotesWatchUIViewController viewController = Storyboard.InstantiateViewController( "NotesWatchUIViewController" ) as NotesWatchUIViewController;
            viewController.WatchUrl = SeriesEntries[ 0 ].Series.Messages[ 0 ].WatchUrl;
            viewController.ShareUrl = SeriesEntries[ 0 ].Series.Messages[ 0 ].ShareUrl;

            Task.PerformSegue( this, viewController );
        }

        /// <summary>
        /// Called when the user pressed the 'Take Notes' button in the primary cell
        /// </summary>
        public void TakeNotesClicked( )
        {
            NotesViewController viewController = new NotesViewController();
            viewController.NotePresentableName = string.Format( "Message - {0}", SeriesEntries[ 0 ].Series.Messages[ 0 ].Name );
            viewController.NoteName = SeriesEntries[ 0 ].Series.Messages[ 0 ].NoteUrl;

            Task.PerformSegue( this, viewController );
        }

        public void RowClicked( int row )
        {
            DetailsViewController = Storyboard.InstantiateViewController( "NotesDetailsUIViewController" ) as NotesDetailsUIViewController;
            DetailsViewController.Series = SeriesEntries[ row ].Series;
            DetailsViewController.SeriesBillboard = SeriesEntries[ row ].Thumbnail;

            // Note - if they are fast enough, they will end up going to the details of a series before
            // the series banner comes down, resulting in them seeing the generic thumbnail.
            // This isn't really a bug, more just a design issue. Ultimately it'll go away when we
            // start caching images
            DetailsViewController.ThumbnailPlaceholder = SeriesEntries[ row ].Thumbnail;

            Task.PerformSegue( this, DetailsViewController );
        }
    }
}
