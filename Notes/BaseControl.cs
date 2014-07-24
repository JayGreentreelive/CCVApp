using System;
using System.Drawing;
using System.Xml;
using Notes.Styles;
using Notes.PlatformUI;

namespace Notes
{
	public class BaseControl : IUIControl
	{
		protected PlatformLabel DebugFrameView { get; set; }
		protected bool ShowDebugFrame { get; set; }

        protected Style mStyle;

        // Class used for passing creation params to child controls
        public class CreateParams
        {
            public float Height { get; set; }
            public float Width { get; set; }
            public Style Style { get; set; }

            public CreateParams(float width, float height, ref Style style)
            {
                Height = height;
                Width = width;
                Style = style;
            }
        }

		protected virtual void Initialize()
		{
            mStyle = new Style();
            mStyle.Initialize();
            mStyle.mAlignment = Alignment.Inherit;

            //Debugging - show the grid frames
            DebugFrameView = PlatformLabel.Create();
			DebugFrameView.Opacity = .50f;
            DebugFrameView.BackgroundColor = 0x0000FFFF;
			DebugFrameView.ZPosition = 100;
			//Debugging
		}

		public BaseControl ()
		{
		}

        protected virtual void ParseCommonAttribs(XmlReader reader, ref RectangleF bounds)
		{
			// check for positioning attribs
			string result = reader.GetAttribute("Left");
			if(String.IsNullOrEmpty(result) == false)
			{
                float denominator = 1.0f;
                if(result.Contains("%"))
                {
                    result = result.Trim('%');
                    denominator = 100.0f;
                }

                bounds.X = float.Parse(result) / denominator;
			}

			result = reader.GetAttribute("Top");
			if(String.IsNullOrEmpty(result) == false)
			{
                float denominator = 1.0f;
                if(result.Contains("%"))
                {
                    result = result.Trim('%');
                    denominator = 100.0f;
                }

                bounds.Y = float.Parse(result) / denominator;
			}

			result = reader.GetAttribute("Width");
			if(String.IsNullOrEmpty(result) == false)
			{
                float denominator = 1.0f;
                if(result.Contains("%"))
                {
                    result = result.Trim('%');
                    denominator = 100.0f;
                }

				bounds.Width = float.Parse(result) / denominator;
			}

			result = reader.GetAttribute("Height");
			if(String.IsNullOrEmpty(result) == false)
			{
                float denominator = 1.0f;
                if(result.Contains("%"))
                {
                    result = result.Trim('%');
                    denominator = 100.0f;
                }

                bounds.Height = float.Parse(result) / denominator;
			}


			// check for a debug frame
			result = reader.GetAttribute("Debug");
			if(String.IsNullOrEmpty(result) == false)
			{
				ShowDebugFrame = bool.Parse(result);
			}
			else
			{
				ShowDebugFrame = false;
			}
		}

		public virtual void AddOffset (float xOffset, float yOffset)
		{
			if(ShowDebugFrame)
			{
				DebugFrameView.Position = new PointF(DebugFrameView.Position.X + xOffset, 
													 DebugFrameView.Position.Y + yOffset);
			}
		}

		public virtual void AddToView(object obj)
		{
			if(ShowDebugFrame)
			{
                DebugFrameView.AddAsSubview(obj);
			}
		}

		public virtual void RemoveFromView()
		{
			if(ShowDebugFrame)
			{
				DebugFrameView.RemoveAsSubview();
			}
		}

		public virtual void TouchesEnded(PointF touch)
		{
		}

        public Alignment GetHorzAlignment()
        {
            return mStyle.mAlignment.Value;
        }

		public virtual RectangleF GetFrame()
		{
			return new RectangleF();
		}
	}
}
    