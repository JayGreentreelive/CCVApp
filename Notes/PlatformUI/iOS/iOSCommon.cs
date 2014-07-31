#if __IOS__
using System;
using MonoTouch.Foundation;
using MonoTouch.CoreGraphics;
using MonoTouch.CoreText;

// This file is where you can put anything SPECIFIC to Android that doesn't 
// require common base classes, and should be DYE-RECTLY referenced by Android code.

namespace Notes
{
    namespace PlatformUI
    {
        public class iOSCommon
        {
            public static String LoadDynamicFont(String name)
            {
                // get a path to our custom fonts folder
                String fontPath = NSBundle.MainBundle.BundlePath + "/Fonts/" + name + ".ttf";

                // build a data model for the font
                CGDataProvider fontProvider = MonoTouch.CoreGraphics.CGDataProvider.FromFile(fontPath);

                // create a renderable font out of it
                CGFont newFont = MonoTouch.CoreGraphics.CGFont.CreateFromProvider(fontProvider);

                // get the legal loadable font name
                String fontScriptName = newFont.PostScriptName;

                // register the font with the CoreText / UIFont system.
                NSError error = null;
                CTFontManager.RegisterGraphicsFont(newFont, out error);

                //TODO: only throw errors if it's not an "already registered" error
                //TODO: Unregister all fonts registered
                //TODO: See if we can use FromURL instead of FromProvider to simplify
                if(error != null)
                {
                    //throw new NSErrorException(error);
                }

                return fontScriptName;
            }
        }
    }
}

#endif
