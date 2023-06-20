namespace LChocolate
{   
    public class LSecurity
    {
        public static System.String LSECURITY_KEY_STRING = "\\_##Korea tritonesoft##_\\";
        private static System.Byte[] LSECURITY_KEY = null;

        public static System.Boolean LSecuirty_EncryptDecrypt(ref System.Byte[] pInOutData, System.UInt16 offset, System.UInt16 dataSize, System.String securityKeyString, System.Byte securityKeyLen)	// 敗呪誤亀 郊蚊醤 馬澗汽, 益軍 紫遂馬奄 毘給 ぱせ
		{
			if (null == pInOutData || 1 > dataSize)
				return false;
            
            System.Byte[] tempSecurityKey;                                    // 戚 痕呪誤亀 需奄奄 是背辞 章硲鉢櫛 穿粕 淫域蒸澗 汝骨廃 戚硯生稽 梅製.

            System.Byte contentLength = 0;
			if (null != securityKeyString)
			{
				if (1 > securityKeyLen)
					return false;

                if (!LString.ConvertStringToByteArray(ref securityKeyString, out tempSecurityKey))
                    return false;
                contentLength = securityKeyLen;
			}
			else
			{
                if (null == LSECURITY_KEY)
                {
                    if (!LString.ConvertStringToByteArray(ref LSECURITY_KEY_STRING, out LSECURITY_KEY))
                        return false;
                }

                tempSecurityKey = LSECURITY_KEY;
                contentLength = (System.Byte)tempSecurityKey.Length;
            }

			for (System.UInt16 i = 0; dataSize > i; ++i)
			{
                pInOutData[i + offset] = (System.Byte)(pInOutData[i + offset] ^ tempSecurityKey[i % contentLength]);
			}

			return true;
		}

        /*
        public static System.Boolean LSecuirty_EncryptDecrypt(ref System.Byte[] pInOutData, System.UInt16 dataSize, System.String securityKeyString, System.Byte securityKeyLen)  // 敗呪誤亀 郊蚊醤 馬澗汽, 益軍 紫遂馬奄 毘給 ぱせ
        {
            if (null == pInOutData || 1 > dataSize)
                return false;

            System.Byte[] tempSecurityKey;                                    // 戚 痕呪誤亀 ?皹痿是背辞 章硲鉢櫛 穿粕 淫域蒸澗 汝骨廃 戚硯生稽 梅製.

            System.Byte contentLength = 0;
            if (null != securityKeyString)
            {
                if (1 > securityKeyLen)
                    return false;

                if (!LString.ConvertStringToByteArray(ref securityKeyString, out tempSecurityKey))
                    return false;
                contentLength = securityKeyLen;
            }
            else
            {
                if (null == LSECURITY_KEY)
                {
                    if (!LString.ConvertStringToByteArray(ref LSECURITY_KEY_STRING, out LSECURITY_KEY))
                        return false;
                }

                tempSecurityKey = LSECURITY_KEY;
                contentLength = (System.Byte)tempSecurityKey.Length;
            }
            
            for (System.UInt16 i = 0; dataSize > i; ++i)
            {
                pInOutData[i] = (System.Byte)(pInOutData[i] ^ tempSecurityKey[i % contentLength]);
            }

            return true;
        }
        */

        public static System.Boolean LSecuirty_EncryptDecrypt(ref System.Byte[] pInOutData, System.UInt16 dataSize, System.Byte[] securityKey, System.Byte securityKeyLen)  // 敗呪誤亀 郊蚊醤 馬澗汽, 益軍 紫遂馬奄 毘給 ぱせ
        {
            if (null == pInOutData || 1 > dataSize)
                return false;

            System.Byte[] tempSecurityKey;                                    // 戚 痕呪誤亀 需奄奄 是背辞 章硲鉢櫛 穿粕 淫域蒸澗 汝骨廃 戚硯生稽 梅製.

            System.Byte contentLength = 0;
            if (null != securityKey)
            {
                if (1 > securityKeyLen)
                    return false;

                tempSecurityKey = securityKey;
                contentLength = securityKeyLen;
            }
            else
            {
                if (null == LSECURITY_KEY)
                {
                    if (!LString.ConvertStringToByteArray(ref LSECURITY_KEY_STRING, out LSECURITY_KEY))
                        return false;
                }

                tempSecurityKey = LSECURITY_KEY;
                contentLength = (System.Byte)tempSecurityKey.Length;
            }

            for (System.UInt16 i = 0; dataSize > i; ++i)
            {
                pInOutData[i] = (System.Byte)(pInOutData[i] ^ tempSecurityKey[i % contentLength]);
            }

            return true;
        }



        /*
        public static System.Boolean LSecuirty_EncryptDecrypt(ref System.Byte[] pInOutData, System.UInt16 dataSize, System.Byte[] securityKey, System.Byte securityKeyLen)  // 敗呪誤亀 郊蚊醤 馬澗汽, 益軍 紫遂馬奄 毘給 ぱせ
        {
            if (null == pInOutData || 1 > dataSize)
                return false;

            System.Byte[] tempSecurityKey;                                    // 戚 痕呪誤亀 需奄奄 是背辞 章硲鉢櫛 穿粕 淫域蒸澗 汝骨廃 戚硯生稽 梅製.

            System.Byte contentLength = 0;
            if (null != securityKey)
            {
                if (1 > securityKeyLen)
                    return false;
                tempSecurityKey = null;
                System.Array.Copy(securityKey, tempSecurityKey, securityKeyLen);
                contentLength = securityKeyLen;
            }
            else
            {
                if (null == LSECURITY_KEY)
                {
                    if (!LString.ConvertStringToByteArray(ref LSECURITY_KEY_STRING, out LSECURITY_KEY))
                        return false;
                }

                tempSecurityKey = LSECURITY_KEY;
                contentLength = (System.Byte)tempSecurityKey.Length;
            }

            for (System.UInt16 i = 0; dataSize > i; ++i)
            {
                pInOutData[i] = (System.Byte)(pInOutData[i] ^ tempSecurityKey[i % contentLength]);
            }

            return true;
        }
         */
    }
}