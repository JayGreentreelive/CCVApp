using System;
using MonoTouch.UIKit;
using System.Xml;
using System.Drawing;

namespace Notes
{
	public class Quote : BaseControl
	{
		protected UILabel Quote_PlatformLabel { get; set; }
		protected UILabel Citation_PlatformLabel { get; set; }
        protected RectangleF Bounds { get; set; }

		protected override void Initialize()
		{
			base.Initialize();

			// TODO: Update this to be like iBeacon, where it's platform abstracted
			Quote_PlatformLabel = new MonoTouch.UIKit.UILabel();
			Citation_PlatformLabel = new MonoTouch.UIKit.UILabel();

			// these could be optional in XML
			Quote_PlatformLabel.Layer.AnchorPoint = new PointF(0, 0);
			Quote_PlatformLabel.TextAlignment = UITextAlignment.Left;
			Quote_PlatformLabel.LineBreakMode = UILineBreakMode.WordWrap;
			Quote_PlatformLabel.Lines = 0;

			Citation_PlatformLabel.Layer.AnchorPoint = new PointF(0, 0);
			Citation_PlatformLabel.TextAlignment = UITextAlignment.Left;
		}

        public Quote (CreateParams parentParams, XmlReader reader)
		{
			Initialize();

			// check for attributes we support
			RectangleF bounds = new RectangleF();
            base.ParseCommonAttribs(reader, ref bounds);

            // take our parent's style but override with anything we set
            mStyle = parentParams.Style;
            Styles.Style.ParseStyleAttributesWithDefaults(reader, ref mStyle, ref ControlStyles.mQuote);


            // create the font that either we or our parent defined
            Quote_PlatformLabel.Font = UIFont.FromName(mStyle.mFont.mName, mStyle.mFont.mSize.Value);
            Citation_PlatformLabel.Font = UIFont.FromName(mStyle.mFont.mName, mStyle.mFont.mSize.Value);

            UIColor fontColor = Styles.Style.GetUIColor(mStyle.mFont.mColor.Value);

            Quote_PlatformLabel.TextColor = fontColor;
            Citation_PlatformLabel.TextColor = fontColor;

            if(mStyle.mBackgroundColor.HasValue)
            {
                Quote_PlatformLabel.BackgroundColor = Styles.Style.GetUIColor(mStyle.mBackgroundColor.Value);
                Citation_PlatformLabel.BackgroundColor = Styles.Style.GetUIColor(mStyle.mBackgroundColor.Value);
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

            // Set the bounds and position for the frame (except Height, which we'll calculate based on the text)
            Quote_PlatformLabel.Frame = new RectangleF(bounds.X + leftPadding, bounds.Y + topPadding, availableWidth, 0);


			// expect the citation to be an attribute
			string result = reader.GetAttribute("Citation");
			if(String.IsNullOrEmpty(result) == false)
			{
				// set and resize the citation to fit
				Citation_PlatformLabel.Text = result;
				Citation_PlatformLabel.SizeToFit();
			}

            bool finishedScripture = false;
            while (finishedScripture == false && reader.Read ()) 
			{
				switch(reader.NodeType)
				{
					case XmlNodeType.Element:
					{
						switch(reader.Name)
						{
							case "Text":
							{
								Quote_PlatformLabel.Text = reader.ReadElementContentAsString();
								break;
							}
						}
						break;
					}

                    case XmlNodeType.Text:
                    {
                        // support text as embedded in the element
                        Quote_PlatformLabel.Text = reader.Value.Replace(System.Environment.NewLine, "").Trim(' ');

                        break;
                    }


					case XmlNodeType.EndElement:
					{
						// if we hit the end of our label, we're done.
						if(reader.Name == "Quote")
						{
							finishedScripture = true;
						}

						break;
					}
				}
			}

            // We forced the text to fit within the width specified above, so this will simply calculate the height.
            Quote_PlatformLabel.SizeToFit();

			// now that we know our text size, we can adjust the citation
            Citation_PlatformLabel.Frame = new RectangleF(Quote_PlatformLabel.Frame.Left, 
                                                          Quote_PlatformLabel.Frame.Bottom, 
                                                          Quote_PlatformLabel.Frame.Width,
                                                          Citation_PlatformLabel.Frame.Height);
            Citation_PlatformLabel.TextAlignment = UITextAlignment.Right;


            // get a bounding frame for the quote and citation
            RectangleF frame = Parser.CalcBoundingFrame(Quote_PlatformLabel.Frame, Citation_PlatformLabel.Frame);

            // reintroduce vertical padding
            bounds.Height = frame.Height + topPadding + bottomPadding;

            // and store that as our bounds
            Bounds = bounds;
            base.DebugFrameView.Frame = Bounds;
		}

		public override void AddOffset(float xOffset, float yOffset)
		{
			base.AddOffset(xOffset, yOffset);

            Quote_PlatformLabel.Layer.Position = new PointF (Quote_PlatformLabel.Layer.Position.X + xOffset, 
                                                             Quote_PlatformLabel.Layer.Position.Y + yOffset);

            Citation_PlatformLabel.Layer.Position = new PointF (Citation_PlatformLabel.Layer.Position.X + xOffset, 
                                                                Citation_PlatformLabel.Layer.Position.Y + yOffset);

            // update our bounds by the new offsets.
            Bounds = new RectangleF(Bounds.X + xOffset, Bounds.Y + yOffset, Bounds.Width, Bounds.Height);
            base.DebugFrameView.Frame = Bounds;
		}

		public override void AddToView(object obj)
		{
			base.AddToView(obj);

			((UIView)obj).AddSubview(Quote_PlatformLabel);
			((UIView)obj).AddSubview(Citation_PlatformLabel);
		}

		public override void RemoveFromView()
		{
			base.RemoveFromView();

			Quote_PlatformLabel.RemoveFromSuperview();
			Citation_PlatformLabel.RemoveFromSuperview();
		}

		public override System.Drawing.RectangleF GetFrame()
		{
			return Bounds;
		}
	}
}
