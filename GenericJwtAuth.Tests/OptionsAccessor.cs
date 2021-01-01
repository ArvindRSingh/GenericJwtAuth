using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace GenericJwtAuth.Tests
{
    public class OptionsAccessor<T> : IOptions<IdentityOptions>
    {
        IdentityOptions IOptions<IdentityOptions>.Value => new IdentityOptions();
    }
}