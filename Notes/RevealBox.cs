using System;
using System.Xml;
using MonoTouch.UIKit;
using System.Drawing;

namespace Notes
{
	public class RevealBox : Label
	{
		protected string HiddenText { get; set; }

        public RevealBox (CreateParams parentParams, XmlReader reader) : base()
		{
            // don't call the base constructor that reads. we'll do the reading here.

			Initialize();


			// check for attributes we support
			RectangleF bounds = new RectangleF();
            ParseCommonAttribs(reader, ref bounds);

            // take our parent's style but override with anything we set
            mStyle = parentParams.Style;
            Styles.Style.ParseStyleAttributesWithDefaults(reader, ref mStyle, ref ControlStyles.mRevealBox);

            // create the font that either we or our parent defined
            PlatformLabel.Font = UIFont.FromName(mStyle.mFont.mName, mStyle.mFont.mSize.Value);
            PlatformLabel.TextColor = Styles.Style.GetUIColor(mStyle.mFont.mColor.Value);


			// get text
			string result = reader.GetAttribute("Text");
			if(String.IsNullOrEmpty(result) == false)
			{
				HiddenText = result;
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
								case "Text":
								{
									HiddenText = reader.ReadElementContentAsString();
									break;
								}
							}
							break;
						}

                        case XmlNodeType.Text:
                        {
                            // support text as embedded in the element
                            HiddenText = reader.Value.Replace(System.Environment.NewLine, "");

                            break;
                        }

						case XmlNodeType.EndElement:
						{
							// if we hit the end of our label, we're done.
							if(reader.Name == "RevealBox")
							{
								finishedLabel = true;
							}

							break;
						}
					}
				}
			}

            // before applying the hidden text, measure and see what length we should be using.
            // This is totally hacky / temp till we get a real reveal affect in.
            if(HiddenText.Length > "[TOUCH]".Length)
            {
                SetText(HiddenText);
            }
            else
            {
                SetText("[TOUCH]");
            }
            PlatformLabel.Text = "[TOUCH]";

            PlatformLabel.Layer.Position = new PointF (bounds.X, bounds.Y);
		}

		public override void TouchesEnded (UITouch touch)
		{
			// if the touch that was released was in our region, toggle the text
			PointF point = touch.LocationInView(PlatformLabel);
			if(PlatformLabel.Bounds.Contains(point))
			{
				SetText(HiddenText);
			}
		}
	}
}
