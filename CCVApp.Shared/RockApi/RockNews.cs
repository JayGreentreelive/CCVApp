using System;
using Newtonsoft.Json;

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

                [JsonConstructor]
                public RockNews( string title, string description, string referenceUrl, string imageName, string headerImageName )
                {
                    Title = title;
                    Description = description;
                    ReferenceURL = referenceUrl;

                    ImageName = imageName;
                    HeaderImageName = headerImageName;
                }

                // create a copy constructor
                public RockNews( RockNews rhs )
                {
                    Title = rhs.Title;
                    Description = rhs.Description;
                    ReferenceURL = rhs.ReferenceURL;

                    ImageName = rhs.ImageName;
                    HeaderImageName = rhs.HeaderImageName;
                }
            }
        }
    }
}
