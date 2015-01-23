using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using MonoTouch.CoreLocation;
using MonoTouch.MapKit;
using CCVApp.Shared.Config;
using System.Drawing;
using System.Collections.Generic;
using CCVApp.Shared.Network;
using Rock.Mobile.Util.Strings;
using System.Collections;

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

                public PrimaryCell( SizeF parentSize, UITableViewCellStyle style, string cellIdentifier ) : base( style, cellIdentifier )
                {
                    BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.BG_Layer_Color );

                    SearchButton = UIButton.FromType( UIButtonType.System );
                    SearchButton.Layer.AnchorPoint = new System.Drawing.PointF( 0, 0 );
                    ControlStyling.StyleButton( SearchButton, "", ControlStylingConfig.Icon_Font_Secondary, 32 );
                    //SearchButton.BackgroundColor = UIColor.Clear;
                    SearchButton.SizeToFit( );
                    SearchButton.Frame = new System.Drawing.RectangleF( parentSize.Width - SearchButton.Frame.Width, 0, SearchButton.Frame.Width, 33 );
                    SearchButton.TouchUpInside += (object sender, EventArgs e) => 
                        {
                            TableSource.RowClicked( 0, AddressTextField.Text );
                        };
                    AddSubview( SearchButton );

                    AddressTextField = new UITextField( );
                    AddressTextField.Layer.AnchorPoint = new System.Drawing.PointF( 0, 0 );
                    ControlStyling.StyleTextField( AddressTextField, "Street address", ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
                    AddressTextField.Frame = new System.Drawing.RectangleF( 10, 0, parentSize.Width - SearchButton.Frame.Width - 5, SearchButton.Frame.Height );
                    AddressTextField.ReturnKeyType = UIReturnKeyType.Search;
                    AddressTextField.Delegate = new AddressDelegate( ) { PrimaryCell = this };
                    AddSubview( AddressTextField );

                    MapView = new MKMapView( );
                    MapView.Layer.AnchorPoint = new System.Drawing.PointF( 0, 0 );
                    MapView.Frame = new System.Drawing.RectangleF( 0, AddressTextField.Frame.Bottom, parentSize.Width, 250 );
                    AddSubview( MapView );

                    SearchResultsBanner = new UILabel( );
                    SearchResultsBanner.Layer.AnchorPoint = new System.Drawing.PointF( 0, 0 );
                    SearchResultsBanner.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
                    SearchResultsBanner.Text = "Search Results";
                    SearchResultsBanner.SizeToFit( );
                    SearchResultsBanner.Frame = new RectangleF( 0, MapView.Frame.Bottom, parentSize.Width, SearchResultsBanner.Frame.Height );
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
                    Title.Layer.AnchorPoint = PointF.Empty;
                    Title.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.Label_TextColor );
                    Title.BackgroundColor = UIColor.Clear;
                    Title.LineBreakMode = UILineBreakMode.TailTruncation;
                    AddSubview( Title );

                    Address = new UILabel( );
                    Address.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
                    Address.Layer.AnchorPoint = PointF.Empty;
                    Address.TextColor = Rock.Mobile.PlatformUI.Util.GetUIColor( ControlStylingConfig.TextField_PlaceholderTextColor );
                    Address.BackgroundColor = UIColor.Clear;
                    Address.LineBreakMode = UILineBreakMode.TailTruncation;
                    Address.Lines = 99;
                    AddSubview( Address );

                    Neighborhood = new UILabel( );
                    Neighborhood.Font = Rock.Mobile.PlatformSpecific.iOS.Graphics.FontManager.GetFont( ControlStylingConfig.Small_Font_Regular, ControlStylingConfig.Small_FontSize );
                    Neighborhood.Layer.AnchorPoint = PointF.Empty;
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

            float PendingPrimaryCellHeight { get; set; }
            float PendingCellHeight { get; set; }

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
                PrimaryTableCell.SearchResultsBanner.Hidden = true;
                PendingPrimaryCellHeight = PrimaryTableCell.MapView.Frame.Bottom;

                // additionally, set the default position for the map to whatever specified area.
                MKCoordinateRegion region = MKCoordinateRegion.FromDistance( new CLLocationCoordinate2D( 
                    CCVApp.Shared.Config.GroupFinderConfig.DefaultLatitude, 
                    CCVApp.Shared.Config.GroupFinderConfig.DefaultLongitude ), 
                    CCVApp.Shared.Config.GroupFinderConfig.LatitudeScale, 
                    CCVApp.Shared.Config.GroupFinderConfig.LongitudeScale );

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

                // if it HAS, reveal them and populate appropriately
                PrimaryTableCell.SearchResultsBanner.Hidden = false;
                if ( Parent.GroupEntries.Count > 0 )
                {
                    PrimaryTableCell.SearchResultsBanner.Text = "Search Results";
                }
                else
                {
                    PrimaryTableCell.SearchResultsBanner.Text = "No groups in your area.";
                }
                PendingPrimaryCellHeight = PrimaryTableCell.SearchResultsBanner.Frame.Bottom;

                // add an annotation for each position found in the group
                List<IMKAnnotation> annotations = new List<IMKAnnotation>();
                foreach ( GroupEntry entry in Parent.GroupEntries )
                {
                    MKPointAnnotation annotation = new MKPointAnnotation();
                    annotation.Coordinate = new CLLocationCoordinate2D( double.Parse( entry.Latitude ), double.Parse( entry.Longitude ) );
                    annotations.Add( annotation );
                }
                PrimaryTableCell.MapView.ShowAnnotations( annotations.ToArray( ), true );
            }

            public override int RowsInSection (UITableView tableview, int section)
            {
                return Parent.GroupEntries.Count + 1;
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
                    cell.Bounds = new RectangleF( cell.Bounds.X, cell.Bounds.Y, tableView.Bounds.Width, cell.Bounds.Height );

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
                cell.Title.Frame = new RectangleF( 10, 0, cell.Frame.Width - 5, cell.Title.Frame.Height );
                cell.Address.Frame = new RectangleF( 10, cell.Title.Frame.Bottom - 6, cell.Frame.Width - 5, cell.Address.Frame.Height + 5 );
                cell.Neighborhood.Frame = new RectangleF( 10, cell.Address.Frame.Bottom - 6, cell.Frame.Width - 5, cell.Neighborhood.Frame.Height + 5 );

                // add the seperator to the bottom
                cell.Seperator.Frame = new RectangleF( 0, cell.Neighborhood.Frame.Bottom - 1, cell.Bounds.Width, 1 );

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

        public class GroupEntry
        {
            public string Title { get; set; }
            public string Address { get; set; }

            public string Distance { get; set; }

            public string NeighborhoodArea { get; set; }

            public string Latitude { get; set; }
            public string Longitude { get; set; }
        }
        List<GroupEntry> GroupEntries { get; set; }

        GroupFinderViewController.TableSource GroupTableSource { get; set; }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.BackgroundColor = Rock.Mobile.PlatformUI.Util.GetUIColor( CCVApp.Shared.Config.ControlStylingConfig.BackgroundColor );

            GroupEntries = new List<GroupEntry>();

            GroupTableSource = new GroupFinderViewController.TableSource( this );
            GroupFinderTableView.Source = GroupTableSource;
            GroupFinderTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
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
                GroupEntries.Clear( );

                string street = "";
                string city = "";
                string state = "";
                string zip = "";
                bool result = Parsers.ParseAddress( address, ref street, ref city, ref state, ref zip );

                if ( result == false )
                {
                    SpringboardViewController.DisplayError( "Invalid Address", "Use the format 'street, city, state, zip' and try again." );
                }
                else
                {
                    RockApi.Instance.GetGroupsByLocation( CCVApp.Shared.Config.GeneralConfig.NeighborhoodGroupGeoFenceValueId, 
                        CCVApp.Shared.Config.GeneralConfig.NeighborhoodGroupValueId,
                        street, city, state, zip,
                        delegate(System.Net.HttpStatusCode statusCode, string statusDescription, List<Rock.Client.Group> model )
                        {
                            if ( Rock.Mobile.Network.Util.StatusInSuccessRange( statusCode ) == true )
                            {
                                // first thing we receive is the "area" group(s)
                                foreach ( Rock.Client.Group areaGroup in model )
                                {
                                    // in each area, there's an actual small group
                                    foreach( Rock.Client.Group smallGroup in areaGroup.Groups )
                                    {
                                        // get the group location out of the small group enumerator
                                        IEnumerator enumerator = smallGroup.GroupLocations.GetEnumerator( );
                                        enumerator.MoveNext( );
                                        Rock.Client.Location location = ((Rock.Client.GroupLocation)enumerator.Current).Location;

                                        // and of course, each group has a location
                                        GroupEntry entry = new GroupEntry( );
                                        entry.Title = smallGroup.Name;
                                        entry.Address = location.Street1 + "\n" + location.City + ", " + location.State + " " + location.PostalCode.Substring( 0, Math.Max( 0, location.PostalCode.IndexOf( '-' ) ) );
                                        entry.NeighborhoodArea = string.Format( "Part of the {0} Neighborhood", areaGroup.Name );
                                        entry.Latitude = location.Latitude.ToString( );
                                        entry.Longitude = location.Longitude.ToString( );

                                        GroupEntries.Add( entry );
                                    }
                                }
                            }

                            GroupTableSource.UpdateMap( );
                            GroupFinderTableView.ReloadData( );
                        } );
                }
            }
        }
	}
}
