using System;
using System.Collections.Generic;
using System.Text;

namespace eObcanka.Certificates
{
    public class IdentificationCertificate : Certificate
    {
        public IdentificationCertificate(byte[] data, CertificateType type) : base(data, type)
        {}

        public string givenName {
            get { return ExtractName("2.5.4.42"); }
        }

        public string surname {
            get { return ExtractName("2.5.4.4"); }
        }

        public string countryName {
           get { return ExtractName("1.2.203.7064.1.1.11.7"); }
        }

        public string countryCode {
            get { return ExtractName("C"); }
        }

        public string serialNumber {
            get { return ExtractName("2.5.4.5"); }
        }

        public string organization {
            get { return ExtractName("O"); }
        }

        public string marriageStatus {
            get { return ExtractName("1.2.203.7064.1.1.11.6"); }
        }

        public string birthNumber {
            get { return ExtractName("1.2.203.7064.1.1.11.5"); }
        }

        public string sex {
            get { return ExtractName("1.2.203.7064.1.1.11.2"); }
        }

        public string birthCity {
            get { return ExtractName("1.2.203.7064.1.1.11.4"); }
        }

        public string birthDate {
            get { return ExtractName("1.2.203.7064.1.1.11.1"); }
        }

        public string locality {
            get { return ExtractName("L"); }
        }

        public string city {
            get { return ExtractName("ST"); }
        }

        public string street {
            get { return ExtractName("STREET"); }
        }

        public string documentNumber {
            get { return documentNumber; }
        }
        
        public override string ToString()
        {
            return "IdentificationCertificate{" +
                    "serialNumber: " + serialNumber + "\n" +
                    "documentNumber: " + documentNumber + "\n" +
                    givenName + " " + surname + "\n" +
                    "Marriage:" + marriageStatus + " sex: " + sex + "\n" +
                    street + "\n" +
                    city + "\n" +
                    locality + "\n" +
                    countryName + "\n" +
                    countryCode + "\n" +

                    "Birth number: " + birthNumber + "\n" +
                    "Birth date: " + birthDate + "\n" +
                    "City: " + birthCity + "\n" +

                    "Organization: " + organization + "\n" +

                    '}';
        }
    }
}
