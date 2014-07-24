using System;
using MonoTouch.UIKit;
using System.Drawing;
using System.Xml;
using Notes.Styles;

namespace Notes
{
	public class BaseControl : IUIControl
	{
		protected UIView DebugFrameView { get; set; }
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
			DebugFrameView = new MonoTouch.UIKit.UIView();
			DebugFrameView.Layer.AnchorPoint = new PointF(0, 0);
			DebugFrameView.Layer.Opacity = .50f;
			DebugFrameView.Layer.BackgroundColor = UIColor.Blue.CGColor;
			DebugFrameView.Layer.ZPosition = 100;
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
				DebugFrameView.Layer.Position = new PointF(DebugFrameView.Layer.Position.X + xOffset, 
														   DebugFrameView.Layer.Position.Y + yOffset);
			}
		}

		public virtual void AddToView(object obj)
		{
			if(ShowDebugFrame)
			{
				((UIView)obj).AddSubview(DebugFrameView);
			}
		}

		public virtual void RemoveFromView()
		{
			if(ShowDebugFrame)
			{
				DebugFrameView.RemoveFromSuperview();
			}
		}

		public virtual void TouchesEnded(UITouch touch)
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

