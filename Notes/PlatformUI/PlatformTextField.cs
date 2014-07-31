﻿using System;
using System.Drawing;

namespace Notes
{
    namespace PlatformUI
    {
        /// <summary>
        /// The base text field that provides an interface to platform specific text fields.
        /// </summary>
        public abstract class PlatformTextField : PlatformBaseUI
        {
            public static PlatformTextField Create( )
            {
                #if __IOS__
                return new iOSTextField( );
                #endif

                #if __ANDROID__
                return new DroidTextField( );
                #endif
            }

            public string Placeholder
            {
                get { return getPlaceholder( ); }
                set { setPlaceholder( value ); }
            }

            protected abstract string getPlaceholder( );

            protected abstract void setPlaceholder( string placeholder );

            public uint PlaceholderTextColor
            {
                set { setPlaceholderTextColor( value ); }
            }

            protected abstract void setPlaceholderTextColor( uint color );

            public abstract void ResignFirstResponder( );
        }
    }
}

