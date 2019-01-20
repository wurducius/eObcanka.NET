using System;
using System.Collections.Generic;
using System.IO;
 
using PCSC;
using PCSC.Iso7816;
using eObcanka.certificates;

namespace eObcanka
{
    public class Reader
    {
        public static int TAG_ID_CARD_NUMBER = 0x01;
        public static int TAG_ID_CERTIFICATE_SERIAL_NUMBER = 2;
        public static int TAG_ID_KEY_KCV = 0xC0;
        public static int TAG_ID_KEY_COUNTER = 0xC1;

        public static int TAG_ID_DOK_STATE = 0x8B;
        public static int TAG_ID_DOK_TRY_LIMIT = 0x8C;
        public static int TAG_ID_DOK_MAX_TRY_LIMIT = 0x8D;

        public static int TAG_ID_IOK_STATE = 0x82;
        public static int TAG_ID_IOK_TRY_LIMIT = 0x83;
        public static int TAG_ID_IOK_MAX_TRY_LIMIT = 0x84;

        private static int FILE_ID_CERTIFICATE_AUTHORIZATION = 0x0132;
        private static int FILE_ID_CERTIFICATE_IDENTIFICATION = 0x0001;

        private static byte[] APP_ID_CARD_MANAGEMENT = { 0xD2, 0X03, 0x10, 0x01, 0x00, 0x01, 0x00, 0x02, 0x02 };
        private static byte[] APP_ID_FILE_MANAGEMENT = { 0xD2, 0x03, 0x10, 0x01, 0x00, 0x01, 0x03, 0x02, 0x01, 0x00 };

        private static byte[] currentApplication;

        public static void DoWork()
        {
            var contextFactory = ContextFactory.Instance;
            using (var ctx = contextFactory.Establish(SCardScope.System))
            {
                var readerNames = ctx.GetReaders();
                if (NoReaderFound(readerNames))
                {
                    Console.WriteLine("You need at least one reader in order to run this example.");
                    Console.ReadKey();
                    return;
                }

                var name = ChooseReader(readerNames);
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
                    //ReadCardId(isoReader);

                    byte[] fileInfoData = SelectFile(isoReader);
                    IdentificationCertificate cert = Certificate.Parse(fileInfoData, CertificateType.IDENTIFICATION) as IdentificationCertificate;
                    //IdentificationCertificate cert = new Certificate(fileInfoData, CertificateType.IDENTIFICATION) as IdentificationCertificate;
                }
            }
        }

        private static void ReadCardId(IsoReader isoReader)
        {
            //  Build a GET CHALLENGE command
            var apdu = new CommandApdu(IsoCase.Case2Short, isoReader.ActiveProtocol)
            {
                CLA = 0x80, // Class
                Instruction = InstructionCode.GetData,
                P1 = 0x01, // Parameter 1
                P2 = 0x01, // Parameter 2
                Le = 0x08 // Expected length of the returned data
            };

            Console.WriteLine("Send APDU with \"READ ID\" command: {0}",
                 BitConverter.ToString(apdu.ToArray()));

            var response = isoReader.Transmit(apdu);

            Console.WriteLine("SW1 SW2 = {0:X2} {1:X2}",
                                    response.SW1, response.SW2);

            if (!response.HasData)
            {
                Console.WriteLine("No data. (Card does not understand \"GET CHALLENGE\")");
            }
            else
            {
                var data = response.GetData();
                Console.WriteLine("Challenge: {0}", BitConverter.ToString(data));
            }
        }

        private static byte[] SelectFile(IsoReader isoReader)
        {
            SelectApplication(APP_ID_FILE_MANAGEMENT, isoReader);

            int h = FILE_ID_CERTIFICATE_IDENTIFICATION / 256;
            int l = FILE_ID_CERTIFICATE_IDENTIFICATION % 256;

            // byte[] fileInfo = new byte[] { 0x00, (byte)0xA4, 0x08, 0x00, 0x02, (byte)h, (byte)l };

            var apdu = new CommandApdu(IsoCase.Case4Short, isoReader.ActiveProtocol)
            {
                CLA = 0x00, // Class
                Instruction = InstructionCode.SelectFile,
                P1 = 0x08, // Parameter 1
                P2 = 0x00, // Parameter 2
                //P3 = (byte)h,
                Data = new byte[] { (byte)h, (byte)l }
                // Le = 0x02, // Expected length of the returned data
                // Data = new byte[] { (byte) h, (byte)l }
            };



            //   Console.WriteLine("Send APDU with \"CHECK FILE\" command: {0}",

            //  BitConverter.ToString(apdu.ToArray()));
            var response = isoReader.Transmit(apdu);
            //var response = isoReader.Transmit(apdu, fileInfo);

            Console.WriteLine("SW1 SW2 = {0:X2} {1:X2}",
                                    response.SW1, response.SW2);


            if (response.SW1 == 144)
            { //you must call get response
              //byte[] getResponse = new byte[] { 0x00, (byte)0xC0, 0, 0, (byte)r.getSW2() };
              //r = c.transmit(c.createCommand(getResponse));

                //GetFile(isoReader, response.SW1, response.SW2);
                return ReadFile(isoReader, response.GetData());
            }

            if (!response.HasData)
            {
                Console.WriteLine("No data. (Card does not understand \"GET CHALLENGE\")");
                return null;
            }
            else
            {
                var data = response.GetData();
                Console.WriteLine("Challenge: {0}", BitConverter.ToString(data));
                return data;
            }
        }


