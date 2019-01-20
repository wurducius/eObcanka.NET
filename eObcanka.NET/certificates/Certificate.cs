using System;
using System.Security.Cryptography.X509Certificates;

using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text;

namespace eObcanka.certificates
{
    public class Certificate
    {
        private readonly byte[] data;
        private readonly CertificateType type;
        protected List<X500DistinguishedName> distNames;
        protected string documentNumber;

        public Certificate(byte[] data, CertificateType type)
        {
            this.data = data;
            this.type = type;
            Parse();
        }

        private void Parse()
        {
            try
            {
                var certCollection = new X509Certificate2Collection();
                certCollection.Import(data);

                distNames = new List<X500DistinguishedName>();
                foreach (var cert in certCollection)
                {
                    distNames.Add(cert.SubjectName);
                    documentNumber = Convert.ToString(cert.GetSerialNumber());
                }
            }
            catch (CryptographicException e)
            {
                Console.WriteLine(e.StackTrace);
            }
        }

        public static Certificate Parse(byte[] berData, CertificateType type)
        {
            if (berData == null)
            {
                return null;
            }
            if (type == CertificateType.IDENTIFICATION)
            {
                return new IdentificationCertificate(berData, type);
            }
            /*else if (type == CertificateType.AUTHORIZATION)
            {
                //return new AuthorizationCertificate(berData, type);
            }*/
            return null;
        }


        protected string ExtractName(string name)
        {
            foreach (var rdn in distNames )
            {
                var decodedName = rdn.Decode(X500DistinguishedNameFlags.None);
                if (string.Equals(name, decodedName, StringComparison.InvariantCultureIgnoreCase))
                {
                    return decodedName;
                }
            }
            return null;
        }
    }
}
