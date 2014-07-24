
using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using RockMobile.Network;
using Notes;
using System.Collections.Generic;

namespace CCVApp
{
	// create a subclass of UIScrollView so we can intercept its touch events
	public class CustomScrollView : UIScrollView
	{
		public UIViewController Interceptor { get; set; }

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			if(Interceptor != null)
			{
				Interceptor.TouchesEnded(touches, evt);
			}
		}
	}

	public partial class NotesViewController : UIViewController
	{
		UIActivityIndicatorView Indicator { get; set; }
		UIButton RefreshButton { get; set; }
		CustomScrollView UIScrollView { get; set; }

		// Notes members
		bool RefreshingNotes { get; set; }
		Note Note { get; set; }
        String NoteXml { get; set; }

		public NotesViewController () : base ("NotesViewController", null)
		{
		}

		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

        public override void ViewWillLayoutSubviews()
        {
            base.ViewWillLayoutSubviews();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            UIScrollView.Frame = new RectangleF(0, 0, View.Bounds.Width, View.Bounds.Height);

            Indicator.Layer.Position = new PointF(View.Bounds.Width / 2, View.Bounds.Height / 2);

            RefreshButton.Layer.Position = new PointF(View.Bounds.Width / 2, RefreshButton.Bounds.Height + 10);

            // re-create our notes with the new dimensions
            CreateNotes();
        }

		public override void ViewDidLoad ()
		{
            base.ViewDidLoad ();

			UIScrollView = new CustomScrollView();
			UIScrollView.Interceptor = this;
            UIScrollView.Frame = View.Frame;
            UIScrollView.BackgroundColor = UIColor.Black;
			View.AddSubview(UIScrollView);

			// add a busy indicator
			Indicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.White);
			UIScrollView.AddSubview(Indicator);

			// add a refresh button for debugging
			RefreshButton = UIButton.FromType(UIButtonType.System);;
			RefreshButton.SetTitle("Refresh", UIControlState.Normal);
            RefreshButton.SizeToFit();

			// if they tap the refresh button, refresh the list
			RefreshButton.TouchUpInside += (object sender, EventArgs e) => 
			{
				CreateNotes();
			};
			UIScrollView.AddSubview(RefreshButton);
		}

		public override void TouchesEnded(NSSet touches, UIEvent evt)
		{
			base.TouchesEnded(touches, evt);

			UITouch touch = touches.AnyObject as UITouch;
			if(touch != null)
			{
                if(Note != null)
                {
                    Note.TouchesEnded(touch.LocationInView(UIScrollView));
                }
			}
		}

		public void DestroyNotes()
		{
			if(Note != null)
			{
				Note.Destroy();
                Note = null;
			}
		}

		public void CreateNotes()
		{
			if(RefreshingNotes == false)
			{
				RefreshingNotes = true;

				DestroyNotes();

				// show a busy indicator
				Indicator.StartAnimating();

				// grab the notes (clearly this should not be hard-coded)
				HttpWebRequest.Instance.MakeAsyncRequest("http://www.jeredmcferron.com/test_note.xml", OnCompletion);
			}
		}

        public void OnCompletion(bool result, Dictionary<String, String> responseHeaders, String body)
        {
            if(result)
            {
                NoteXml = body;

                // the body is raw XML, so pass it on in to the notes generator
                try
                {
                    Note.CreateNote(NoteXml, OnPreReqsComplete);
                }
                catch(Exception e)
                {
                    ReportException(e);
                }
            }
            else
            {
                ReportException(new InvalidOperationException(String.Format("Could not download sermon notes.")));
            }
        }

        protected void ReportException(Exception e)
        {
            new NSObject().InvokeOnMainThread(delegate
                {
                    // explain that we couldn't generate notes
                    UIAlertView alert = new UIAlertView();
                    alert.Title = "Error";
                    alert.Message = "Failed to build notes. " + e.Message;
                    alert.AddButton("Ok");
                    alert.Show();

                    FinishNotesCreation();
                });
        }

        protected void OnPreReqsComplete(Note note, Exception e)
        {
            if(e != null)
            {
                ReportException(e);
            }
            else
            {
                InvokeOnMainThread(delegate
                    {
                        Note = note;

                        try
                        {
                            Note.Create(UIScrollView.Bounds.Width, UIScrollView.Bounds.Height, NoteXml);

                            //todo: we can't pass in an iOS type like this...
                            Note.AddToView(this.UIScrollView);

                            // take the requested background color
                            UIScrollView.BackgroundColor = Notes.Styles.Style.GetUIColor(ControlStyles.mMainNote.mBackgroundColor.Value);

                            // update the height of the scroll view to fit all content
                            RectangleF frame = Note.GetFrame();
                            UIScrollView.ContentSize = new SizeF(UIScrollView.Bounds.Width, frame.Size.Height + (UIScrollView.Bounds.Height / 2));

                            FinishNotesCreation();
                        }
                        catch(Exception ex)
                        {
                            ReportException(ex);
                        }
                    });
            }
        }

        void FinishNotesCreation ()
        {
            Indicator.StopAnimating();

            // flag that we're clear to refresh again
            RefreshingNotes = false;
        }
	}
}

