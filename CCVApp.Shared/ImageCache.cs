using System;
using CCVApp.Shared.Config;
using System.IO;

namespace CCVApp.Shared
{
    public class ImageCache
    {
        static ImageCache _Instance = new ImageCache( );
        public static ImageCache Instance { get { return _Instance; } }

        object locker = new object();

        public ImageCache( )
        {
        }

        string ImageCachePath
        {
            get
            {
                // get the path based on the platform
                #if __IOS__
                //Note: Xamarin warns that we should use the below commented out version. But it doesn't work...the original does. 
                //http://developer.xamarin.com/guides/ios/application_fundamentals/working_with_the_file_system/ Their comment here says it's temporary, so maybe the docs are out of date?
                //string cachePath = MonoTouch.Foundation.NSFileManager.DefaultManager.GetUrls (MonoTouch.Foundation.NSSearchPathDirectory.DocumentDirectory, MonoTouch.Foundation.NSSearchPathDomain.User) [0].ToString();
                string cachePath = System.IO.Path.Combine ( Environment.GetFolderPath(Environment.SpecialFolder.Personal), "" );
                #else
                string cachePath = Rock.Mobile.PlatformCommon.Droid.Context.GetExternalFilesDir( null ).ToString( );
                #endif

                cachePath += "/" + GeneralConfig.ImageCacheDirectory;
                return cachePath;
            }
        }

        public bool WriteImage( MemoryStream imageBuffer, string filename )
        {
            // sync point so we don't read multiple times at once.
            // Note: If we ever need to support multiple threads reading from the cache at once, we'll need
            // a table to hash multiple locks per filename
            lock ( locker )
            {
                bool result = false;

                try
                {
                    // ensure the cache directory exists
                    if ( Directory.Exists( ImageCachePath ) == false )
                    {
                        Directory.CreateDirectory( ImageCachePath );
                    }

                    // attempt to write the image out to disk
                    using ( FileStream writer = new FileStream( ImageCachePath + "/" + filename, FileMode.Create ) )
                    {
                        imageBuffer.WriteTo( writer );

                        // if an exception occurs we won't set result to true
                        result = true;
                    }
                }
                catch ( Exception )
                {

                }

                // return the result
                return result;
            }
        }

        public MemoryStream ReadImage( string filename )
        {
            MemoryStream imageBuffer = new MemoryStream();

            try
            {
                using ( FileStream reader = new FileStream( ImageCachePath + "/" + filename, FileMode.Open ) )
                {
                    reader.CopyTo( imageBuffer );
                    imageBuffer.Position = 0;
                }
            }
            catch( Exception )
            {
                // null out the image buffer
                imageBuffer = null;
            }

            return imageBuffer;
        }
    }
}

