using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdentityApi.Model
{
    public class TokenModel
    {
        public string? Token { get; set; }
        public DateTime? Expiration { get; set; }
        public DateTime? CreatedDate { get; set; }
        public List<ClaimModel> Claims { get; set; }
    }

    public class ClaimModel
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }
}
