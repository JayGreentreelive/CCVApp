using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using CoreGraphics;
using CCVApp.Shared.Config;
using System.Collections.Generic;
using CCVApp.Shared;

namespace iOS
{
	partial class ConnectMainPageViewController : TaskUIViewController
	{
        public class TableSource : UITableViewSource 
        {
            /// <summary>
            /// Definition for the primary (top) cell, which contains the map and search field
            /// </summary>
            class PrimaryCell : UITableViewCell
            {
                public static string Identifier = "PrimaryCell";

                public UIImageView Image { get; set; }
                public TableSource TableSource { get; set; }
                public UIButton Button { get; set; }
                public UILabel BottomBanner { get; set; }

                public PrimaryCell( CGSize parentSize, UITableViewCellStyle style, string cellIdentifier ) : base( style, cellIdentifier )
                {
                    BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );

                    Image = new UIImageView( );
                    Image.ContentMode = UIViewContentMode.ScaleAspectFit;
                    Image.Layer.AnchorPoint = CGPoint.Empty;
                    AddSubview( Image );

                    // Banner Image
                    Image.Image = new UIImage( NSBundle.MainBundle.BundlePath + "/" + "connect_banner.jpg" );
                    Image.SizeToFit( );

                    // resize the image to fit the width of the device
                    nfloat imageAspect = Image.Bounds.Height / Image.Bounds.Width;
                    Image.Frame = new CGRect( 0, 0, parentSize.Width, parentSize.Width * imageAspect );

                    Button = UIButton.FromType( UIButtonType.System );
                    ControlStyling.StyleButton( Button, "Group Finder", ControlStylingConfig.Icon_Font_Secondary, 32 );
                    Button.BackgroundColor = UIColor.Clear;
                    Button.SizeToFit( );
                    Button.Frame = new CGRect( (parentSize.Width - Button.Frame.Width ) / 2, Image.Frame.Bottom - Button.Frame.Height, Button.Frame.Width, Button.Frame.Height );
                    Button.TouchUpInside += (object sender, EventArgs e) => 
                        {
                            TableSource.RowClicked( -1 );
                        };
                    AddSubview( Button );

                    BottomBanner = new UILabel( );
                    BottomBanner.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
                    BottomBanner.Text = "Other Ways to Connect";
                    BottomBanner.SizeToFit( );
                    BottomBanner.Frame = new CGRect( 0, Button.Frame.Bottom, parentSize.Width, BottomBanner.Frame.Height );
                    BottomBanner.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    BottomBanner.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Table_Footer_Color );
                    BottomBanner.TextAlignment = UITextAlignment.Center;
                    AddSubview( BottomBanner );
                }
            }

            /// <summary>
            /// Definition for each cell in this table
            /// </summary>
            class StandardCell : UITableViewCell
            {
                public static string Identifier = "StandardCell";

                public TableSource TableSource { get; set; }

                public UIButton Button { get; set; }

                public UIView Seperator { get; set; }

                public int RowIndex { get; set; }

                public StandardCell( UITableViewCellStyle style, string cellIdentifier ) : base( style, cellIdentifier )
                {
                    Button = UIButton.FromType( UIButtonType.System );
                    Button.TouchUpInside += (object sender, EventArgs e ) =>
                        {
                            TableSource.RowClicked( RowIndex );
                        };

                    AddSubview( Button );

                    Seperator = new UIView( );
                    AddSubview( Seperator );
                    Seperator.Layer.BorderWidth = 1;
                    Seperator.Layer.BorderColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ).CGColor;
                }
            }

            ConnectMainPageViewController Parent { get; set; }

            nfloat PendingPrimaryCellHeight { get; set; }
            nfloat PendingCellHeight { get; set; }

            PrimaryCell PrimaryTableCell { get; set; }

            public TableSource ( ConnectMainPageViewController parent )
            {
                Parent = parent;

                PrimaryTableCell = new PrimaryCell( parent.View.Bounds.Size, UITableViewCellStyle.Default, PrimaryCell.Identifier );
                PrimaryTableCell.TableSource = this;

                // take the parent table's width so we inherit its width constraint
                PrimaryTableCell.Bounds = parent.View.Bounds;

                // configure the cell colors
                PrimaryTableCell.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
                PrimaryTableCell.SelectionStyle = UITableViewCellSelectionStyle.None;

                PendingPrimaryCellHeight = PrimaryTableCell.BottomBanner.Frame.Bottom;
            }

            public override nint RowsInSection (UITableView tableview, nint section)
            {
                return Parent.LinkEntries.Count + 1;
            }

            public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath)
            {
                return GetCachedRowHeight( tableView, indexPath );
            }

            public override nfloat EstimatedHeight(UITableView tableView, NSIndexPath indexPath)
            {
                return GetCachedRowHeight( tableView, indexPath );
            }

            nfloat GetCachedRowHeight( UITableView tableView, NSIndexPath indexPath )
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

            public override UITableViewCell GetCell (UITableView tableView, NSIndexPath indexPath)
            {
                if ( indexPath.Row == 0 )
                {
                    return PrimaryTableCell;
                }
                else
                {
                    return GetStandardCell( tableView, indexPath.Row - 1 );
                }
            }

            UITableViewCell GetStandardCell( UITableView tableView, int row )
            {
                StandardCell cell = tableView.DequeueReusableCell( StandardCell.Identifier ) as StandardCell;

                // if there are no cells to reuse, create a new one
                if (cell == null)
                {
                    cell = new StandardCell( UITableViewCellStyle.Default, StandardCell.Identifier );
                    cell.TableSource = this;

                    // take the parent table's width so we inherit its width constraint
                    cell.Bounds = new CGRect( cell.Bounds.X, cell.Bounds.Y, tableView.Bounds.Width, cell.Bounds.Height );

                    // configure the cell colors
                    cell.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );
                    cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                }

                cell.RowIndex = row;
                cell.Button.SetTitle( Parent.LinkEntries[ row ].Title, UIControlState.Normal );
                cell.Button.SizeToFit( );
                cell.Button.Frame = new CGRect( (cell.Bounds.Width - cell.Button.Bounds.Width) / 2, 0, cell.Button.Bounds.Width, cell.Button.Bounds.Height );
                cell.Seperator.Frame = new CGRect( 0, cell.Button.Frame.Bottom - 1, cell.Bounds.Width, 1 );

                PendingCellHeight = cell.Button.Frame.Bottom;

                return cell;
            }

            public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            {
                tableView.DeselectRow( indexPath, true );

                // let the parent know it should reveal the nav bar
                Parent.RowClicked( indexPath.Row - 1 );
            }

            public void RowClicked( int row )
            {
                Parent.RowClicked( row );
            }
        }

        public List<ConnectLink> LinkEntries { get; set; }

		public ConnectMainPageViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( CCVApp.Shared.Config.ControlStylingConfig.BackgroundColor );

            LinkEntries = ConnectLink.BuildList( );

            ConnectTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            ConnectTableView.Source = new TableSource( this );
            ConnectTableView.ReloadData( );
        }

        public void RowClicked( int rowIndex )
        {
            if ( rowIndex == -1 )
            {
                TaskUIViewController viewController = Storyboard.InstantiateViewController( "GroupFinderViewController" ) as TaskUIViewController;
                Task.PerformSegue( this, viewController );
            }
            else
            {
                ConnectWebViewController viewController = Storyboard.InstantiateViewController( "ConnectWebViewController" ) as ConnectWebViewController;
                viewController.DisplayUrl = LinkEntries[ rowIndex ].Url;
                Task.PerformSegue( this, viewController );
            }
        }
	}
}
