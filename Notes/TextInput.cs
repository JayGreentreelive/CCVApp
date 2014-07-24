using System;
using MonoTouch.UIKit;
using System.Xml;
using System.Drawing;
using MonoTouch.Foundation;

namespace Notes
{
	public class TextInput : BaseControl
	{
		protected UITextField PlatformTextField { get; set; }

		protected override void Initialize()
		{
			base.Initialize();

			// TODO: Update this to be like iBeacon, where it's platform abstracted
			PlatformTextField = new UITextField();

			// these could be optional in XML
			PlatformTextField.Layer.AnchorPoint = new PointF(0, 0);
			PlatformTextField.Bounds = new RectangleF();
			PlatformTextField.TextAlignment = UITextAlignment.Left;
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
            PlatformTextField.Font = UIFont.FromName(mStyle.mFont.mName, mStyle.mFont.mSize.Value);
            PlatformTextField.TextColor = Styles.Style.GetUIColor(mStyle.mFont.mColor.Value);
           

			// if no width was specified, use our parents
			if(bounds.Width == 0)
			{
				// always take the available width, in case this control
				// is specified to be offset relative to its parent
                bounds.Width = parentParams.Width - bounds.X;
			}

			// set the dimensions and position
			PlatformTextField.Bounds = bounds;
            PlatformTextField.Placeholder = " ";

			// get the hint text if it's as an attribute
			string result = reader.GetAttribute("PlaceHolder");
			if(String.IsNullOrEmpty(result) == false)
			{
				PlatformTextField.Placeholder = result;
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
									PlatformTextField.Placeholder = reader.ReadElementContentAsString();
									
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

			// position ourselves in absolute coordinates, and trust our parent to offset us to be relative to them.
            PlatformTextField.Layer.Position = new PointF (bounds.X, bounds.Y);

            // set the color of the hint text
            PlatformTextField.AttributedPlaceholder = new NSAttributedString (
                PlatformTextField.Placeholder,
                font: PlatformTextField.Font,
                foregroundColor: PlatformTextField.TextColor
            );
            PlatformTextField.SizeToFit();
		}

		public override void TouchesEnded (UITouch touch)
		{
			// hide the keyboard
			PlatformTextField.ResignFirstResponder();
		}

		public override void AddOffset(float xOffset, float yOffset)
		{
			base.AddOffset(xOffset, yOffset);

			PlatformTextField.Layer.Position = new PointF(PlatformTextField.Layer.Position.X + xOffset, 
												          PlatformTextField.Layer.Position.Y + yOffset);
		}

		public override void AddToView(object obj)
		{
			base.AddToView(obj);

			((UIView)obj).AddSubview(PlatformTextField);
		}

		public override void RemoveFromView()
		{
			base.RemoveFromView();

			PlatformTextField.RemoveFromSuperview();
		}

		public override RectangleF GetFrame()
		{
            base.DebugFrameView.Frame = PlatformTextField.Frame;
            return PlatformTextField.Frame;
		}
	}
}
