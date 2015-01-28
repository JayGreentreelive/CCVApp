using System;
using Foundation;
using UIKit;
using System.CodeDom.Compiler;
using CoreLocation;
using MapKit;
using CCVApp.Shared.Config;
using CoreGraphics;
using System.Collections.Generic;
using CCVApp.Shared.Network;
using Rock.Mobile.Util.Strings;
using System.Collections;
using CCVApp.Shared;
using Rock.Mobile.PlatformSpecific.iOS.UI;
using CCVApp.Shared.Strings;

namespace iOS
{
	partial class GroupFinderViewController : TaskUIViewController
	{
        public GroupFinderViewController (IntPtr handle) : base (handle)
        {
        }

        public class TableSource : UITableViewSource 
        {
            /// <summary>
            /// Definition for the primary (top) cell, which contains the map and search field
            /// </summary>
            class PrimaryCell : UITableViewCell
            {
                public static string Identifier = "PrimaryCell";

                public TableSource TableSource { get; set; }

                public UITextField AddressTextField { get; set; }
                public MKMapView MapView { get; set; }

                public UILabel SearchResultsBanner { get; set; }
                //public UILabel NeighborhoodBanner { get; set; }
                public UIView Seperator { get; set; }

                public UIButton SearchButton { get; set; }

                /// <summary>
                /// Delegate for our address field. When returning, notify the primary cell's parent that this was clicked.
                /// </summary>
                class AddressDelegate : UITextFieldDelegate
                {
                    public PrimaryCell PrimaryCell { get; set; }

                    public override bool ShouldReturn(UITextField textField)
                    {
                        PrimaryCell.TableSource.RowClicked( 0, textField.Text );
                        return true;
                    }
                }

                public PrimaryCell( CGSize parentSize, UITableViewCellStyle style, string cellIdentifier ) : base( style, cellIdentifier )
                {
                    BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );

                    SearchButton = UIButton.FromType( UIButtonType.System );
                    SearchButton.Layer.AnchorPoint = new CGPoint( 0, 0 );
                    ControlStyling.StyleButton( SearchButton, ConnectConfig.GroupFinder_SearchIcon, ControlStylingConfig.Icon_Font_Secondary, 32 );
                    SearchButton.SizeToFit( );
                    SearchButton.Frame = new CGRect( parentSize.Width - SearchButton.Frame.Width, 0, SearchButton.Frame.Width, 33 );
                    SearchButton.TouchUpInside += (object sender, EventArgs e) => 
                        {
                            TableSource.RowClicked( 0, AddressTextField.Text );
                        };
                    AddSubview( SearchButton );

                    AddressTextField = new UITextField( );
                    AddressTextField.Layer.AnchorPoint = new CGPoint( 0, 0 );
                    ControlStyling.StyleTextField( AddressTextField, ConnectStrings.GroupFinder_AddressPlaceholder, ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
                    AddressTextField.Frame = new CGRect( 10, 0, parentSize.Width - SearchButton.Frame.Width - 5, SearchButton.Frame.Height );
                    AddressTextField.ReturnKeyType = UIReturnKeyType.Search;
                    AddressTextField.Delegate = new AddressDelegate( ) { PrimaryCell = this };
                    AddSubview( AddressTextField );

                    MapView = new MKMapView( );
                    MapView.Layer.AnchorPoint = new CGPoint( 0, 0 );
                    MapView.Frame = new CGRect( 0, AddressTextField.Frame.Bottom, parentSize.Width, 250 );
                    AddSubview( MapView );

                    SearchResultsBanner = new UILabel( );
                    SearchResultsBanner.Layer.AnchorPoint = new CGPoint( 0, 0 );
                    SearchResultsBanner.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
                    SearchResultsBanner.Text = ConnectStrings.GroupFinder_BeforeSearch;
                    SearchResultsBanner.SizeToFit( );
                    SearchResultsBanner.Frame = new CGRect( 0, MapView.Frame.Bottom, parentSize.Width, SearchResultsBanner.Frame.Height );
                    SearchResultsBanner.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    SearchResultsBanner.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Table_Footer_Color );
                    SearchResultsBanner.TextAlignment = UITextAlignment.Center;
                    AddSubview( SearchResultsBanner );

                    /*NeighborhoodBanner = new UILabel( );
                    NeighborhoodBanner.Layer.AnchorPoint = new System.Drawing.PointF( 0, 0 );
                    NeighborhoodBanner.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
                    NeighborhoodBanner.Text = "Your neighborhood is {0}";
                    NeighborhoodBanner.SizeToFit( );
                    NeighborhoodBanner.Frame = new RectangleF( 0, SearchResultsBanner.Frame.Bottom, parentSize.Width, NeighborhoodBanner.Frame.Height );
                    NeighborhoodBanner.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    NeighborhoodBanner.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Table_Footer_Color );
                    NeighborhoodBanner.TextAlignment = UITextAlignment.Center;
                    AddSubview( NeighborhoodBanner );*/

