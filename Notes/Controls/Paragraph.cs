using System;
using System.Collections.Generic;
using System.Xml;
using System.Drawing;
using Notes.Styles;

namespace Notes
{
	public class Paragraph : BaseControl
	{
		protected List<IUIControl> ChildControls { get; set; }
        protected Alignment ChildHorzAlignment { get; set; }
        protected RectangleF Bounds { get; set; }

		protected override void Initialize()
		{
			base.Initialize();

			ChildControls = new List<IUIControl>();

            ChildHorzAlignment = Alignment.Inherit;
		}

        public Paragraph (CreateParams parentParams, XmlReader reader)
		{
			Initialize();

			// check for attributes we support
			RectangleF bounds = new RectangleF();
            ParseCommonAttribs(reader, ref bounds);

            // take our parent's style but override with anything we set
            mStyle = parentParams.Style;
            Styles.Style.ParseStyleAttributesWithDefaults(reader, ref mStyle, ref ControlStyles.mParagraph);


            // now read what our children's alignment should be
            // check for alignment
            string result = reader.GetAttribute("ChildAlignment");
            if(String.IsNullOrEmpty(result) == false)
            {
                switch(result)
                {
                    case "Left":   ChildHorzAlignment = Alignment.Left; break;
                    case "Right":  ChildHorzAlignment = Alignment.Right; break;
                    case "Center": ChildHorzAlignment = Alignment.Center; break;
                    default:       ChildHorzAlignment = mStyle.mAlignment.Value; break;
                }
            }
            else
            {
                // if it wasn't specified, use OUR alignment.
                ChildHorzAlignment = mStyle.mAlignment.Value;
            }

            // if our left position is requested as a %, then that needs to be % of parent width
            if(bounds.X < 1)
            {
                bounds.X = parentParams.Width * bounds.X;
            }

            // if our top position is requested as a %, then that needs to be % of parent width
            if(bounds.Y < 1)
            {
                bounds.Y = parentParams.Height * bounds.Y;
            }

            //WIDTH
            if(bounds.Width == 0)
            {
                // if 0, just take the our parents width
                bounds.Width = Math.Max(1, parentParams.Width - bounds.X);
            }
            // if < 1 it's a percent and we should convert
            else if(bounds.Width <= 1)
            {
                bounds.Width = Math.Max(1, parentParams.Width - bounds.X) * bounds.Width;
            }

            // PADDING
            float leftPadding   = Styles.Style.GetStyleValue(mStyle.mPaddingLeft, parentParams.Width);
            float rightPadding  = Styles.Style.GetStyleValue(mStyle.mPaddingRight, parentParams.Width);
            float topPadding    = Styles.Style.GetStyleValue(mStyle.mPaddingTop, parentParams.Height);
            float bottomPadding = Styles.Style.GetStyleValue(mStyle.mPaddingBottom, parentParams.Height);

            // now calculate the available width based on padding. (Don't actually change our width)
            float availableWidth = bounds.Width - leftPadding - rightPadding;

            bool trimmedLeadingSpace = false;

			bool finishedReading = false;
            while (finishedReading == false && reader.Read ()) 
			{
				switch(reader.NodeType)
				{
					case XmlNodeType.Element:
					{
                        IUIControl control = Parser.TryParseControl( new CreateParams(availableWidth, parentParams.Height, ref mStyle), reader );
						if(control != null)
						{
                            // only allow RevealBoxes as children.
                            if(control as RevealBox == null && control as TextInput == null)
                            {
                                throw new InvalidOperationException("RevealBox and TextInput are the only supported children of Paragraph.");
                            }
							ChildControls.Add(control);
						}
						break;
					}

                    case XmlNodeType.Text:
                    {
                        // grab the text. Remove leading/trailing spaces and lines
                        string text = reader.Value.Replace(System.Environment.NewLine, "");

                        // Remove leading spaces only on the very first set of text within this paragraph.
                        // This allows authors to put the Paragraph tag on a seperate line from the content.
                        if(trimmedLeadingSpace == false)
                        {
                            text = text.TrimStart(' ');
                            trimmedLeadingSpace = true;
                        }

                        // now break it into words so we can do word wrapping
                        string[] words = text.Split(' ');
                        foreach(string word in words)
                        {
                            // create labels out of each one
                            Label textLabel = new Label(new CreateParams(availableWidth, parentParams.Height, ref mStyle), word + " ");

                            ChildControls.Add(textLabel);
                        }
                        break;
                    }

					case XmlNodeType.EndElement:
					{
						// if we hit the end of our label, we're done.
						if(reader.Name == "Paragraph")
						{
							finishedReading = true;
						}
						break;
					}
				}
			}


            // layout all controls
            // paragraphs are tricky. 
            // We need to lay out controls horizontally and wrap when we run out of room. 

            // To align, we need to keep track of each "row". When the row is full,
            // we calculate its width, and then adjust each item IN that row so
            // that the row is centered within the max width of the paragraph.
            // The max width of the paragraph is defined as the widest row.

            // maintain a list of all our rows so that once they are all generated,
            // we can align them based on the widest row.
            float maxRowWidth = 0;
            List< List<IUIControl> > rowList = new List< List<IUIControl> >();

            // track where within a row we need to start a control
            float rowRemainingWidth = availableWidth;
            float startingX = bounds.X + leftPadding;

            // always store the last placed control's height so that should 
            // our NEXT control need to wrap, we know how far down to wrap.
            float yOffset = bounds.Y + topPadding;
            float lastControlHeight = 0;
            float rowWidth = 0;

            //Create our first row and put it in our list
            List< IUIControl > currentRow = new List<IUIControl>();
            rowList.Add(currentRow);

            foreach(IUIControl control in ChildControls)
            {
                RectangleF controlFrame = control.GetFrame();

                // if there is NOT enough room on this row for the next control
                if( rowRemainingWidth < controlFrame.Width )
                {
                    // advance to the next row
                    yOffset += lastControlHeight;

                    // Reset values for the new row
                    rowRemainingWidth = availableWidth;
                    startingX         = bounds.X + leftPadding;
                    lastControlHeight = 0;
                    rowWidth          = 0;

                    currentRow = new List<IUIControl>();
                    rowList.Add(currentRow);
                }

                // Add this next control to the current row
                currentRow.Add(control);

                // position this control appropriately
                control.AddOffset(startingX, yOffset);

                // update so the next child begins beyond this one.
                // also reduce the available width by this control's.
                rowWidth          += controlFrame.Width;
                startingX         += controlFrame.Width; //Increment startingX so the next control is placed after this one.
                rowRemainingWidth -= controlFrame.Width; //Reduce the available width by what this control took.
                lastControlHeight  = controlFrame.Height > lastControlHeight ? controlFrame.Height : lastControlHeight; //Store the height of the tallest control on this row.

                // track the widest row
                maxRowWidth = rowWidth > maxRowWidth ? rowWidth : maxRowWidth;
            }


            // Now that we know the widest row, align all the rows
            foreach(List<IUIControl> row in rowList)
            {
                AlignRow(bounds, row, maxRowWidth);
            }


            // Build our final frame that determines our dimensions
            RectangleF frame = new RectangleF(65000, 65000, -65000, -65000);

            // for each child control
            foreach(IUIControl control in ChildControls)
            {
                // enlarge our frame by the current frame and the next child
                frame = Parser.CalcBoundingFrame(frame, control.GetFrame());
            }

            frame.Y = bounds.Y;
            frame.X = bounds.X;
            frame.Height += bottomPadding + topPadding; //add in padding
            frame.Width = bounds.Width;

            Bounds = frame;
            base.DebugFrameView.Frame = Bounds;
		}

