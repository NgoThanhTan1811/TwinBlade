using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TwinBlade.Infrastructure.Options
{
    public sealed class CognitoOptions
    {
        public const string SectionName = "Cognito";

        public string UserPoolId { get; set; } = string.Empty;
        public string ClientId { get; set; } = string.Empty;
        public string Region { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }

}