using System;
using System.Collections.Generic;
using System.Resources;
using System.Text;

namespace Nivra.Localization
{
    public class TextResourceManager
    {
        public ResourceManager DefaultResourceManager { get; set; }
        public TextResourceManager()
        {
            DefaultResourceManager = new ResourceManager("default", typeof(TextResourceManager).Assembly);
        }

    }
}
