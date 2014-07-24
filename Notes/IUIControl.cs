using System;
using MonoTouch.UIKit;

namespace Notes
{
	public interface IUIControl
	{
		void AddOffset(float xOffset, float yOffset);
		System.Drawing.RectangleF GetFrame();

		void AddToView(object obj);
		void RemoveFromView();

		void TouchesEnded (UITouch touch);

        Styles.Alignment GetHorzAlignment();
	}
}