                    // add the seperator to the bottom
                    Seperator = new UIView( );
                    AddSubview( Seperator );
                    Seperator.Layer.BorderWidth = 1;
                    Seperator.Layer.BorderColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ).CGColor;
                    Seperator.Frame = new CGRect( 0, SearchResultsBanner.Frame.Bottom - 1, Bounds.Width, 1 );
                }
            }

            /// <summary>
            /// Definition for each cell in this table
            /// </summary>
            class GroupCell : UITableViewCell
            {
                public static string Identifier = "GroupCell";

                public TableSource TableSource { get; set; }

                public UILabel Title { get; set; }
                public UILabel Address { get; set; }
                public UILabel Neighborhood { get; set; }
                public UILabel Distance { get; set; }

                public UIView Seperator { get; set; }

                public int RowIndex { get; set; }

                public GroupCell( UITableViewCellStyle style, string cellIdentifier ) : base( style, cellIdentifier )
                {
                    Title = new UILabel( );
                    Title.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Medium_Font_Regular, ControlStylingConfig.Medium_FontSize );
                    Title.Layer.AnchorPoint = CGPoint.Empty;
                    Title.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor );
                    Title.BackgroundColor = UIColor.Clear;
                    Title.LineBreakMode = UILineBreakMode.TailTruncation;
                    AddSubview( Title );

                    Address = new UILabel( );
                    Address.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
                    Address.Layer.AnchorPoint = CGPoint.Empty;
                    Address.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    Address.BackgroundColor = UIColor.Clear;
                    Address.LineBreakMode = UILineBreakMode.TailTruncation;
                    Address.Lines = 99;
                    AddSubview( Address );