        void AlignRow (RectangleF bounds, List<IUIControl> currentRow, float maxWidth)
        {
            // Determine the row's width and height (Height is defined as the tallest control on this line)
            float rowHeight = 0;
            float rowWidth  = 0;

            foreach(IUIControl rowControl in currentRow)
            {
                RectangleF controlFrame  = rowControl.GetFrame();

                rowWidth  += controlFrame.Width;
                rowHeight  = rowHeight > controlFrame.Height ? rowHeight : controlFrame.Height;
            }

            // the amount each control in the row should adjust is the 
            // difference of paragraph width (which is defined by the max row width)
            // and this row's width.
            float xRowAdjust = 0;
            switch(ChildHorzAlignment)
            {
                // JHM Note 7-24: Yesterday I changed bounds.Width to be MaxRowWidth. I can't remember why.
                // Today Jon found that if you put a single line of text, you can't align it because its
                // width is the max width, which causes no movement in the paragraph.
                // I made it bounds.width again and can't find any problems with it, but I'm leaving the old calculation
                // here just in case we need it again. :-\
                case Alignment.Right:  xRowAdjust = (bounds.Width - rowWidth); break;
                case Alignment.Center: xRowAdjust = ((bounds.Width / 2) - (rowWidth / 2)); break;
                case Alignment.Left:   xRowAdjust = 0; break;
            }

            // Now adjust each control to be aligned correctly on X and Y
            foreach(IUIControl rowControl in currentRow)
            {
                // vertically center all items within the row.
                float yAdjust = rowHeight / 2 - (rowControl.GetFrame().Height / 2);

                // set their correct X offset
                rowControl.AddOffset(xRowAdjust, yAdjust);
            }
        }

		public override void AddOffset(float xOffset, float yOffset)
		{
			base.AddOffset(xOffset, yOffset);

			// position each interactive label relative to ourselves
			foreach(IUIControl control in ChildControls)
			{
				control.AddOffset(xOffset, yOffset);
			}

            // update our bounds by the new offsets.
            Bounds = new RectangleF(Bounds.X + xOffset, Bounds.Y + yOffset, Bounds.Width, Bounds.Height);
            base.DebugFrameView.Frame = Bounds;
		}

		public override void TouchesEnded (PointF touch)
		{
			// let each child handle it
			foreach(IUIControl control in ChildControls)
			{
				control.TouchesEnded(touch);
			}
		}

		public override void AddToView(object obj)
		{
			base.AddToView(obj);

			// let each child do the same thing
			foreach(IUIControl control in ChildControls)
			{
				control.AddToView(obj);
			}
		}

		public override void RemoveFromView()
		{
			base.RemoveFromView();

			// let each child do the same thing
			foreach(IUIControl control in ChildControls)
			{
				control.RemoveFromView();
			}
		}

		public override RectangleF GetFrame()
		{
			return Bounds;
		}
	}
}
