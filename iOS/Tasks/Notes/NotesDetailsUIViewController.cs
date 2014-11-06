using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.CodeDom.Compiler;
using CCVApp.Shared.Notes.Model;
using System.Collections.Generic;
using System.Drawing;

namespace iOS
{
    partial class NotesDetailsUIViewController : TaskUIViewController
	{
        public class TableSource : UITableViewSource 
        {
            NotesDetailsUIViewController Parent { get; set; }

            List<Series.Message> Messages { get; set; }
            List<UIImageView> MessageImage { get; set; }

            string cellIdentifier = "TableCell";

            float PendingCellHeight { get; set; }

            public TableSource (NotesDetailsUIViewController parent, List<Series.Message> messagesList, List<UIImageView> messageImage )
            {
                Parent = parent;

                Messages = messagesList;

                MessageImage = messageImage;
            }

            public override int RowsInSection (UITableView tableview, int section)
            {
                return Messages.Count;
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

                cell.TextLabel.Text = Messages[ indexPath.Row ].Name + "\n" + Messages[ indexPath.Row ].Description;
                cell.TextLabel.Lines = 0;
                cell.TextLabel.LineBreakMode = UILineBreakMode.WordWrap;
                cell.TextLabel.SizeToFit( );

                PendingCellHeight = cell.Frame.Height;

                return cell;
            }
        }

        public Series Series { get; set; }

		public NotesDetailsUIViewController (IntPtr handle) : base (handle)
		{
		}

        public override void ViewDidLoad()
        {
            base.ViewDidLoad( );

            TableSource source = new TableSource( this, Series.Messages, null );
            SeriesTable.Source = source;
            SeriesTable.ReloadData( );
        }

        public void RowClicked( int row )
        {
            // for now, let the note name be the previous saturday
            /*DateTime time = DateTime.UtcNow;

            // if it's not saturday, find the date of the past saturday
            if( time.DayOfWeek != DayOfWeek.Saturday )
            {
                time = time.Subtract( new TimeSpan( (int)time.DayOfWeek + 1, 0, 0, 0 ) );
            }

            NotesViewController.NotePresentableName = string.Format( "Sermon Note - {0}.{1}.{2}", time.Month, time.Day, time.Year );
            #if DEBUG
            NotesViewController.NoteName = "sample_note";
            #else
            NotesViewController.NoteName = string.Format("{0}_{1}_{2}_{3}", CCVApp.Shared.Config.Note.NamePrefix, time.Month, time.Day, time.Year );
            #endif
            //

            // set our current page as root
            parentViewController.PushViewController(NotesViewController, false);

            NotesViewController.MakeActive( );*/


            /*NotesDetailsUIViewController viewController = Storyboard.InstantiateViewController( "NotesDetailsUIViewController" ) as NotesDetailsUIViewController;
            viewController.Series = Series[ row ];
            Task.PerformSegue( this, viewController );*/

            NotesViewController viewController = new NotesViewController( );
            viewController.NotePresentableName = string.Format( "Message - {0}", Series.Messages[ row ].Name );
            viewController.NoteName = Series.Messages[ row ].NoteUrl;

            Task.PerformSegue( this, viewController );
        }
	}
}
