/* Copyright (C) 2017 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using SMBLibrary.Authentication.NTLM;
using Utilities;

namespace SMBLibrary
{
    public class SigningTests
    {
        public static bool TestLMMIC()
        {
            string password = "Password";
            byte[] type1 = new byte[] { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x01, 0x00, 0x00, 0x00, 0x97, 0x82, 0x08, 0xe2,
                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                        0x06, 0x01, 0xb1, 0x1d, 0x00, 0x00, 0x00, 0x0f};
            byte[] type2 = new byte[] { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x02, 0x00, 0x00, 0x00, 0x06, 0x00, 0x06, 0x00,
                                        0x38, 0x00, 0x00, 0x00, 0x95, 0x00, 0x82, 0xa2, 0x28, 0x96, 0xe3, 0x6a, 0xd1, 0xb7, 0x74, 0xb8,
                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x18, 0x00, 0x18, 0x00, 0x3e, 0x00, 0x00, 0x00,
                                        0x05, 0x02, 0xce, 0x0e, 0x00, 0x00, 0x00, 0x0f, 0x54, 0x00, 0x41, 0x00, 0x4c, 0x00, 0x02, 0x00,
                                        0x06, 0x00, 0x54, 0x00, 0x41, 0x00, 0x4c, 0x00, 0x01, 0x00, 0x06, 0x00, 0x54, 0x00, 0x41, 0x00,
                                        0x4c, 0x00, 0x00, 0x00, 0x00, 0x00};
            byte[] type3 = new byte[] { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x03, 0x00, 0x00, 0x00, 0x18, 0x00, 0x18, 0x00,
                                        0x7c, 0x00, 0x00, 0x00, 0x18, 0x00, 0x18, 0x00, 0x94, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x0e, 0x00,
                                        0x58, 0x00, 0x00, 0x00, 0x08, 0x00, 0x08, 0x00, 0x66, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x0e, 0x00,
                                        0x6e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xac, 0x00, 0x00, 0x00, 0x95, 0x00, 0x80, 0xa2,
                                        0x06, 0x01, 0xb1, 0x1d, 0x00, 0x00, 0x00, 0x0f, 0x4e, 0x65, 0x54, 0xe6, 0xb3, 0xdc, 0xdc, 0x16,
                                        0xef, 0xc4, 0xd0, 0x03, 0x3b, 0x81, 0x61, 0x6f, 0x54, 0x00, 0x41, 0x00, 0x4c, 0x00, 0x2d, 0x00,
                                        0x56, 0x00, 0x4d, 0x00, 0x37, 0x00, 0x55, 0x00, 0x73, 0x00, 0x65, 0x00, 0x72, 0x00, 0x54, 0x00,
                                        0x41, 0x00, 0x4c, 0x00, 0x2d, 0x00, 0x56, 0x00, 0x4d, 0x00, 0x37, 0x00, 0xc1, 0xd1, 0x06, 0x56,
                                        0xa3, 0xa9, 0x64, 0x14, 0x0e, 0x7f, 0x43, 0x19, 0x3f, 0x29, 0xf3, 0x72, 0xa3, 0xc1, 0xbe, 0x02,
                                        0xd0, 0x6f, 0xff, 0x20, 0xc1, 0xd1, 0x06, 0x56, 0xa3, 0xa9, 0x64, 0x14, 0x0e, 0x7f, 0x43, 0x19,
                                        0x3f, 0x29, 0xf3, 0x72, 0xa3, 0xc1, 0xbe, 0x02, 0xd0, 0x6f, 0xff, 0x20};

            byte[] serverChallenge = new ChallengeMessage(type2).ServerChallenge;
            AuthenticateMessage authenticateMessage = new AuthenticateMessage(type3);
            byte[] sessionBaseKey = new MD4().GetByteHashFromBytes(NTLMCryptography.NTOWFv1(password));
            byte[] lmowf = NTLMCryptography.LMOWFv1(password);
            byte[] exportedSessionKey = GetExportedSessionKey(sessionBaseKey, authenticateMessage, serverChallenge, lmowf);

            // https://msdn.microsoft.com/en-us/library/cc236695.aspx
            const int micFieldOffset = 72;
            ByteWriter.WriteBytes(type3, micFieldOffset, new byte[16]);
            byte[] temp = ByteUtils.Concatenate(ByteUtils.Concatenate(type1, type2), type3);
            byte[] mic = new HMACMD5(exportedSessionKey).ComputeHash(temp);
            byte[] expected = new byte[] { 0x4e, 0x65, 0x54, 0xe6, 0xb3, 0xdc, 0xdc, 0x16, 0xef, 0xc4, 0xd0, 0x03, 0x3b, 0x81, 0x61, 0x6f };

            return ByteUtils.AreByteArraysEqual(mic, expected);
        }

        public static bool TestNTLMv1MIC()
        {
            string password = "Password";
            byte[] type1 = new byte[] { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x01, 0x00, 0x00, 0x00, 0x97, 0x82, 0x08, 0xe2,
                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                        0x0a, 0x00, 0x39, 0x38, 0x00, 0x00, 0x00, 0x0f};
            byte[] type2 = new byte[] { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x02, 0x00, 0x00, 0x00, 0x06, 0x00, 0x06, 0x00,
                                        0x38, 0x00, 0x00, 0x00, 0x15, 0x02, 0x82, 0xa2, 0xe8, 0xbe, 0x2f, 0x5b, 0xc5, 0xe9, 0xf7, 0xa7,
                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x18, 0x00, 0x18, 0x00, 0x3e, 0x00, 0x00, 0x00,
                                        0x05, 0x02, 0xce, 0x0e, 0x00, 0x00, 0x00, 0x0f, 0x54, 0x00, 0x41, 0x00, 0x4c, 0x00, 0x02, 0x00,
                                        0x06, 0x00, 0x54, 0x00, 0x41, 0x00, 0x4c, 0x00, 0x01, 0x00, 0x06, 0x00, 0x54, 0x00, 0x41, 0x00,
                                        0x4c, 0x00, 0x00, 0x00, 0x00, 0x00};
            byte[] type3 = new byte[] { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x03, 0x00, 0x00, 0x00, 0x18, 0x00, 0x18, 0x00,
                                        0x7c, 0x00, 0x00, 0x00, 0x18, 0x00, 0x18, 0x00, 0x94, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x0e, 0x00,
                                        0x58, 0x00, 0x00, 0x00, 0x08, 0x00, 0x08, 0x00, 0x66, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x0e, 0x00,
                                        0x6e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xac, 0x00, 0x00, 0x00, 0x15, 0x02, 0x80, 0xa2,
                                        0x0a, 0x00, 0x39, 0x38, 0x00, 0x00, 0x00, 0x0f, 0xae, 0xa7, 0xba, 0x44, 0x4e, 0x93, 0xa7, 0xdb,
                                        0xb3, 0x0c, 0x85, 0x49, 0xc2, 0x2b, 0xba, 0x9a, 0x54, 0x00, 0x41, 0x00, 0x4c, 0x00, 0x2d, 0x00,
                                        0x56, 0x00, 0x4d, 0x00, 0x36, 0x00, 0x55, 0x00, 0x73, 0x00, 0x65, 0x00, 0x72, 0x00, 0x54, 0x00,
                                        0x41, 0x00, 0x4c, 0x00, 0x2d, 0x00, 0x56, 0x00, 0x4d, 0x00, 0x36, 0x00, 0xa6, 0x71, 0xbd, 0x94,
                                        0x78, 0x4f, 0x05, 0xf1, 0x3f, 0x3a, 0x7b, 0x41, 0xcf, 0x53, 0x2e, 0x36, 0x73, 0xe2, 0x14, 0x53,
                                        0xbd, 0x42, 0x5e, 0x8f, 0xa6, 0x71, 0xbd, 0x94, 0x78, 0x4f, 0x05, 0xf1, 0x3f, 0x3a, 0x7b, 0x41,
                                        0xcf, 0x53, 0x2e, 0x36, 0x73, 0xe2, 0x14, 0x53, 0xbd, 0x42, 0x5e, 0x8f};

            byte[] serverChallenge = new ChallengeMessage(type2).ServerChallenge;
            AuthenticateMessage authenticateMessage = new AuthenticateMessage(type3);
            byte[] sessionBaseKey = new MD4().GetByteHashFromBytes(NTLMCryptography.NTOWFv1(password));
            byte[] lmowf = NTLMCryptography.LMOWFv1(password);
            byte[] exportedSessionKey = GetExportedSessionKey(sessionBaseKey, authenticateMessage, serverChallenge, lmowf);
            
            // https://msdn.microsoft.com/en-us/library/cc236695.aspx
            const int micFieldOffset = 72;
            ByteWriter.WriteBytes(type3, micFieldOffset, new byte[16]);
            byte[] temp = ByteUtils.Concatenate(ByteUtils.Concatenate(type1, type2), type3);
            byte[] mic = new HMACMD5(exportedSessionKey).ComputeHash(temp);
            byte[] expected = new byte[] { 0xae, 0xa7, 0xba, 0x44, 0x4e, 0x93, 0xa7, 0xdb, 0xb3, 0x0c, 0x85, 0x49, 0xc2, 0x2b, 0xba, 0x9a };

            return ByteUtils.AreByteArraysEqual(mic, expected);
        }

        public static bool TestNTLMv1ExtendedSecurityKeyExchangeMIC()
        {
            string password = "Password";
            byte[] type1 = new byte[] { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x01, 0x00, 0x00, 0x00, 0x97, 0x82, 0x08, 0xe2,
                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                        0x0a, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x0f};
            byte[] type2 = new byte[] { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x02, 0x00, 0x00, 0x00, 0x10, 0x00, 0x10, 0x00,
                                        0x38, 0x00, 0x00, 0x00, 0x15, 0x82, 0x8a, 0xe2, 0x7a, 0x6d, 0x47, 0x52, 0x11, 0x8b, 0x9f, 0x37,
                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x60, 0x00, 0x60, 0x00, 0x48, 0x00, 0x00, 0x00,
                                        0x06, 0x00, 0x71, 0x17, 0x00, 0x00, 0x00, 0x0f, 0x54, 0x00, 0x41, 0x00, 0x4c, 0x00, 0x2d, 0x00,
                                        0x56, 0x00, 0x4d, 0x00, 0x31, 0x00, 0x30, 0x00, 0x02, 0x00, 0x10, 0x00, 0x54, 0x00, 0x41, 0x00,
                                        0x4c, 0x00, 0x2d, 0x00, 0x56, 0x00, 0x4d, 0x00, 0x31, 0x00, 0x30, 0x00, 0x01, 0x00, 0x10, 0x00,
                                        0x54, 0x00, 0x41, 0x00, 0x4c, 0x00, 0x2d, 0x00, 0x56, 0x00, 0x4d, 0x00, 0x31, 0x00, 0x30, 0x00,
                                        0x04, 0x00, 0x10, 0x00, 0x54, 0x00, 0x61, 0x00, 0x6c, 0x00, 0x2d, 0x00, 0x56, 0x00, 0x4d, 0x00,
                                        0x31, 0x00, 0x30, 0x00, 0x03, 0x00, 0x10, 0x00, 0x54, 0x00, 0x61, 0x00, 0x6c, 0x00, 0x2d, 0x00,
                                        0x56, 0x00, 0x4d, 0x00, 0x31, 0x00, 0x30, 0x00, 0x07, 0x00, 0x08, 0x00, 0x28, 0x9a, 0x19, 0xec,
                                        0x8d, 0x92, 0xd2, 0x01, 0x00, 0x00, 0x00, 0x00};
            byte[] type3 = new byte[] { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x03, 0x00, 0x00, 0x00, 0x18, 0x00, 0x18, 0x00,
                                        0x7c, 0x00, 0x00, 0x00, 0x18, 0x00, 0x18, 0x00, 0x94, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x0e, 0x00,
                                        0x58, 0x00, 0x00, 0x00, 0x08, 0x00, 0x08, 0x00, 0x66, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x0e, 0x00,
                                        0x6e, 0x00, 0x00, 0x00, 0x10, 0x00, 0x10, 0x00, 0xac, 0x00, 0x00, 0x00, 0x15, 0x82, 0x88, 0xe2,
                                        0x0a, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x0f, 0xc6, 0x21, 0x82, 0x59, 0x83, 0xda, 0xc7, 0xe7,
                                        0xfa, 0x96, 0x44, 0x67, 0x16, 0xc3, 0xb3, 0x5b, 0x54, 0x00, 0x41, 0x00, 0x4c, 0x00, 0x2d, 0x00,
                                        0x56, 0x00, 0x4d, 0x00, 0x36, 0x00, 0x55, 0x00, 0x73, 0x00, 0x65, 0x00, 0x72, 0x00, 0x54, 0x00,
                                        0x41, 0x00, 0x4c, 0x00, 0x2d, 0x00, 0x56, 0x00, 0x4d, 0x00, 0x36, 0x00, 0x90, 0x13, 0xb0, 0x36,
                                        0xa4, 0xa5, 0xf0, 0x2e, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                        0x00, 0x00, 0x00, 0x00, 0x5c, 0x46, 0xea, 0x77, 0x30, 0x44, 0xee, 0xa5, 0x98, 0x26, 0xa0, 0x93,
                                        0x71, 0x5c, 0x83, 0xff, 0x76, 0x70, 0x1d, 0xf0, 0xb8, 0xa0, 0xad, 0x4d, 0xac, 0xe9, 0xf4, 0x5c,
                                        0x3e, 0xb1, 0xb6, 0x48, 0x08, 0xa0, 0x46, 0x8c, 0x31, 0xe1, 0x2d, 0x60};

            byte[] serverChallenge = new ChallengeMessage(type2).ServerChallenge;
            AuthenticateMessage authenticateMessage = new AuthenticateMessage(type3);
            byte[] sessionBaseKey = new MD4().GetByteHashFromBytes(NTLMCryptography.NTOWFv1(password));
            byte[] lmowf = NTLMCryptography.LMOWFv1(password);
            byte[] exportedSessionKey = GetExportedSessionKey(sessionBaseKey, authenticateMessage, serverChallenge, lmowf);

            // https://msdn.microsoft.com/en-us/library/cc236695.aspx
            const int micFieldOffset = 72;
            ByteWriter.WriteBytes(type3, micFieldOffset, new byte[16]);
            byte[] temp = ByteUtils.Concatenate(ByteUtils.Concatenate(type1, type2), type3);
            byte[] mic = new HMACMD5(exportedSessionKey).ComputeHash(temp);
            byte[] expected = new byte[] { 0xc6, 0x21, 0x82, 0x59, 0x83, 0xda, 0xc7, 0xe7, 0xfa, 0x96, 0x44, 0x67, 0x16, 0xc3, 0xb3, 0x5b };

            return ByteUtils.AreByteArraysEqual(mic, expected);
        }

        public static bool TestNTLMv2KeyExchangeMIC()
        {
            byte[] responseKeyNT = NTLMCryptography.NTOWFv2("Password", "User", "TAL-VM6");
            byte[] type1 = new byte[] { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x01, 0x00, 0x00, 0x00, 0x97, 0x82, 0x08, 0xe2,
                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                        0x0a, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x0f};
            byte[] type2 = new byte[] { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x02, 0x00, 0x00, 0x00, 0x10, 0x00, 0x10, 0x00,
                                        0x38, 0x00, 0x00, 0x00, 0x15, 0x82, 0x8a, 0xe2, 0x63, 0x74, 0x79, 0x77, 0xe1, 0xea, 0x35, 0x51,
                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x60, 0x00, 0x60, 0x00, 0x48, 0x00, 0x00, 0x00,
                                        0x06, 0x00, 0x71, 0x17, 0x00, 0x00, 0x00, 0x0f, 0x54, 0x00, 0x41, 0x00, 0x4c, 0x00, 0x2d, 0x00,
                                        0x56, 0x00, 0x4d, 0x00, 0x31, 0x00, 0x30, 0x00, 0x02, 0x00, 0x10, 0x00, 0x54, 0x00, 0x41, 0x00,
                                        0x4c, 0x00, 0x2d, 0x00, 0x56, 0x00, 0x4d, 0x00, 0x31, 0x00, 0x30, 0x00, 0x01, 0x00, 0x10, 0x00,
                                        0x54, 0x00, 0x41, 0x00, 0x4c, 0x00, 0x2d, 0x00, 0x56, 0x00, 0x4d, 0x00, 0x31, 0x00, 0x30, 0x00,
                                        0x04, 0x00, 0x10, 0x00, 0x54, 0x00, 0x61, 0x00, 0x6c, 0x00, 0x2d, 0x00, 0x56, 0x00, 0x4d, 0x00,
                                        0x31, 0x00, 0x30, 0x00, 0x03, 0x00, 0x10, 0x00, 0x54, 0x00, 0x61, 0x00, 0x6c, 0x00, 0x2d, 0x00,
                                        0x56, 0x00, 0x4d, 0x00, 0x31, 0x00, 0x30, 0x00, 0x07, 0x00, 0x08, 0x00, 0x1f, 0x8a, 0xd4, 0xff,
                                        0x01, 0x91, 0xd2, 0x01, 0x00, 0x00, 0x00, 0x00};
            byte[] type3 = new byte[] { 0x4e, 0x54, 0x4c, 0x4d, 0x53, 0x53, 0x50, 0x00, 0x03, 0x00, 0x00, 0x00, 0x18, 0x00, 0x18, 0x00,
                                        0x7c, 0x00, 0x00, 0x00, 0x02, 0x01, 0x02, 0x01, 0x94, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x0e, 0x00,
                                        0x58, 0x00, 0x00, 0x00, 0x08, 0x00, 0x08, 0x00, 0x66, 0x00, 0x00, 0x00, 0x0e, 0x00, 0x0e, 0x00,
                                        0x6e, 0x00, 0x00, 0x00, 0x10, 0x00, 0x10, 0x00, 0x96, 0x01, 0x00, 0x00, 0x15, 0x82, 0x88, 0xe2,
                                        0x0a, 0x00, 0x00, 0x28, 0x00, 0x00, 0x00, 0x0f, 0x82, 0x3c, 0xff, 0x48, 0xa9, 0x03, 0x13, 0x4c,
                                        0x33, 0x3c, 0x09, 0x87, 0xf3, 0x16, 0x59, 0x89, 0x54, 0x00, 0x41, 0x00, 0x4c, 0x00, 0x2d, 0x00,
                                        0x56, 0x00, 0x4d, 0x00, 0x36, 0x00, 0x55, 0x00, 0x73, 0x00, 0x65, 0x00, 0x72, 0x00, 0x54, 0x00,
                                        0x41, 0x00, 0x4c, 0x00, 0x2d, 0x00, 0x56, 0x00, 0x4d, 0x00, 0x36, 0x00, 0x00, 0x00, 0x00, 0x00,
                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                        0x00, 0x00, 0x00, 0x00, 0xb3, 0x06, 0x65, 0xe3, 0x9f, 0x03, 0xe1, 0xc3, 0xd8, 0x28, 0x7c, 0x9c,
                                        0x35, 0x0d, 0x32, 0x4c, 0x01, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x1f, 0x8a, 0xd4, 0xff,
                                        0x01, 0x91, 0xd2, 0x01, 0x77, 0x71, 0x91, 0x94, 0xb1, 0x6e, 0x66, 0x28, 0x00, 0x00, 0x00, 0x00,
                                        0x02, 0x00, 0x10, 0x00, 0x54, 0x00, 0x41, 0x00, 0x4c, 0x00, 0x2d, 0x00, 0x56, 0x00, 0x4d, 0x00,
                                        0x31, 0x00, 0x30, 0x00, 0x01, 0x00, 0x10, 0x00, 0x54, 0x00, 0x41, 0x00, 0x4c, 0x00, 0x2d, 0x00,
                                        0x56, 0x00, 0x4d, 0x00, 0x31, 0x00, 0x30, 0x00, 0x04, 0x00, 0x10, 0x00, 0x54, 0x00, 0x61, 0x00,
                                        0x6c, 0x00, 0x2d, 0x00, 0x56, 0x00, 0x4d, 0x00, 0x31, 0x00, 0x30, 0x00, 0x03, 0x00, 0x10, 0x00,
                                        0x54, 0x00, 0x61, 0x00, 0x6c, 0x00, 0x2d, 0x00, 0x56, 0x00, 0x4d, 0x00, 0x31, 0x00, 0x30, 0x00,
                                        0x07, 0x00, 0x08, 0x00, 0x1f, 0x8a, 0xd4, 0xff, 0x01, 0x91, 0xd2, 0x01, 0x06, 0x00, 0x04, 0x00,
                                        0x02, 0x00, 0x00, 0x00, 0x08, 0x00, 0x30, 0x00, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                        0x01, 0x00, 0x00, 0x00, 0x00, 0x20, 0x00, 0x00, 0x19, 0x0d, 0x73, 0xca, 0x97, 0x30, 0x2a, 0xa7,
                                        0x7a, 0x1f, 0xb6, 0xad, 0xe2, 0xe5, 0x4a, 0x59, 0x4a, 0x93, 0x7e, 0x37, 0xcd, 0x0c, 0xd7, 0x90,
                                        0x25, 0xc4, 0xaf, 0x8a, 0x17, 0x99, 0x69, 0x56, 0x0a, 0x00, 0x10, 0x00, 0x00, 0x00, 0x00, 0x00,
                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x00, 0x1a, 0x00,
                                        0x63, 0x00, 0x69, 0x00, 0x66, 0x00, 0x73, 0x00, 0x2f, 0x00, 0x54, 0x00, 0x61, 0x00, 0x6c, 0x00,
                                        0x2d, 0x00, 0x56, 0x00, 0x4d, 0x00, 0x31, 0x00, 0x30, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
                                        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x71, 0x7c, 0xce, 0x0b, 0x92, 0x46, 0x46, 0x0d, 0x5b, 0x3b,
                                        0x11, 0xb4, 0xde, 0x86, 0x28, 0x11};

            byte[] serverChallenge = new ChallengeMessage(type2).ServerChallenge;
            AuthenticateMessage authenticateMessage = new AuthenticateMessage(type3);
            byte[] ntProofStr = ByteReader.ReadBytes(authenticateMessage.NtChallengeResponse, 0, 16);
            byte[] sessionBaseKey = new HMACMD5(responseKeyNT).ComputeHash(ntProofStr);
            byte[] exportedSessionKey = GetExportedSessionKey(sessionBaseKey, authenticateMessage, serverChallenge, null);

            // https://msdn.microsoft.com/en-us/library/cc236695.aspx
            const int micFieldOffset = 72;
            ByteWriter.WriteBytes(type3, micFieldOffset, new byte[16]);
            byte[] temp = ByteUtils.Concatenate(ByteUtils.Concatenate(type1, type2), type3);
            byte[] mic = new HMACMD5(exportedSessionKey).ComputeHash(temp);
            byte[] expected = new byte[] { 0x82, 0x3c, 0xff, 0x48, 0xa9, 0x03, 0x13, 0x4c, 0x33, 0x3c, 0x09, 0x87, 0xf3, 0x16, 0x59, 0x89 };

            return ByteUtils.AreByteArraysEqual(mic, expected);
        }

        private static byte[] GetExportedSessionKey(byte[] sessionBaseKey, AuthenticateMessage message, byte[] serverChallenge, byte[] lmowf)
        {
            byte[] keyExchangeKey;
            if (AuthenticationMessageUtils.IsNTLMv2NTResponse(message.NtChallengeResponse))
            {
                keyExchangeKey = sessionBaseKey;
            }
            else
            {
                keyExchangeKey = NTLMCryptography.KXKey(sessionBaseKey, message.NegotiateFlags, message.LmChallengeResponse, serverChallenge, lmowf);
            }

            if ((message.NegotiateFlags & NegotiateFlags.KeyExchange) > 0)
            {
                return RC4.Decrypt(keyExchangeKey, message.EncryptedRandomSessionKey);
            }
            else
            {
                return keyExchangeKey;
            }
        }   
    }
}
