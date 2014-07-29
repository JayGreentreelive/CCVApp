
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
            String noteXml = null;
            String styleSheetXml = null;
            if(Note != null)
            {
                noteXml = Note.NoteXml;
                styleSheetXml = ControlStyles.StyleSheetXml;
            }
            CreateNotes(noteXml, styleSheetXml);
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
                CreateNotes(null, null);
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
				Note.Destroy(null);
                Note = null;
			}
		}

        public void CreateNotes(String noteXml, String styleSheetXml)
		{
			if(RefreshingNotes == false)
			{
				RefreshingNotes = true;

				DestroyNotes();

				// show a busy indicator
				Indicator.StartAnimating();

                // if we don't have BOTH xml strings, re-download
                if(noteXml == null || styleSheetXml == null)
                {
                    HttpWebRequest.Instance.MakeAsyncRequest("http://www.jeredmcferron.com/sample_note.xml", (Exception ex, Dictionary<string, string> responseHeaders, string body) => 
                        {
                            if(ex == null)
                            {
                                HandleNotePreReqs(body, null);
                            }
                            else
                            {
                                ReportException("Failed to download Sermon Notes", ex);
                            }
                        });
                }
                else
                {
                    // if we DO have both, go ahead and create with them.
                    HandleNotePreReqs(noteXml, styleSheetXml);
                }
			}
		}

        protected void HandleNotePreReqs(String noteXml, String styleXml)
        {
            try
            {
                Note.HandlePreReqs(noteXml, styleXml, OnPreReqsComplete);
            }
            catch(Exception e)
            {
                ReportException("Note PreReqs Failed", e);
            }
        }

        protected void OnPreReqsComplete(Note note, Exception e)
        {
            if(e != null)
            {
                ReportException("Note PreReqs Failed", e);
            }
            else
            {
                InvokeOnMainThread(delegate
                    {
                        Note = note;

                        try
                        {
                            Note.Create(UIScrollView.Bounds.Width, UIScrollView.Bounds.Height);

                            //todo: we can't pass in an iOS type like this...
                            Note.AddToView(this.UIScrollView);

                            // take the requested background color
                            //TODO: Not acceptable to leave this!
                            UIScrollView.BackgroundColor = Notes.PlatformUI.iOSLabel.GetUIColor(ControlStyles.mMainNote.mBackgroundColor.Value);

                            // update the height of the scroll view to fit all content
                            RectangleF frame = Note.GetFrame();
                            UIScrollView.ContentSize = new SizeF(UIScrollView.Bounds.Width, frame.Size.Height + (UIScrollView.Bounds.Height / 2));

                            FinishNotesCreation();
                        }
                        catch(Exception ex)
                        {
                            ReportException("Note Creation Failed", ex);
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

        protected void ReportException(String errorMsg, Exception e)
        {
            new NSObject().InvokeOnMainThread(delegate
                {
                    // explain that we couldn't generate notes
                    UIAlertView alert = new UIAlertView();
                    alert.Title = "Note Error";
                    alert.Message = errorMsg + "Exception: " + e.Message;
                    alert.AddButton("Ok");
                    alert.Show();

                    FinishNotesCreation();
                });
        }
	}
}

