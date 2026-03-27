namespace Blogsphere.Webapp.Bff.Domain.Configurations
{
    public class IdentityGroupAccessOption
    {
        public const string OptionName = "IdentityGroupAccess";
        public string Authority { get; set; }
        public string Audience { get; set; }
        /// <summary>
        /// Optional list of acceptable JWT issuers (iss claim) for token validation.
        /// Useful when running in Docker where the authority host differs from the issuer
        /// encoded in tokens (e.g., localhost vs host.docker.internal).
        /// </summary>
        public string[] ValidIssuers { get; set; }
    }
}