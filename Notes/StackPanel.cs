using System;
using System.Collections.Generic;
using System.Xml;
using MonoTouch.UIKit;
using System.Drawing;
using Notes.Styles;

namespace Notes
{
	public class StackPanel : BaseControl
	{
		protected List<IUIControl> ChildControls { get; set; }
        protected RectangleF Bounds { get; set; }
        protected Alignment ChildHorzAlignment { get; set; }

		protected override void Initialize()
		{
			base.Initialize();

			ChildControls = new List<IUIControl>();

            ChildHorzAlignment = Alignment.Inherit;
		}

        public StackPanel (CreateParams parentParams, XmlReader reader)
		{
			Initialize();

			// check for attributes we support
			RectangleF bounds = new RectangleF();
            ParseCommonAttribs(reader, ref bounds);

            // take our parent's style but override with anything we set
            mStyle = parentParams.Style;
            Styles.Style.ParseStyleAttributes(reader, ref mStyle);


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

            // LEFT/TOP POSITIONING
            if(bounds.X < 1)
            {
                // convert % to pixel, based on parent's width
                bounds.X = parentParams.Width * bounds.X;
            }

            if(bounds.Y < 1)
            {
                // convert % to pixel, based on parent's width
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


            // Parse Child Controls
            bool finishedParsing = false;
            while (finishedParsing == false && reader.Read ()) 
			{
				switch(reader.NodeType)
				{
					case XmlNodeType.Element:
					{
						// let each child have our available width.
                        Style style = new Style();
                        style = mStyle;
                        style.mAlignment = ChildHorzAlignment;
                        IUIControl control = Parser.TryParseControl( new CreateParams(availableWidth, parentParams.Height, ref style), reader );
						if(control != null)
						{
                            ChildControls.Add(control);
						}
						break;
					}

					case XmlNodeType.EndElement:
					{
						// if we hit the end of our label, we're done.
						if(reader.Name == "StackPanel")
						{
                            finishedParsing = true;
						}

						break;
					}
				}
            }


            // layout all controls
            float yOffset = bounds.Y + topPadding; //vertically they should just stack

            // now we must center each control within the stack.
            foreach(IUIControl control in ChildControls)
            {
                RectangleF controlFrame = control.GetFrame();

                // horizontally position the controls according to their 
                // requested alignment
                Alignment controlAlignment = control.GetHorzAlignment();

                // adjust by our position
                float xAdjust = 0;
                switch(controlAlignment)
                {
                    case Alignment.Center: xAdjust = bounds.X + ((availableWidth / 2) - (controlFrame.Width / 2)); break;
                    case Alignment.Right:  xAdjust = bounds.X + (availableWidth - controlFrame.Width); break;
                    case Alignment.Left:   xAdjust = bounds.X; break;
                }

                // adjust the next sibling by yOffset
                control.AddOffset(xAdjust + leftPadding, yOffset);

                // and the next sibling must begin there
                yOffset = control.GetFrame().Bottom;
            }

            // we need to store our bounds. We cannot
            // calculate them on the fly because we
            // would lose any control defined offsets, which would throw everything off.
            bounds.Height = (yOffset - bounds.Y) + bottomPadding;
            Bounds = bounds;

            // store our debug frame
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
