using System;
using System.Collections.Generic;
using System.Xml;
using MonoTouch.UIKit;
using System.Drawing;
using System.IO;

namespace Notes
{
	public class Note
	{
        public delegate void OnPreReqsComplete(Note note, Exception e);

		protected List<IUIControl> ChildControls { get; set; }

        public static void CreateNote(string noteXml, OnPreReqsComplete onPreReqsComplete)
        {
            // now use a reader to get each element
            XmlReader reader = XmlReader.Create (new StringReader(noteXml));

            string styleSheetUrl = "";

            bool finishedReading = false;
            while(finishedReading == false && reader.Read())
            {
                // expect the first element to be "Note"
                switch(reader.NodeType)
                {
                    case XmlNodeType.Element:
                    {
                        if(reader.Name == "Note")
                        {
                            styleSheetUrl = reader.GetAttribute("StyleSheet");
                            if(styleSheetUrl == null)
                            {
                                throw new InvalidDataException("Could not find attribute 'StyleSheet'. This should be a URL pointing to the style to use.");
                            }
                        }
                        else
                        {
                            throw new InvalidDataException(String.Format("Expected root element to be <Note>. Found <{0}>", reader.Name));
                        }

                        finishedReading = true;
                        break;
                    }
                }
            }

            // Parse the styles. We cannot go any further until this is finished.
            ControlStyles.Initialize(styleSheetUrl, (Exception e) =>
                {
                    // We don't just create the note here because the parent
                    // might need to change threads before creating UI objects
                    onPreReqsComplete(new Note(), e);
                });
        }

        public void Create(float parentWidth, float parentHeight, string noteXml)
        {
            // create a child control list
            ChildControls = new List<IUIControl>();

            // setup a blank style so we don't override anything.
            Styles.Style style = new Styles.Style();
            style.Initialize();
            style.mAlignment = Styles.Alignment.Left;

            // now use a reader to get each element
            XmlReader reader = XmlReader.Create (new StringReader(noteXml));

            // begin reading the xml stream
            while (reader.Read ()) 
            {
                switch(reader.NodeType)
                {
                    // there should only be ui controls within a note
                    case XmlNodeType.Element:
                    {
                        // don't worry about our root node.
                        if(reader.Name != "Note")
                        {
                            // pass an empty style because we just don't care.
                            IUIControl control = Parser.TryParseControl(new BaseControl.CreateParams(parentWidth, parentHeight, ref style), reader);
                            ChildControls.Add(control);
                        }
                        break;
                    }
                }
            }
        }

		public void Destroy ()
		{
			// release references to our UI objects
			if(ChildControls != null)
			{
				foreach(IUIControl uiControl in ChildControls)
				{
					uiControl.RemoveFromView();
				}

				// and clear our UI list
				ChildControls.Clear();
			}
		}

		public void AddToView (UIView view)
		{
			foreach(IUIControl uiControl in ChildControls)
			{
				uiControl.AddToView(view);
			}
		}

		public RectangleF GetFrame()
		{
			// create an inverse region to guarantee we use our first child's bounds.
			RectangleF frame = new RectangleF(65000, 65000, -65000, -65000);

			// for each child control
			foreach(IUIControl control in ChildControls)
			{
				// enlarge our frame by the current frame and the next child
				frame = Parser.CalcBoundingFrame(frame, control.GetFrame());
			}

			return frame;
		}

		public void TouchesEnded (PointF touch)
		{
			// notify all controls
			foreach(IUIControl control in ChildControls)
			{
				control.TouchesEnded(touch);
			}
		}
	}
}
