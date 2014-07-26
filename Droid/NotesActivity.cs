
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using RockMobile.Network;
using Notes;
using System.Drawing;

namespace Droid
{
    [Activity( Label = "NotesActivity" )]            
    public class NotesActivity : Activity
    {
        ScrollView ScrollView { get; set; }

        // Notes members
        bool RefreshingNotes { get; set; }
        Note Note { get; set; }
        String NoteXml { get; set; }

        protected override void OnCreate( Bundle bundle )
        {
            base.OnCreate( bundle );

            Notes.PlatformUI.PlatformCommonUI.Context = this;

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.Notes);

            ScrollView = FindViewById<ScrollView> (Resource.Id.scrollView);

            // grab the notes (clearly this should not be hard-coded)
            HttpWebRequest.Instance.MakeAsyncRequest("http://www.jeredmcferron.com/sample_note.xml", OnCompletion);
        }

        void OnCompletion( bool result, Dictionary<string, string> responseHeaders, string body )
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

        }

        protected void OnPreReqsComplete(Note note, Exception e)
        {
            if(e != null)
            {
                ReportException(e);
            }
            else
            {
                RunOnUiThread(delegate
                    {
                        Note = note;

                        try
                        {
                            Note.Create(ScrollView.Width, this.Resources.DisplayMetrics.HeightPixels, NoteXml);

                            RelativeLayout layout = new RelativeLayout(this);
                            ScrollView.AddView(layout);
                            Note.AddToView(layout);

                            /*TextView text = new TextView(this);
                            text.Text = "JERED";
                            text.Id = 5;
                            text.SetY(100);
                            layout.AddView(text);

                            text = new TextView(this);
                            text.Text = "TESTING AGAIN";
                            text.SetX(125);
                            text.SetY(100);
                            text.Id = 6;
                            layout.AddView(text);

                            ScrollView.LayoutParameters.Height = 960;
                            layout.LayoutParameters.Height = 960;*/

                            // take the requested background color
                            //TODO: Not acceptable to leave this!
                            //ScrollView.BackgroundColor = Notes.PlatformUI.iOSLabel.GetUIColor(ControlStyles.mMainNote.mBackgroundColor.Value);

                            // update the height of the scroll view to fit all content
                            RectangleF frame = Note.GetFrame();

                            int scrollFrameHeight = (int)frame.Size.Height + (this.Resources.DisplayMetrics.HeightPixels / 2);

                            ScrollView.LayoutParameters.Height = scrollFrameHeight;
                            layout.LayoutParameters.Height = scrollFrameHeight;

                            //UIScrollView.ContentSize = new SizeF(UIScrollView.Bounds.Width, frame.Size.Height + (UIScrollView.Bounds.Height / 2));

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
            //Indicator.StopAnimating();

            // flag that we're clear to refresh again
            RefreshingNotes = false;
        }
    }
}

