using System;
using System.Collections.Generic;
using System.IO;

using PCSC;
using PCSC.Iso7816;
using eObcanka.Certificates;
using eObcanka.NET.Enums;
using eObcanka.NET;

namespace eObcanka
{
    public class Card
    {

        /// <summary>
        /// Selected application mode
        /// </summary>
        private static byte[] CurrentApplication;


        /// <summary>
        /// Read Card ID and Identity Certificate Data
        /// </summary>
        public static void ReadData()
        {
            try
            {

                var contextFactory = ContextFactory.Instance;

                using (var ctx = contextFactory.Establish(SCardScope.System))
                {

                    // Load Readers
                    var readerNames = ctx.GetReaders();
                    if (Reader.NoReaderFound(readerNames))
                    {
                        Console.WriteLine("You need at least one reader in order to run this example.");
                        Console.ReadKey();
                        return;
                    }

                    var name = Reader.ChooseReader(readerNames);
                    if (name == null)
                    {
                        return;
                    }

                    using (var isoReader = new IsoReader(
                        context: ctx,
                        readerName: name,
                        mode: SCardShareMode.Shared,
                        protocol: SCardProtocol.Any,
                        releaseContextOnDispose: false))
                    {

                        /// Read Card Id
                        string cardId = ReadCardId(isoReader);

                        // Get identification certificate
                        byte[] IdentificationCertifitateData = GetIdentificationCertificateFile(isoReader);

                        // Decrypt identification certificate
                        IdentificationCertificate cert = Certificate.Parse(IdentificationCertifitateData, CertificateType.IDENTIFICATION) as IdentificationCertificate;

                        Console.WriteLine("IdentificationCertificate data:");
                        Console.WriteLine("===============================");
                        Console.WriteLine(cert.ToString2());
                       /* Console.WriteLine(cert.surname);
                        Console.WriteLine(cert.documentNumber);
                        Console.WriteLine(cert.birthCity);
                        Console.WriteLine(cert.birthDate);
                        Console.WriteLine(cert.birthNumber);
                        Console.WriteLine(cert.city);
                        Console.WriteLine(cert.countryCode);
                        Console.WriteLine(cert.countryName);
                        Console.WriteLine(cert.locality);
                        Console.WriteLine(cert.marriageStatus);
                        Console.WriteLine(cert.organization);
                        Console.WriteLine(cert.serialNumber);
                        Console.WriteLine(cert.sex);
                        Console.WriteLine(cert.street);*/

                    }
                }
            }
            catch (Exception ex)
            {

            }
        }

        /// <summary>
        /// Read Card Id
        /// </summary>
        /// <param name="isoReader"></param>
        private static string ReadCardId(IsoReader isoReader)
        {
            SelectApplication(AppId.APP_ID_CARD_MANAGEMENT, isoReader);

            var apdu = new CommandApdu(IsoCase.Case2Short, isoReader.ActiveProtocol)
            {
                CLA = 0x80,
                Instruction = InstructionCode.GetData,
                P1 = 0x01,
                P2 = 0x01,
                Le = 0x08
            };

            Console.WriteLine("Send APDU with \"READ CARD ID\" command: {0}",
                 BitConverter.ToString(apdu.ToArray()));

            var response = isoReader.Transmit(apdu);

            Console.WriteLine("SW1 SW2 = {0:X2} {1:X2}",
                                    response.SW1, response.SW2);

            if (!response.HasData)
            {
                Console.WriteLine("No data. (Card does not understand \"READ CARD ID\")");
                return null;
            }
            else
            {
                var data = response.GetData();
                Console.WriteLine("CARD ID: {0}", BitConverter.ToString(data));
                return BitConverter.ToString(data);
            }
        }

