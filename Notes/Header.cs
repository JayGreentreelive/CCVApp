using System;
using System.Xml;
using MonoTouch.UIKit;
using System.Drawing;

namespace Notes
{
	public class Header : BaseControl
	{
		protected UILabel Title_PlatformLabel { get; set; }
		protected UILabel Date_PlatformLabel { get; set; }
		protected UILabel Speaker_PlatformLabel { get; set; }

		protected override void Initialize()
		{
			base.Initialize();

			// TODO: Update this to be like iBeacon, where it's platform abstracted
			Title_PlatformLabel = new MonoTouch.UIKit.UILabel();
			Title_PlatformLabel.Layer.AnchorPoint = new PointF(0, 0);
			Title_PlatformLabel.TextAlignment = UITextAlignment.Left;
			Title_PlatformLabel.Font = UIFont.FromName("Helvetica-Bold", 16f);

			Date_PlatformLabel = new MonoTouch.UIKit.UILabel();
			Date_PlatformLabel.Layer.AnchorPoint = new PointF(0, 0);
			Date_PlatformLabel.TextAlignment = UITextAlignment.Left;
			Date_PlatformLabel.Font = UIFont.FromName("Helvetica", 12f);

			Speaker_PlatformLabel = new MonoTouch.UIKit.UILabel();
			Speaker_PlatformLabel.Layer.AnchorPoint = new PointF(0, 0);
			Speaker_PlatformLabel.TextAlignment = UITextAlignment.Left;
			Speaker_PlatformLabel.Font = UIFont.FromName("Helvetica", 12f);
		}

        public Header (CreateParams parentParams, XmlReader reader)
		{
			Initialize();

			// check for attributes we support
			RectangleF bounds = new RectangleF();
            ParseCommonAttribs(reader, ref bounds);

            // take our parent's style but override with anything we set
            mStyle = parentParams.Style;
            Styles.Style.ParseStyleAttributesWithDefaults(reader, ref mStyle, ref ControlStyles.mHeader);

            // create the font that either we or our parent defined
            Title_PlatformLabel.Font = UIFont.FromName(mStyle.mFont.mName, mStyle.mFont.mSize.Value);
            Date_PlatformLabel.Font = UIFont.FromName(mStyle.mFont.mName, mStyle.mFont.mSize.Value);
            Speaker_PlatformLabel.Font = UIFont.FromName(mStyle.mFont.mName, mStyle.mFont.mSize.Value);

            UIColor fontColor = Styles.Style.GetUIColor(mStyle.mFont.mColor.Value);

            Title_PlatformLabel.TextColor = fontColor;
            Date_PlatformLabel.TextColor = fontColor;
            Speaker_PlatformLabel.TextColor = fontColor;


            //TODO: Stop hardcoding offsets for the header
			float xPos = bounds.X;
			float yPos = bounds.Y;

			// position all the header elements
			Title_PlatformLabel.Layer.Position = new PointF(xPos, yPos);
			Date_PlatformLabel.Layer.Position = new PointF(xPos, yPos + 15);
			Speaker_PlatformLabel.Layer.Position = new PointF(xPos, yPos + 30);


			bool finishedHeader = false;

            while(finishedHeader == false && reader.Read())
			{
				// look for the next tag type
				switch(reader.NodeType)
				{
				// we expect elements
					case XmlNodeType.Element:
					{
						// determine which element it is and setup appropriately
						switch(reader.Name)
						{
							case "Title":
							{
								Title_PlatformLabel.Text = reader.ReadElementContentAsString();
								break;
							}

							case "Date":
							{
								Date_PlatformLabel.Text = reader.ReadElementContentAsString();

								break;
							}

							case "Speaker":
							{
								Speaker_PlatformLabel.Text = reader.ReadElementContentAsString();
								break;
							}
						}
						break;
					}

					case XmlNodeType.EndElement:
					{
						if(reader.Name == "Header")
						{
							// flag that we're done reading the header
							finishedHeader = true;
						}
						break;
					}
				}
			}

			// position all the header elements
			Title_PlatformLabel.SizeToFit();
			Date_PlatformLabel.SizeToFit();
			Speaker_PlatformLabel.SizeToFit();
		}

		public override void AddOffset(float xOffset, float yOffset)
		{
			base.AddOffset(xOffset, yOffset);

			Title_PlatformLabel.Layer.Position = new PointF(Title_PlatformLabel.Layer.Position.X + xOffset, 
																		   Title_PlatformLabel.Layer.Position.Y + yOffset);

			Date_PlatformLabel.Layer.Position = new PointF(Date_PlatformLabel.Layer.Position.X + xOffset, 
																		  Date_PlatformLabel.Layer.Position.Y + yOffset);

			Speaker_PlatformLabel.Layer.Position = new PointF(Speaker_PlatformLabel.Layer.Position.X + xOffset, 
																			 Speaker_PlatformLabel.Layer.Position.Y + yOffset);
		}

		public override void AddToView(object obj)
		{
			base.AddToView(obj);

			((UIView)obj).AddSubview(Speaker_PlatformLabel);
			((UIView)obj).AddSubview(Title_PlatformLabel);
			((UIView)obj).AddSubview(Date_PlatformLabel);
		}

		public override void RemoveFromView()
		{
			base.RemoveFromView();

			Title_PlatformLabel.RemoveFromSuperview();
			Date_PlatformLabel.RemoveFromSuperview();
			Speaker_PlatformLabel.RemoveFromSuperview();
		}

		public override RectangleF GetFrame()
		{
			// Get a bounding frame that encompasses all children
			RectangleF frame = Parser.CalcBoundingFrame(Speaker_PlatformLabel.Frame, Title_PlatformLabel.Frame);

			frame = Parser.CalcBoundingFrame(frame, Date_PlatformLabel.Frame);

			base.DebugFrameView.Frame = frame;

			return frame;
		}
	}
}
