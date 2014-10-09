using System;

namespace CCVApp
{
    namespace Shared
    {
        namespace Network
        {
            /// <summary>
            /// Contains a news item for the news display.
            /// </summary>
            public class RockNews
            {
                public string Title { get; set; }
                public string Description { get; set; }
                public string ReferenceURL { get; set; }

                public string ImageName { get; set; }
                public string HeaderImageName { get; set; }

                public RockNews( string title, string description, string referenceUrl, string imageName, string headerImageName )
                {
                    Title = title;
                    Description = description;
                    ReferenceURL = referenceUrl;

                    ImageName = imageName;
                    HeaderImageName = headerImageName;
                }
            }
        }
    }
}