        /// <summary>
        /// Get Identification Certificate File
        /// </summary>
        /// <param name="isoReader"></param>
        /// <returns></returns>
        private static byte[] GetIdentificationCertificateFile(IsoReader isoReader)
        {
            SelectApplication(AppId.APP_ID_FILE_MANAGEMENT, isoReader);

            int h = FileId.FILE_ID_CERTIFICATE_IDENTIFICATION / 256;
            int l = FileId.FILE_ID_CERTIFICATE_IDENTIFICATION % 256;

            var apdu = new CommandApdu(IsoCase.Case4Short, isoReader.ActiveProtocol)
            {
                CLA = 0x00,
                Instruction = InstructionCode.SelectFile,
                P1 = 0x08,
                P2 = 0x00,
                Data = new byte[] { (byte)h, (byte)l }
            };

            Console.WriteLine("Send APDU with \"SELECT INDETIFICATION CERTIFICATE FILE\" command: {0}",
                                           BitConverter.ToString(apdu.ToArray()));

            var response = isoReader.Transmit(apdu);

            Console.WriteLine("SW1 SW2 = {0:X2} {1:X2}",
                                    response.SW1, response.SW2);

            if (response.SW1 == StatusCode.OK)
            {
                if (response.HasData)
                {
                    return ReadIdentificationCertificateFile(isoReader, response.GetData());
                }
                else
                {
                    Console.WriteLine("No data. (Card does not understand \"SELECT INDETIFICATION CERTIFICATE FILE - EMPTY RESPONSE\")");
                    return null;
                }
            }
            else
            {
                Console.WriteLine("No data. (Card does not understand \"SELECT INDETIFICATION CERTIFICATE FILE\")");
                return null;
            }
        }

        /// <summary>
        /// Read Identification Certificate File
        /// </summary>
        /// <param name="isoReader"></param>
        /// <param name="fileInfoData"></param>
        /// <returns></returns>
        private static byte[] ReadIdentificationCertificateFile(IsoReader isoReader, byte[] fileInfoData)
        {
            int size = 0xD0;
            int offset = 0;
            Response response;
            int fileSize = Convert.ToInt32(fileInfoData[4]) * 256 + Convert.ToInt32(fileInfoData[5]);
            MemoryStream stream = new MemoryStream();
            BinaryWriter bos = new BinaryWriter(stream);

            do
            {
                var h = offset / 256;
                var l = offset % 256;
                if (offset + size > fileSize)
                {
                    size = fileSize - offset;
                }
                if (size <= 0)
                {
                    break;
                }

                byte[] readFileRequest = new byte[] { 0x00, (byte)0xB0, (byte)h, (byte)l, (byte)size };

                var apdu = new CommandApdu(IsoCase.Case2Extended, isoReader.ActiveProtocol)
                {
                    CLA = 0x00,
                    Instruction = InstructionCode.ReadBinary,
                    P1 = (byte)h,
                    P2 = (byte)l,
                    Le = (byte)size

                };

                Console.WriteLine("Send APDU with \"READ BINARY INDETIFICATION CERTIFICATE FILE\" command: {0}",
                 BitConverter.ToString(apdu.ToArray()));

                response = isoReader.Transmit(apdu);

                Console.WriteLine("SW1 SW2 = {0:X2} {1:X2}",
                                        response.SW1, response.SW2);

                if (response.SW1 == StatusCode.OK)
                {
                    try
                    {
                        bos.Write(response.GetData());
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error in gathering response data READ BINARY INDETIFICATION CERTIFICATE FILE: {0}", e.InnerException);
                    }
                    offset += size;
                }
            } while (response.SW1 == StatusCode.OK);
            return stream.ToArray();
        }

        /// <summary>
        /// Select Application Mode
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="isoReader"></param>
        /// <returns></returns>
        public static bool SelectApplication(byte[] appId, IsoReader isoReader)
        {
            if (CurrentApplication != null)
            {
                if (Array.Equals(CurrentApplication, appId))
                {
                    //Don't select application if is already set
                    return true;
                }
            }

            var apdu = new CommandApdu(IsoCase.Case4Short, isoReader.ActiveProtocol)
            {
                CLA = 0x00,
                Instruction = InstructionCode.SelectFile,
                P1 = 0x04,
                P2 = 0x0C,
                Data = appId
            };

            Console.WriteLine("Send APDU with \"SELECT APPLICATION\" command: {0}",
                            BitConverter.ToString(apdu.ToArray()));

            var response = isoReader.Transmit(apdu);

            Console.WriteLine("SW1 SW2 = {0:X2} {1:X2}", response.SW1, response.SW2);

            if (response.SW1 == StatusCode.OK)
            {
                CurrentApplication = appId;
                return true;
            }
            else
            {
                Console.WriteLine("No data. (Card does not understand \"SELECT APPLICATION\")");
                return false;

            }
        }

    }
}