                    Neighborhood = new UILabel( );
                    Neighborhood.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
                    Neighborhood.Layer.AnchorPoint = CGPoint.Empty;
                    Neighborhood.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor );
                    Neighborhood.BackgroundColor = UIColor.Clear;
                    Neighborhood.LineBreakMode = UILineBreakMode.TailTruncation;
                    AddSubview( Neighborhood );

                    Seperator = new UIView( );
                    AddSubview( Seperator );
                    Seperator.Layer.BorderWidth = 1;
                    Seperator.Layer.BorderColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color ).CGColor;
                }
            }

            GroupFinderViewController Parent { get; set; }

            nfloat PendingPrimaryCellHeight { get; set; }
            nfloat PendingCellHeight { get; set; }

            PrimaryCell PrimaryTableCell { get; set; }

            public TableSource (GroupFinderViewController parent )
            {
                Parent = parent;

                PrimaryTableCell = new PrimaryCell( parent.View.Bounds.Size, UITableViewCellStyle.Default, PrimaryCell.Identifier );
                PrimaryTableCell.TableSource = this;

                // take the parent table's width so we inherit its width constraint
                PrimaryTableCell.Bounds = parent.View.Bounds;

                // configure the cell colors
                PrimaryTableCell.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );
                PrimaryTableCell.SelectionStyle = UITableViewCellSelectionStyle.None;

                // default to our startup location
                PendingPrimaryCellHeight = PrimaryTableCell.Seperator.Frame.Bottom;

                // additionally, set the default position for the map to whatever specified area.
                MKCoordinateRegion region = MKCoordinateRegion.FromDistance( new CLLocationCoordinate2D( 
                    ConnectConfig.GroupFinder_DefaultLatitude, 
                    ConnectConfig.GroupFinder_DefaultLongitude ), 
                    ConnectConfig.GroupFinder_DefaultScale_iOS, 
                    ConnectConfig.GroupFinder_DefaultScale_iOS );

                PrimaryTableCell.MapView.SetRegion( region, true );
            }

            public void UpdateAddress( string address )
            {
                PrimaryTableCell.AddressTextField.Text = address;
            }

            public void UpdateMap( )
            {
                // remove existing annotations
                PrimaryTableCell.MapView.RemoveAnnotations( PrimaryTableCell.MapView.Annotations );

                // set the search results banner appropriately
                if ( Parent.GroupEntries.Count > 0 )
                {
                    PrimaryTableCell.SearchResultsBanner.Text = ConnectStrings.GroupFinder_GroupsFound;
                }
                else
                {
                    PrimaryTableCell.SearchResultsBanner.Text = ConnectStrings.GroupFinder_NoGroupsFound;
                }
                PendingPrimaryCellHeight = PrimaryTableCell.Seperator.Frame.Bottom;

                // add an annotation for each position found in the group
                List<IMKAnnotation> annotations = new List<IMKAnnotation>();
                foreach ( GroupFinder.GroupEntry entry in Parent.GroupEntries )
                {
                    MKPointAnnotation annotation = new MKPointAnnotation();
                    annotation.SetCoordinate( new CLLocationCoordinate2D( double.Parse( entry.Latitude ), double.Parse( entry.Longitude ) ) );
                    annotation.Title = entry.Title;
                    annotation.Subtitle = entry.Distance;
                    annotations.Add( annotation );
                }
                PrimaryTableCell.MapView.ShowAnnotations( annotations.ToArray( ), true );
            }

            public override nint RowsInSection (UITableView tableview, nint section)
            {
                return Parent.GroupEntries.Count + 1;
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
                GroupCell cell = tableView.DequeueReusableCell( GroupCell.Identifier ) as GroupCell;

                // if there are no cells to reuse, create a new one
                if (cell == null)
                {
                    cell = new GroupCell( UITableViewCellStyle.Default, GroupCell.Identifier );
                    cell.TableSource = this;

                    // take the parent table's width so we inherit its width constraint
                    cell.Bounds = new CGRect( cell.Bounds.X, cell.Bounds.Y, tableView.Bounds.Width, cell.Bounds.Height );

                    // configure the cell colors
                    cell.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BackgroundColor );
                    cell.SelectionStyle = UITableViewCellSelectionStyle.None;
                }

                cell.RowIndex = row;

                // Create the title
                cell.Title.Text = Parent.GroupEntries[ row ].Title;
                cell.Title.SizeToFit( );

                // Address
                cell.Address.Text = Parent.GroupEntries[ row ].Address;
                cell.Address.SizeToFit( );

                // Neighborhood
                cell.Neighborhood.Text = Parent.GroupEntries[ row ].NeighborhoodArea;
                cell.Neighborhood.SizeToFit( );

                // Position the Title & Address in the center to the right of the image
                cell.Title.Frame = new CGRect( 10, 0, cell.Frame.Width - 5, cell.Title.Frame.Height );
                cell.Address.Frame = new CGRect( 10, cell.Title.Frame.Bottom - 6, cell.Frame.Width - 5, cell.Address.Frame.Height + 5 );
                cell.Neighborhood.Frame = new CGRect( 10, cell.Address.Frame.Bottom - 6, cell.Frame.Width - 5, cell.Neighborhood.Frame.Height + 5 );

                // add the seperator to the bottom
                cell.Seperator.Frame = new CGRect( 0, cell.Neighborhood.Frame.Bottom - 1, cell.Bounds.Width, 1 );

                PendingCellHeight = cell.Seperator.Frame.Bottom;

                return cell;
            }

            public override void RowSelected (UITableView tableView, NSIndexPath indexPath)
            {
                tableView.DeselectRow( indexPath, true );

                // let the parent know it should reveal the nav bar
                Parent.RowClicked( indexPath.Row, "-1" );
            }

            public void RowClicked( int row, string context = null )
            {
                Parent.RowClicked( row, context );
            }
        }


        List<GroupFinder.GroupEntry> GroupEntries { get; set; }

        GroupFinderViewController.TableSource GroupTableSource { get; set; }

        BlockerView BlockerView { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( CCVApp.Shared.Config.ControlStylingConfig.BackgroundColor );

            GroupEntries = new List<GroupFinder.GroupEntry>();

            GroupTableSource = new GroupFinderViewController.TableSource( this );
            GroupFinderTableView.Source = GroupTableSource;
            GroupFinderTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;

            BlockerView = new BlockerView( View.Frame );
            View.AddSubview( BlockerView );
        }

        public void RowClicked( int row, string context )
        {
            if ( context == "-1" )
            {
                Task.NavToolbar.RevealForTime( 3.0f );
            }
            else
            {
                // if the first row was clicked, (and it wasn't a -1 context),
                // run address searching.
                if ( row == 0 )
                {
                    GetGroups( context );
                }
            }
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // see if there's an address for this person that we can automatically use.
            if ( RockMobileUser.Instance.HasFullAddress( ) == true )
            {
                string address = RockMobileUser.Instance.Street1( ) + " " + RockMobileUser.Instance.City( ) + ", " + RockMobileUser.Instance.State( ) + ", " + RockMobileUser.Instance.Zip( );

                GetGroups( address );
                GroupTableSource.UpdateAddress( address );
            }
        }

        void GetGroups( string address )
        {
            if ( string.IsNullOrEmpty( address ) == false )
            {
                BlockerView.FadeIn( delegate
                    {
                        GroupFinder.GetGroups( address, 
                            delegate( bool result, List<GroupFinder.GroupEntry> groupEntries )
                            {
                                BlockerView.FadeOut( delegate
                                    {
                                        if ( result == false )
                                        {
                                            SpringboardViewController.DisplayError( ConnectStrings.GroupFinder_InvalidAddressHeader, ConnectStrings.GroupFinder_InvalidAddressMsg );
                                        }
                                        else
                                        {
                                            GroupEntries = groupEntries;
                                            GroupTableSource.UpdateMap( );
                                            GroupFinderTableView.ReloadData( );
                                        }
                                    } );
                            } );
                    } );
            }
        }
	}
}