        //    private static void GetFile(IsoReader isoReader, byte sW1, byte sW2)
        //    {
        //        var apdu = new CommandApdu(IsoCase.Case4Short, isoReader.ActiveProtocol)
        //        {
        //            CLA = 0x00, // Class
        //            Instruction = InstructionCode.GetResponse,
        //            P1 = 0x00, // Parameter 1
        //            P2 = 0x00, // Parameter 2
        //                       // Le = 0x02, // Expected length of the returned data
        //            Data = new byte[] { 0x01, 0x32 }
        //        };



        //        Console.WriteLine("Send APDU with \"GET FILE\" command: {0}",
        //BitConverter.ToString(apdu.ToArray()));

        //        var response = isoReader.Transmit(apdu);

        //        Console.WriteLine("SW1 SW2 = {0:X2} {1:X2}",
        //                                response.SW1, response.SW2);

        //    }

        private static byte[] ReadFile(IsoReader isoReader, byte[] fileInfoData)
        {
            int size = 0xD0;
            //file exists
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

                var apdu = new CommandApdu(IsoCase.Case4Short, isoReader.ActiveProtocol)
                {
                    CLA = 0x00, // Class
                    Instruction = InstructionCode.ReadBinary,
                    P1 = 0x08, // Parameter 1
                    P2 = 0x00, // Parameter 2
                               //P3 = (byte)h,
                    Data = new byte[] { (byte)h, (byte)l }
                    // Le = 0x02, // Expected length of the returned data
                    // Data = new byte[] { (byte) h, (byte)l }
                };



                //   Console.WriteLine("Send APDU with \"CHECK FILE\" command: {0}",

                //  BitConverter.ToString(apdu.ToArray()));

                response = isoReader.Transmit(apdu);

                /*Console.WriteLine("SW1 SW2 = {0:X2} {1:X2}",
                                        response.SW1, response.SW2);*/

                //r = c.transmit(c.createCommand(readFileRequest));



                //if (r.getSW() == 0x9000)
                //{
                try
                {
                    bos.Write(response.GetData());
                }
                catch (Exception e)
                {
                    //e.printStackTrace();
                }
                offset += size;
                //}
            } while (response.SW1 == 144);
            return stream.ToArray();
        }

        private static string ChooseReader(IList<string> readerNames)
        {
            // Show available readers.
            Console.WriteLine("Available readers: ");
            for (var i = 0; i < readerNames.Count; i++)
            {
                Console.WriteLine("[" + i + "] " + readerNames[i]);
            }

            // Ask the user which one to choose.
            Console.Write("Which reader has an inserted card that supports the GET CHALLENGE command? ");
            var line = Console.ReadLine();

            if (int.TryParse(line, out var choice) && (choice >= 0) && (choice <= readerNames.Count))
            {
                return readerNames[choice];
            }

            Console.WriteLine("An invalid number has been entered.");
            Console.ReadKey();
            return null;
        }

        public static bool SelectApplication(byte[] appId, IsoReader isoReader)
        {
            if (currentApplication != null)
            {
                if (Array.Equals(currentApplication, appId))
                { //Don't select application if is already set
                    return true;
                }
            }

            var apdu = new CommandApdu(IsoCase.Case4Short, isoReader.ActiveProtocol)
            {
                CLA = 0x00, // Class
                Instruction = InstructionCode.SelectFile,
                P1 = 0x04, // Parameter 1
                P2 = 0x0C, // Parameter 2
                //P3 = (byte)h,
                Data = appId
                // Le = 0x02, // Expected length of the returned data
                // Data = new byte[] { (byte) h, (byte)l }
            };


            var response = isoReader.Transmit(apdu);

            Console.WriteLine("SW1 SW2 = {0:X2} {1:X2}", response.SW1, response.SW2);

            //IResponseAPDU r = c.transmit(c.createCommand((HexUtils.concatArrays(selectApplet, appId))));

            //if (r.getSW() == 0x9000)
            //{
            //    currentApplication = appId;
            //    return true;
            //}
            return false;
        }

        public bool Equality(byte[] a1, byte[] b1)
        {
            if (a1 == null || b1 == null)
                return false;
            int length = a1.Length;
            if (b1.Length != length)
                return false;
            while (length > 0)
            {
                length--;
                if (a1[length] != b1[length])
                    return false;
            }
            return true;
        }

        public static byte[] Combine(byte[] first, byte[] second)
        {
            byte[] ret = new byte[first.Length + second.Length];
            Buffer.BlockCopy(first, 0, ret, 0, first.Length);
            Buffer.BlockCopy(second, 0, ret, first.Length, second.Length);
            return ret;
        }

        private static bool NoReaderFound(ICollection<string> readerNames)
        {
            return readerNames == null || readerNames.Count < 1;
        }
    }
}
