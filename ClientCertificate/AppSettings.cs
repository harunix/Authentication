namespace ClientCertificate
{
    public class EntraSettings
    {
        public string TenantId { get; set; }
        public string ClientId { get; set; }
        public string DefaultScope { get; set; }
        public string CertificatePath { get; set; }
        public string CertificateBase64 { get; set; }
        public string CertificatePassword { get; set; }
        public string CertificateThumbPrint { get; set; }

    }
}
