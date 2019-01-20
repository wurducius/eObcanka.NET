using System;
using System.Collections.Generic;
using System.Text;

namespace eObcanka.NET.Enums
{
    public enum TagId
    {
        CARD_NUMBER = 0x01,
        CERTIFICATE_SERIAL_NUMBER = 2,
        KEY_KCV = 0xC0,
        KEY_COUNTER = 0xC1,
        DOK_STATE = 0x8B,
        DOK_TRY_LIMIT = 0x8C,
        DOK_MAX_TRY_LIMIT = 0x8D,
        IOK_STATE = 0x82,
        IOK_TRY_LIMIT = 0x83,
        IOK_MAX_TRY_LIMIT = 0x84
    }
}
