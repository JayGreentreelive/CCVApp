using System;
using System.Xml;
using System.Drawing;
using Notes.PlatformUI;

namespace Notes
{
	public class TextInput : BaseControl
	{
		protected PlatformTextField TextField { get; set; }

		protected override void Initialize()
		{
			base.Initialize();

            TextField = PlatformTextField.Create();
		}

        public TextInput(CreateParams parentParams, XmlReader reader)
		{
			Initialize();

			// check for attributes we support
			RectangleF bounds = new RectangleF();
            ParseCommonAttribs(reader, ref bounds);

            // take our parent's style but override with anything we set
            mStyle = parentParams.Style;
            Styles.Style.ParseStyleAttributesWithDefaults(reader, ref mStyle, ref ControlStyles.mTextInput);

            // create the font that either we or our parent defined
            TextField.SetFont(mStyle.mFont.mName, mStyle.mFont.mSize.Value);
            TextField.TextColor = mStyle.mFont.mColor.Value;
           
            if(mStyle.mBackgroundColor.HasValue)
            {
                TextField.BackgroundColor = mStyle.mBackgroundColor.Value;
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

			// set the dimensions and position
			TextField.Bounds = bounds;
            TextField.Placeholder = " ";

			// get the hint text if it's as an attribute
			string result = reader.GetAttribute("PlaceHolder");
			if(String.IsNullOrEmpty(result) == false)
			{
				TextField.Placeholder = result;
			}

			// parse the rest of the stream
			if(reader.IsEmptyElement == false)
			{
                bool finishedLabel = false;
                while (finishedLabel == false && reader.Read ()) 
				{
					switch(reader.NodeType)
					{
					case XmlNodeType.Element:
						{
							switch(reader.Name)
							{
								case "PlaceHolder":
								{
									TextField.Placeholder = reader.ReadElementContentAsString();
									
									break;
								}
							}
							break;
						}

						case XmlNodeType.EndElement:
						{
							// if we hit the end of our label, we're done.
							if(reader.Name == "TextInput")
							{
								finishedLabel = true;
							}

							break;
						}
					}
				}
			}

            // size to fit to calculate the height, then reset our width with that height.
            TextField.SizeToFit();
            TextField.Bounds = new RectangleF(TextField.Bounds.Left, TextField.Bounds.Top, parentParams.Width, TextField.Bounds.Height);

            // set the color of the hint text
            TextField.PlaceholderTextColor = mStyle.mFont.mColor.Value;
		}

		public override void TouchesEnded (PointF touch)
		{
			// hide the keyboard
			TextField.ResignFirstResponder();
		}

		public override void AddOffset(float xOffset, float yOffset)
		{
			base.AddOffset(xOffset, yOffset);

			TextField.Position = new PointF(TextField.Position.X + xOffset, 
                                            TextField.Position.Y + yOffset);
		}

		public override void AddToView(object obj)
		{
            TextField.AddAsSubview(obj);

            TryAddDebugLayer(obj);
		}

        public override void RemoveFromView(object obj)
		{
			TextField.RemoveAsSubview(obj);

            TryRemoveDebugLayer(obj);
		}

		public override RectangleF GetFrame()
		{
            base.DebugFrameView.Frame = TextField.Frame;
            return TextField.Frame;
		}
	}
}
