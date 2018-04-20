using System;
using Force.Crc32;

namespace IronLeveldb
{
    internal static class Crc32
    {
        private const uint MaskDelta = 0xa282ead8;

        /*
        static const uint32_t kMaskDelta = 0xa282ead8ul;

        // Return a masked representation of crc.
        //
        // Motivation: it is problematic to compute the CRC of a string that
        // contains embedded CRCs.  Therefore we recommend that CRCs stored
        // somewhere (e.g., in files) should be masked before being stored.
        inline uint32_t Mask(uint32_t crc) {
            // Rotate right by 15 bits and add a constant.
            return ((crc >> 15) | (crc << 17)) + kMaskDelta;
        }

        // Return the crc whose masked representation is masked_crc.
        inline uint32_t Unmask(uint32_t masked_crc) {
            uint32_t rot = masked_crc - kMaskDelta;
            return ((rot >> 17) | (rot << 15));
        }
        */

        public static uint Unmask(uint maskedCrc)
        {
            var rot = maskedCrc - MaskDelta;
            return (rot >> 17) | (rot << 15);
        }

        public static uint ReadUnmaskCrc(byte[] data, int offset)
        {
            return Unmask(BitConverter.ToUInt32(data, offset));
        }

        public static uint Value(byte[] data, int offset, int size)
        {
            return Crc32CAlgorithm.Compute(data, 0, size);
        }
    }
}
