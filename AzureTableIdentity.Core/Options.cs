﻿using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureTableIdentity.Core
{
    class Options : IOptions<IdentityOptions>
    {
        public IdentityOptions Value => new IdentityOptions() { };
    }
}
