using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Drawing;
using RockMobile.Network;

namespace Notes
{
    public class Parser
	{
        public static IUIControl TryParseControl(Notes.BaseControl.CreateParams parentParams, XmlReader reader)
		{
			// either create/parse a new control, or return null.
			switch(reader.Name)
			{
                case "Paragraph": return new Paragraph(parentParams, reader);
                case "StackPanel": return new StackPanel(parentParams, reader);
                case "Header": return new Header(parentParams, reader);
                case "RevealBox": return new RevealBox(parentParams, reader);
                case "Quote": return new Quote(parentParams, reader);
                case "TextInput": return new TextInput(parentParams, reader);
			}

			return null;
		}

		// Return a rect that contains both rects A and B (sort of a bounding box)
		public static RectangleF CalcBoundingFrame(RectangleF frameA, RectangleF frameB)
		{
			RectangleF frame = new RectangleF();

			// get left edge
			float leftEdge = frameA.Left < frameB.Left ? frameA.Left : frameB.Left;

			// get top edge
			float topEdge = frameA.Top < frameB.Top ? frameA.Top : frameB.Top;

			// get right edge
			float rightEdge = frameA.Right > frameB.Right ? frameA.Right : frameB.Right;

			// get bottom edge
			float bottomEdge = frameA.Bottom > frameB.Bottom ? frameA.Bottom : frameB.Bottom;

			frame.X = leftEdge;
			frame.Y = topEdge;
			frame.Width = rightEdge - leftEdge;
			frame.Height = bottomEdge - topEdge;

			return frame;
		}

		// Returns a rect that is rectA expanded by rectB (inflate in all four directions)
		public static RectangleF CalcExpandedFrame(RectangleF frameA, RectangleF frameB)
		{
			RectangleF expandedRect = frameA;

			expandedRect.X += frameB.Left;
			expandedRect.Y += frameB.Top;
			expandedRect.Width += frameB.Width;
			expandedRect.Height += frameB.Height;

			return expandedRect;
		}
	}
}
