using System;
using System.Xml;
using System.Drawing;
using Notes.PlatformUI;

namespace Notes
{
	public class Header : BaseControl
	{
        protected PlatformLabel Title { get; set; }
        protected PlatformLabel Date { get; set; }
        protected PlatformLabel Speaker { get; set; }

		protected override void Initialize()
		{
			base.Initialize();

            Title = PlatformLabel.Create();
			Title.SetFont("Verdana", 16f);

            Date = PlatformLabel.Create();
            Date.SetFont("Verdana", 12f);

            Speaker = PlatformLabel.Create();
            Speaker.SetFont("Verdana", 12f);
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
            Title.SetFont(mStyle.mFont.mName, mStyle.mFont.mSize.Value);
            Date.SetFont(mStyle.mFont.mName, mStyle.mFont.mSize.Value);
            Speaker.SetFont(mStyle.mFont.mName, mStyle.mFont.mSize.Value);

            Title.TextColor = mStyle.mFont.mColor.Value;
            Date.TextColor = mStyle.mFont.mColor.Value;
            Speaker.TextColor = mStyle.mFont.mColor.Value;


            //TODO: Stop hardcoding offsets for the header
			float xPos = bounds.X;
			float yPos = bounds.Y;

			// position all the header elements
			Title.Position = new PointF(xPos, yPos);
			Date.Position = new PointF(xPos, yPos + 15);
			Speaker.Position = new PointF(xPos, yPos + 30);


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
								Title.Text = reader.ReadElementContentAsString();
								break;
							}

							case "Date":
							{
								Date.Text = reader.ReadElementContentAsString();

								break;
							}

							case "Speaker":
							{
								Speaker.Text = reader.ReadElementContentAsString();
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
			Title.SizeToFit();
			Date.SizeToFit();
			Speaker.SizeToFit();
		}

		public override void AddOffset(float xOffset, float yOffset)
		{
			base.AddOffset(xOffset, yOffset);

			Title.Position = new PointF(Title.Position.X + xOffset, 
                                        Title.Position.Y + yOffset);

			Date.Position = new PointF(Date.Position.X + xOffset, 
                                       Date.Position.Y + yOffset);

			Speaker.Position = new PointF(Speaker.Position.X + xOffset, 
                                          Speaker.Position.Y + yOffset);
		}

        public override void AddToView(object obj)
		{
			Title.AddAsSubview(obj);
            Date.AddAsSubview(obj);
            Speaker.AddAsSubview(obj);

            TryAddDebugLayer(obj);
		}

        public override void RemoveFromView(object obj)
		{
			Title.RemoveAsSubview(obj);
            Date.RemoveAsSubview(obj);
            Speaker.RemoveAsSubview(obj);

            TryRemoveDebugLayer(obj);
		}

		public override RectangleF GetFrame()
		{
			// Get a bounding frame that encompasses all children
			RectangleF frame = Parser.CalcBoundingFrame(Speaker.Frame, Title.Frame);

			frame = Parser.CalcBoundingFrame(frame, Date.Frame);

			base.DebugFrameView.Frame = frame;

			return frame;
		}
	}
}
