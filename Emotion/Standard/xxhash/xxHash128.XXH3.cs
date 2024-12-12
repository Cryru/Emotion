// ReSharper disable InconsistentNaming

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Standart.Hash.xxHash
{
    public static partial class xxHash128
    {
        private static readonly byte[] XXH3_SECRET =
        {
            0xb8, 0xfe, 0x6c, 0x39, 0x23, 0xa4, 0x4b, 0xbe, 0x7c, 0x01, 0x81, 0x2c, 0xf7, 0x21, 0xad, 0x1c,
            0xde, 0xd4, 0x6d, 0xe9, 0x83, 0x90, 0x97, 0xdb, 0x72, 0x40, 0xa4, 0xa4, 0xb7, 0xb3, 0x67, 0x1f,
            0xcb, 0x79, 0xe6, 0x4e, 0xcc, 0xc0, 0xe5, 0x78, 0x82, 0x5a, 0xd0, 0x7d, 0xcc, 0xff, 0x72, 0x21,
            0xb8, 0x08, 0x46, 0x74, 0xf7, 0x43, 0x24, 0x8e, 0xe0, 0x35, 0x90, 0xe6, 0x81, 0x3a, 0x26, 0x4c,
            0x3c, 0x28, 0x52, 0xbb, 0x91, 0xc3, 0x00, 0xcb, 0x88, 0xd0, 0x65, 0x8b, 0x1b, 0x53, 0x2e, 0xa3,
            0x71, 0x64, 0x48, 0x97, 0xa2, 0x0d, 0xf9, 0x4e, 0x38, 0x19, 0xef, 0x46, 0xa9, 0xde, 0xac, 0xd8,
            0xa8, 0xfa, 0x76, 0x3f, 0xe3, 0x9c, 0x34, 0x3f, 0xf9, 0xdc, 0xbb, 0xc7, 0xc7, 0x0b, 0x4f, 0x1d,
            0x8a, 0x51, 0xe0, 0x4b, 0xcd, 0xb4, 0x59, 0x31, 0xc8, 0x9f, 0x7e, 0xc9, 0xd9, 0x78, 0x73, 0x64,
            0xea, 0xc5, 0xac, 0x83, 0x34, 0xd3, 0xeb, 0xc3, 0xc5, 0x81, 0xa0, 0xff, 0xfa, 0x13, 0x63, 0xeb,
            0x17, 0x0d, 0xdd, 0x51, 0xb7, 0xf0, 0xda, 0x49, 0xd3, 0x16, 0x55, 0x26, 0x29, 0xd4, 0x68, 0x9e,
            0x2b, 0x16, 0xbe, 0x58, 0x7d, 0x47, 0xa1, 0xfc, 0x8f, 0xf8, 0xb8, 0xd1, 0x7a, 0xd0, 0x31, 0xce,
            0x45, 0xcb, 0x3a, 0x8f, 0x95, 0x16, 0x04, 0x28, 0xaf, 0xd7, 0xfb, 0xca, 0xbb, 0x4b, 0x40, 0x7e,
        };
        
        private static readonly ulong[] XXH3_INIT_ACC =
        {
            XXH_PRIME32_3, XXH_PRIME64_1, XXH_PRIME64_2, XXH_PRIME64_3,
            XXH_PRIME64_4, XXH_PRIME32_2, XXH_PRIME64_5, XXH_PRIME32_1
        };

        private static readonly int XXH3_SECRET_SIZE_MIN = 136;
        private static readonly int XXH3_SECRET_DEFAULT_SIZE = 192;
        private static readonly int XXH3_MIDSIZE_MAX = 240;
        private static readonly int XXH3_MIDSIZE_STARTOFFSET = 3;
        private static readonly int XXH3_MIDSIZE_LASTOFFSET = 17;
        private static readonly int XXH3_ACC_SIZE = 64;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint128 XXH3_128bits_internal(byte* input, int len, ulong seed, byte* secret, int secretLen)
        {
            Debug.Assert(secretLen >= XXH3_SECRET_SIZE_MIN);

            if (len <= 16)
                return XXH3_len_0to16_128b(input, len, secret, seed);
            if (len <= 128)
                return XXH3_len_17to128_128b(input, len, secret, secretLen, seed);
            if (len <= XXH3_MIDSIZE_MAX)
                return XXH3_len_129to240_128b(input, len, secret, secretLen, seed);

            return XXH3_hashLong_128b_withSeed(input, len, secret, secretLen, seed);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint128 XXH3_len_0to16_128b(byte* input, int len, byte* secret, ulong seed)
        {
            Debug.Assert(len <= 16);

            if (len > 8) return XXH3_len_9to16_128b(input, len, secret, seed);
            if (len >= 4) return XXH3_len_4to8_128b(input, len, secret, seed);
            if (len != 0) return XXH3_len_1to3_128b(input, len, secret, seed);

            uint128 h128;
            ulong bitflipl = XXH_readLE64(secret + 64) ^ XXH_readLE64(secret + 72);
            ulong bitfliph = XXH_readLE64(secret + 80) ^ XXH_readLE64(secret + 88);
            h128.low64 = XXH64_avalanche(seed ^ bitflipl);
            h128.high64 = XXH64_avalanche(seed ^ bitfliph);
            return h128;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint128 XXH3_len_17to128_128b(byte* input, int len, byte* secret, int secretSize, ulong seed)
        {
            Debug.Assert(secretSize >=  XXH3_SECRET_SIZE_MIN);
            Debug.Assert(16 < len && len <= 128);
        
            uint128 acc;
            acc.low64 = (ulong) len * XXH_PRIME64_1;
            acc.high64 = 0;
            
            if (len > 32) {
                if (len > 64) {
                    if (len > 96) {
                        acc = XXH128_mix32B(acc, input+48, input+len-64, secret+96, seed);
                    }
                    acc = XXH128_mix32B(acc, input+32, input+len-48, secret+64, seed);
                }
                acc = XXH128_mix32B(acc, input+16, input+len-32, secret+32, seed);
            }
            acc = XXH128_mix32B(acc, input, input+len-16, secret, seed);

            uint128 h128;
            h128.low64 = acc.low64 + acc.high64;
            h128.high64 = (acc.low64  * XXH_PRIME64_1)
                          + (acc.high64   * XXH_PRIME64_4)
                          + (((ulong) len - seed) * XXH_PRIME64_2);
            h128.low64  = XXH3_avalanche(h128.low64);
            h128.high64 = (ulong) 0 - XXH3_avalanche(h128.high64);
            return h128;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint128 XXH3_len_9to16_128b(byte* input, int len, byte* secret, ulong seed)
        {
            Debug.Assert(input != null);
            Debug.Assert(secret != null);
            Debug.Assert(9 <= len && len <= 16);

            ulong bitflipl = (XXH_readLE64(secret + 32) ^ XXH_readLE64(secret + 40)) - seed;
            ulong bitfliph = (XXH_readLE64(secret + 48) ^ XXH_readLE64(secret + 56)) + seed;
            ulong input_lo = XXH_readLE64(input);
            ulong input_hi = XXH_readLE64(input + len - 8);
            uint128 m128 = XXH_mult64to128(input_lo ^ input_hi ^ bitflipl, XXH_PRIME64_1);

            m128.low64 += (ulong) (len - 1) << 54;
            input_hi ^= bitfliph;

            m128.high64 += input_hi + XXH_mult32to64((uint) input_hi, XXH_PRIME32_2 - 1);
            m128.low64 ^= XXH_swap64(m128.high64);

            uint128 h128 = XXH_mult64to128(m128.low64, XXH_PRIME64_2);
            h128.high64 += m128.high64 * XXH_PRIME64_2;

            h128.low64 = XXH3_avalanche(h128.low64);
            h128.high64 = XXH3_avalanche(h128.high64);
            return h128;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint128 XXH3_len_1to3_128b(byte* input, int len, byte* secret, ulong seed)
        {
            Debug.Assert(input != null);
            Debug.Assert(1 <= len && len <= 3);
            Debug.Assert(secret != null);

            byte c1 = input[0];
            byte c2 = input[len >> 1];
            byte c3 = input[len - 1];

            uint combinedl = ((uint) c1 << 16) |
                             ((uint) c2 << 24) |
                             ((uint) c3 << 0) |
                             ((uint) len << 8);
            uint combinedh = XXH_rotl32(XXH_swap32(combinedl), 13);

            ulong bitflipl = (XXH_readLE32(secret) ^ XXH_readLE32(secret + 4)) + seed;
            ulong bitfliph = (XXH_readLE32(secret + 8) ^ XXH_readLE32(secret + 12)) - seed;
            ulong keyed_lo = (ulong) combinedl ^ bitflipl;
            ulong keyed_hi = (ulong) combinedh ^ bitfliph;

            uint128 h128;
            h128.low64 = XXH64_avalanche(keyed_lo);
            h128.high64 = XXH64_avalanche(keyed_hi);

            return h128;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint128 XXH3_len_4to8_128b(byte* input, int len, byte* secret, ulong seed)
        {
            Debug.Assert(input != null);
            Debug.Assert(secret != null);
            Debug.Assert(4 <= len && len <= 8);

            seed ^= (ulong) XXH_swap32((uint) seed) << 32;

            uint input_lo = XXH_readLE32(input);
            uint input_hi = XXH_readLE32(input + len - 4);
            ulong input_64 = input_lo + ((ulong) input_hi << 32);
            ulong bitflip = (XXH_readLE64(secret + 16) ^  XXH_readLE64(secret + 24)) + seed;
            ulong keyed = input_64 ^ bitflip;

            uint128 m128 = XXH_mult64to128(keyed,  XXH_PRIME64_1 + ((ulong) len << 2));

            m128.high64 += (m128.low64 << 1);
            m128.low64 ^= (m128.high64 >> 3);

            m128.low64 =  XXH_xorshift64(m128.low64, 35);
            m128.low64 *= 0x9FB21C651E98DF25UL;
            m128.low64 =  XXH_xorshift64(m128.low64, 28);
            m128.high64 = XXH3_avalanche(m128.high64);

            return m128;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint128 XXH3_len_129to240_128b(byte* input, int len, byte* secret, int secretSize, ulong seed)
        {
            Debug.Assert(secretSize >= XXH3_SECRET_SIZE_MIN);
            Debug.Assert(128 < len && len <= XXH3_MIDSIZE_MAX);

            uint128 acc;
            int nbRounds = len / 32;
            
            acc.low64 = (ulong) len * XXH_PRIME64_1;
            acc.high64 = 0;
            for (int i = 0; i < 4; i++) {
                acc = XXH128_mix32B(acc,
                    input  + (32 * i),
                    input  + (32 * i) + 16,
                    secret + (32 * i),
                    seed);
            }
            
            acc.low64 = XXH3_avalanche(acc.low64);
            acc.high64 = XXH3_avalanche(acc.high64);

            for (int i = 4 ; i < nbRounds; i++) {
                acc = XXH128_mix32B(acc,
                    input + (32 * i),
                    input + (32 * i) + 16,
                    secret + XXH3_MIDSIZE_STARTOFFSET + (32 * (i - 4)),
                    seed);
            }
            
            acc = XXH128_mix32B(acc,
                input + len - 16,
                input + len - 32,
                secret + XXH3_SECRET_SIZE_MIN - XXH3_MIDSIZE_LASTOFFSET - 16,
                0UL - seed);
            
            uint128 h128;
            h128.low64  = acc.low64 + acc.high64;
            h128.high64 = (acc.low64    * XXH_PRIME64_1)
                          + (acc.high64   * XXH_PRIME64_4)
                          + (((ulong)len - seed) * XXH_PRIME64_2);
            h128.low64  = XXH3_avalanche(h128.low64);
            h128.high64 = (ulong)0 - XXH3_avalanche(h128.high64);
            return h128;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong XXH3_avalanche(ulong h64)
        {
            h64 = XXH_xorshift64(h64, 37);
            h64 *= 0x165667919E3779F9UL;
            h64 = XXH_xorshift64(h64, 32);
            return h64;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint128 XXH128_mix32B(uint128 acc, byte* input_1, byte* input_2, byte* secret, ulong seed)
        {
            acc.low64 += XXH3_mix16B(input_1, secret + 0, seed);
            acc.low64 ^= XXH_readLE64(input_2) + XXH_readLE64(input_2 + 8);
            acc.high64 += XXH3_mix16B(input_2, secret + 16, seed);
            acc.high64 ^= XXH_readLE64(input_1) + XXH_readLE64(input_1 + 8);
            return acc;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe ulong XXH3_mix16B(byte* input, byte* secret, ulong seed)
        {
            ulong input_lo = XXH_readLE64(input);
            ulong input_hi = XXH_readLE64(input + 8);

            return XXH3_mul128_fold64(
                input_lo ^ (XXH_readLE64(secret) + seed),
                input_hi ^ (XXH_readLE64(secret + 8) - seed)
            );
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ulong XXH3_mul128_fold64(ulong lhs, ulong rhs)
        {
            uint128 product = XXH_mult64to128(lhs, rhs);
            return product.low64 ^ product.high64;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint128 XXH3_hashLong_128b_withSeed(byte* input, int len, byte* secret, int secretSize, ulong seed)
        {
            if (seed == 0)
                return XXH3_hashLong_128b_internal(input, len, secret, secretSize);

            byte* customSecret = stackalloc byte[XXH3_SECRET_DEFAULT_SIZE];

            XXH3_initCustomSecret(customSecret, seed);

            return XXH3_hashLong_128b_internal(input, len, customSecret, XXH3_SECRET_DEFAULT_SIZE);

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe uint128 XXH3_hashLong_128b_internal(byte* input, int len, byte* secret, int secretSize)
        {
            fixed (ulong* src = &XXH3_INIT_ACC[0])
            {
                ulong* acc = stackalloc ulong[8]
                {
                    *(src + 0),
                    *(src + 1),
                    *(src + 2),
                    *(src + 3),
                    *(src + 4),
                    *(src + 5),
                    *(src + 6),
                    *(src + 7),
                };

                XXH3_hashLong_internal_loop(acc, input, len, secret, secretSize);

                uint128 uint128;
                uint128.low64 = XXH3_mergeAccs(acc,
                    secret + XXH_SECRET_MERGEACCS_START,
                    (ulong)len * XXH_PRIME64_1);
                uint128.high64 = XXH3_mergeAccs(acc,
                    secret + secretSize - XXH3_ACC_SIZE - XXH_SECRET_MERGEACCS_START,
                    ~((ulong)len * XXH_PRIME64_2));

                return uint128;
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void XXH3_hashLong_internal_loop(ulong* acc, byte* input, int len, byte* secret, int secretSize)
        {
            Debug.Assert(secretSize >= XXH3_SECRET_SIZE_MIN);
            Debug.Assert(len > XXH_STRIPE_LEN);

            int nbStripesPerBlock = (secretSize - XXH_STRIPE_LEN) / XXH_SECRET_CONSUME_RATE;
            int block_len = XXH_STRIPE_LEN * nbStripesPerBlock;
            int nb_blocks = (len - 1) / block_len;

            for (int n = 0; n < nb_blocks; n++) {
                XXH3_accumulate(acc, input + n * block_len, secret, nbStripesPerBlock);
                XXH3_scrambleAcc(acc, secret + secretSize - XXH_STRIPE_LEN);
            }
            
            int nbStripes = ((len - 1) - (block_len * nb_blocks)) / XXH_STRIPE_LEN;
            XXH3_accumulate(acc, input + nb_blocks * block_len, secret, nbStripes);
            
            byte* p = input + len - XXH_STRIPE_LEN;
            XXH3_accumulate_512(acc, p, secret + secretSize - XXH_STRIPE_LEN - XXH_SECRET_LASTACC_START);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe ulong XXH3_mergeAccs(ulong* acc, byte* secret, ulong start)
        {
            ulong result64 = start;
            
            for (int i = 0; i < 4; i++)
                result64 += XXH3_mix2Accs(acc + 2 * i, secret + 16 * i);

            return XXH3_avalanche(result64);
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe ulong XXH3_mix2Accs(ulong* acc, byte* secret)
        {
            return XXH3_mul128_fold64(
                acc[0] ^ XXH_readLE64(secret),
                acc[1] ^ XXH_readLE64(secret+8) );
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void XXH3_accumulate(ulong* acc, byte* input, byte* secret, int nbStripes)
        {
            for (int n = 0; n < nbStripes; n++ ) {
                byte* inp = input + n * XXH_STRIPE_LEN;
                XXH3_accumulate_512(acc, inp, secret + n * XXH_SECRET_CONSUME_RATE);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void XXH3_accumulate_512(ulong* acc, byte* input, byte* secret)
        {
            if (Avx2.IsSupported)
                XXH3_accumulate_512_avx2(acc, input, secret);
            else if (Sse2.IsSupported)
                XXH3_accumulate_512_sse2(acc, input, secret);
            else
                XXH3_accumulate_512_scalar(acc, input, secret);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void XXH3_accumulate_512_avx2(ulong* acc, byte* input, byte* secret)
        {
            var acc_vec0 = Unsafe.Read<Vector256<ulong>>(acc + 0);
            var acc_vec1 = Unsafe.Read<Vector256<ulong>>(acc + 4);

            var data_vec0 = Unsafe.Read<Vector256<ulong>>((ulong*)input + 0).AsUInt32();
            var data_vec1 = Unsafe.Read<Vector256<ulong>>((ulong*)input + 4).AsUInt32();

            var key_vec0 = Unsafe.Read<Vector256<ulong>>((ulong*)secret + 0).AsUInt32();
            var key_vec1 = Unsafe.Read<Vector256<ulong>>((ulong*)secret + 4).AsUInt32();

            var data_key0 = Avx2.Xor(data_vec0, key_vec0);
            var data_key1 = Avx2.Xor(data_vec1, key_vec1);

            var data_key_lo0 = Avx2.Shuffle(data_key0, MM_SHUFFLE_0_3_0_1);
            var data_key_lo1 = Avx2.Shuffle(data_key1, MM_SHUFFLE_0_3_0_1);

            var product0 = Avx2.Multiply(data_key0, data_key_lo0);
            var product1 = Avx2.Multiply(data_key1, data_key_lo1);

            var data_swap0 = Avx2.Shuffle(data_vec0, MM_SHUFFLE_1_0_3_2).AsUInt64();
            var data_swap1 = Avx2.Shuffle(data_vec1, MM_SHUFFLE_1_0_3_2).AsUInt64();

            var sum0 = Avx2.Add(acc_vec0, data_swap0);
            var sum1 = Avx2.Add(acc_vec1, data_swap1);

            var result0 = Avx2.Add(product0, sum0);
            var result1 = Avx2.Add(product1, sum1);

            Unsafe.Write(acc + 0, result0);
            Unsafe.Write(acc + 4, result1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void XXH3_accumulate_512_sse2(ulong* acc, byte* input, byte* secret)
        {
            var acc_vec0 = Unsafe.Read<Vector128<ulong>>(acc + 0);
            var acc_vec1 = Unsafe.Read<Vector128<ulong>>(acc + 2);
            var acc_vec2 = Unsafe.Read<Vector128<ulong>>(acc + 4);
            var acc_vec3 = Unsafe.Read<Vector128<ulong>>(acc + 6);

            var data_vec0 = Unsafe.Read<Vector128<ulong>>((ulong*)input + 0).AsUInt32();
            var data_vec1 = Unsafe.Read<Vector128<ulong>>((ulong*)input + 2).AsUInt32();
            var data_vec2 = Unsafe.Read<Vector128<ulong>>((ulong*)input + 4).AsUInt32();
            var data_vec3 = Unsafe.Read<Vector128<ulong>>((ulong*)input + 6).AsUInt32();

            var key_vec0 = Unsafe.Read<Vector128<ulong>>((ulong*)secret + 0).AsUInt32();
            var key_vec1 = Unsafe.Read<Vector128<ulong>>((ulong*)secret + 2).AsUInt32();
            var key_vec2 = Unsafe.Read<Vector128<ulong>>((ulong*)secret + 4).AsUInt32();
            var key_vec3 = Unsafe.Read<Vector128<ulong>>((ulong*)secret + 6).AsUInt32();

            var data_key0 = Sse2.Xor(data_vec0, key_vec0);
            var data_key1 = Sse2.Xor(data_vec1, key_vec1);
            var data_key2 = Sse2.Xor(data_vec2, key_vec2);
            var data_key3 = Sse2.Xor(data_vec3, key_vec3);

            var data_key_lo0 = Sse2.Shuffle(data_key0, MM_SHUFFLE_0_3_0_1);
            var data_key_lo1 = Sse2.Shuffle(data_key1, MM_SHUFFLE_0_3_0_1);
            var data_key_lo2 = Sse2.Shuffle(data_key2, MM_SHUFFLE_0_3_0_1);
            var data_key_lo3 = Sse2.Shuffle(data_key3, MM_SHUFFLE_0_3_0_1);

            var product0 = Sse2.Multiply(data_key0, data_key_lo0);
            var product1 = Sse2.Multiply(data_key1, data_key_lo1);
            var product2 = Sse2.Multiply(data_key2, data_key_lo2);
            var product3 = Sse2.Multiply(data_key3, data_key_lo3);

            var data_swap0 = Sse2.Shuffle(data_vec0, MM_SHUFFLE_1_0_3_2).AsUInt64();
            var data_swap1 = Sse2.Shuffle(data_vec1, MM_SHUFFLE_1_0_3_2).AsUInt64();
            var data_swap2 = Sse2.Shuffle(data_vec2, MM_SHUFFLE_1_0_3_2).AsUInt64();
            var data_swap3 = Sse2.Shuffle(data_vec3, MM_SHUFFLE_1_0_3_2).AsUInt64();

            var sum0 = Sse2.Add(acc_vec0, data_swap0);
            var sum1 = Sse2.Add(acc_vec1, data_swap1);
            var sum2 = Sse2.Add(acc_vec2, data_swap2);
            var sum3 = Sse2.Add(acc_vec3, data_swap3);

            var result0 = Sse2.Add(product0, sum0);
            var result1 = Sse2.Add(product1, sum1);
            var result2 = Sse2.Add(product2, sum2);
            var result3 = Sse2.Add(product3, sum3);

            Unsafe.Write(acc + 0, result0);
            Unsafe.Write(acc + 2, result1);
            Unsafe.Write(acc + 4, result2);
            Unsafe.Write(acc + 6, result3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void XXH3_accumulate_512_scalar(ulong* acc, byte* input, byte* secret)
        {
            for (int i = 0; i < XXH_ACC_NB; i++)
                XXH3_scalarRound(acc, input, secret, i);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void XXH3_scalarRound(ulong* acc, byte* input, byte* secret, int lane)
        {
            Debug.Assert(lane < XXH_ACC_NB);

            ulong* xacc = acc;
            byte* xinput = input;
            byte* xsecret = secret;

            ulong data_val = XXH_readLE64(xinput + lane * 8);
            ulong data_key = data_val ^ XXH_readLE64(xsecret + lane * 8);
            xacc[lane ^ 1] += data_val;
            xacc[lane] += XXH_mult32to64(data_key & 0xFFFFFFFF, data_key >> 32);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void XXH3_scrambleAcc(ulong* acc, byte* secret)
        {
            if (Avx2.IsSupported)
                XXH3_scrambleAcc_avx2(acc, secret);
            else if (Sse2.IsSupported)
                XXH3_scrambleAcc_sse2(acc, secret);
            else
                XXH3_scrambleAcc_scalar(acc, secret);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void XXH3_scrambleAcc_avx2(ulong* acc, byte* secret)
        {
            var acc_vec0 = Unsafe.Read<Vector256<ulong>>(acc + 0);
            var acc_vec1 = Unsafe.Read<Vector256<ulong>>(acc + 4);

            var shifted0 = Avx2.ShiftRightLogical(acc_vec0, 47);
            var shifted1 = Avx2.ShiftRightLogical(acc_vec1, 47);

            var data_vec0 = Avx2.Xor(acc_vec0, shifted0);
            var data_vec1 = Avx2.Xor(acc_vec1, shifted1);

            var key_vec0 = Unsafe.Read<Vector256<ulong>>((ulong*)secret + 0);
            var key_vec1 = Unsafe.Read<Vector256<ulong>>((ulong*)secret + 4);

            var data_key0 = Avx2.Xor(data_vec0, key_vec0).AsUInt32();
            var data_key1 = Avx2.Xor(data_vec1, key_vec1).AsUInt32();

            var data_key_hi0 = Avx2.Shuffle(data_key0, MM_SHUFFLE_0_3_0_1);
            var data_key_hi1 = Avx2.Shuffle(data_key1, MM_SHUFFLE_0_3_0_1);

            var prod_lo0 = Avx2.Multiply(data_key0, M256i_XXH_PRIME32_1);
            var prod_lo1 = Avx2.Multiply(data_key1, M256i_XXH_PRIME32_1);

            var prod_hi0 = Avx2.Multiply(data_key_hi0, M256i_XXH_PRIME32_1);
            var prod_hi1 = Avx2.Multiply(data_key_hi1, M256i_XXH_PRIME32_1);

            var result0 = Avx2.Add(prod_lo0, Avx2.ShiftLeftLogical(prod_hi0, 32));
            var result1 = Avx2.Add(prod_lo1, Avx2.ShiftLeftLogical(prod_hi1, 32));

            Unsafe.Write(acc + 0, result0);
            Unsafe.Write(acc + 4, result1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void XXH3_scrambleAcc_sse2(ulong* acc, byte* secret)
        {
            var acc_vec0 = Unsafe.Read<Vector128<ulong>>(acc + 0).AsUInt32();
            var acc_vec1 = Unsafe.Read<Vector128<ulong>>(acc + 2).AsUInt32();
            var acc_vec2 = Unsafe.Read<Vector128<ulong>>(acc + 4).AsUInt32();
            var acc_vec3 = Unsafe.Read<Vector128<ulong>>(acc + 6).AsUInt32();

            var shifted0 = Sse2.ShiftRightLogical(acc_vec0, 47);
            var shifted1 = Sse2.ShiftRightLogical(acc_vec1, 47);
            var shifted2 = Sse2.ShiftRightLogical(acc_vec2, 47);
            var shifted3 = Sse2.ShiftRightLogical(acc_vec3, 47);

            var data_vec0 = Sse2.Xor(acc_vec0, shifted0);
            var data_vec1 = Sse2.Xor(acc_vec1, shifted1);
            var data_vec2 = Sse2.Xor(acc_vec2, shifted2);
            var data_vec3 = Sse2.Xor(acc_vec3, shifted3);

            var key_vec0 = Unsafe.Read<Vector128<ulong>>((ulong*)secret + 0).AsUInt32();
            var key_vec1 = Unsafe.Read<Vector128<ulong>>((ulong*)secret + 2).AsUInt32();
            var key_vec2 = Unsafe.Read<Vector128<ulong>>((ulong*)secret + 4).AsUInt32();
            var key_vec3 = Unsafe.Read<Vector128<ulong>>((ulong*)secret + 6).AsUInt32();

            var data_key0= Sse2.Xor(data_vec0, key_vec0);
            var data_key1= Sse2.Xor(data_vec1, key_vec1);
            var data_key2= Sse2.Xor(data_vec2, key_vec2);
            var data_key3= Sse2.Xor(data_vec3, key_vec3);

            var data_key_hi0 = Sse2.Shuffle(data_key0.AsUInt32(), MM_SHUFFLE_0_3_0_1);
            var data_key_hi1 = Sse2.Shuffle(data_key1.AsUInt32(), MM_SHUFFLE_0_3_0_1);
            var data_key_hi2 = Sse2.Shuffle(data_key2.AsUInt32(), MM_SHUFFLE_0_3_0_1);
            var data_key_hi3 = Sse2.Shuffle(data_key3.AsUInt32(), MM_SHUFFLE_0_3_0_1);

            var prod_lo0 = Sse2.Multiply(data_key0, M128i_XXH_PRIME32_1);
            var prod_lo1 = Sse2.Multiply(data_key1, M128i_XXH_PRIME32_1);
            var prod_lo2 = Sse2.Multiply(data_key2, M128i_XXH_PRIME32_1);
            var prod_lo3 = Sse2.Multiply(data_key3, M128i_XXH_PRIME32_1);

            var prod_hi0 = Sse2.Multiply(data_key_hi0, M128i_XXH_PRIME32_1);
            var prod_hi1 = Sse2.Multiply(data_key_hi1, M128i_XXH_PRIME32_1);
            var prod_hi2 = Sse2.Multiply(data_key_hi2, M128i_XXH_PRIME32_1);
            var prod_hi3 = Sse2.Multiply(data_key_hi3, M128i_XXH_PRIME32_1);

            var result0 = Sse2.Add(prod_lo0, Sse2.ShiftLeftLogical(prod_hi0, 32));
            var result1 = Sse2.Add(prod_lo1, Sse2.ShiftLeftLogical(prod_hi1, 32));
            var result2 = Sse2.Add(prod_lo2, Sse2.ShiftLeftLogical(prod_hi2, 32));
            var result3 = Sse2.Add(prod_lo3, Sse2.ShiftLeftLogical(prod_hi3, 32));

            Unsafe.Write(acc + 0, result0);
            Unsafe.Write(acc + 2, result1);
            Unsafe.Write(acc + 4, result2);
            Unsafe.Write(acc + 6, result3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void XXH3_scrambleAcc_scalar(ulong* acc, byte* secret)
        {
            for (int i = 0; i < XXH_ACC_NB; i++)
                XXH3_scalarScrambleRound(acc, secret, i);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void XXH3_scalarScrambleRound(ulong* acc, byte* secret, int lane)
        {
            Debug.Assert(lane < XXH_ACC_NB);

            ulong* xacc = acc;
            byte* xsecret = secret;

            ulong key64 = XXH_readLE64(xsecret + lane * 8);
            ulong acc64 = xacc[lane];
            acc64 = XXH_xorshift64(acc64, 47);
            acc64 ^= key64;
            acc64 *= XXH_PRIME32_1;
            xacc[lane] = acc64;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void XXH3_initCustomSecret(byte* customSecret, ulong seed)
        {
            if (Avx2.IsSupported)
                XXH3_initCustomSecret_avx2(customSecret, seed);
            else if (Sse2.IsSupported)
                XXH3_initCustomSecret_sse2(customSecret, seed);
            else
                XXH3_initCustomSecret_scalar(customSecret, seed);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void XXH3_initCustomSecret_avx2(byte* customSecret, ulong seed64)
        {
            var seed = Vector256.Create(seed64, (0U - seed64), seed64, (0U - seed64));

            fixed (byte* secret = &XXH3_SECRET[0])
            {
                var src0 = Unsafe.Read<Vector256<ulong>>((ulong*)secret + 0);
                var src1 = Unsafe.Read<Vector256<ulong>>((ulong*)secret + 4);
                var src2 = Unsafe.Read<Vector256<ulong>>((ulong*)secret + 8);
                var src3 = Unsafe.Read<Vector256<ulong>>((ulong*)secret + 12);
                var src4 = Unsafe.Read<Vector256<ulong>>((ulong*)secret + 16);
                var src5 = Unsafe.Read<Vector256<ulong>>((ulong*)secret + 20);

                var dst0 = Avx2.Add(src0, seed);
                var dst1 = Avx2.Add(src1, seed);
                var dst2 = Avx2.Add(src2, seed);
                var dst3 = Avx2.Add(src3, seed);
                var dst4 = Avx2.Add(src4, seed);
                var dst5 = Avx2.Add(src5, seed);

                Unsafe.Write((ulong*)customSecret + 0, dst0);
                Unsafe.Write((ulong*)customSecret + 4, dst1);
                Unsafe.Write((ulong*)customSecret + 8, dst2);
                Unsafe.Write((ulong*)customSecret + 12, dst3);
                Unsafe.Write((ulong*)customSecret + 16, dst4);
                Unsafe.Write((ulong*)customSecret + 20, dst5);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void XXH3_initCustomSecret_sse2(byte* customSecret, ulong seed64)
        {
            var seed = Vector128.Create((long)seed64, (long)(0U - seed64));

            fixed (byte* secret = &XXH3_SECRET[0])
            {
                var src0 = Unsafe.Read<Vector128<long>>((long*)secret + 0);
                var src1 = Unsafe.Read<Vector128<long>>((long*)secret + 2);
                var src2 = Unsafe.Read<Vector128<long>>((long*)secret + 4);
                var src3 = Unsafe.Read<Vector128<long>>((long*)secret + 6);
                var src4 = Unsafe.Read<Vector128<long>>((long*)secret + 8);
                var src5 = Unsafe.Read<Vector128<long>>((long*)secret + 10);
                var src6 = Unsafe.Read<Vector128<long>>((long*)secret + 12);
                var src7 = Unsafe.Read<Vector128<long>>((long*)secret + 14);
                var src8 = Unsafe.Read<Vector128<long>>((long*)secret + 16);
                var src9 = Unsafe.Read<Vector128<long>>((long*)secret + 18);
                var src10 = Unsafe.Read<Vector128<long>>((long*)secret + 20);
                var src11 = Unsafe.Read<Vector128<long>>((long*)secret + 22);

                var dst0 = Sse2.Add(src0, seed);
                var dst1 = Sse2.Add(src1, seed);
                var dst2 = Sse2.Add(src2, seed);
                var dst3 = Sse2.Add(src3, seed);
                var dst4 = Sse2.Add(src4, seed);
                var dst5 = Sse2.Add(src5, seed);
                var dst6 = Sse2.Add(src6, seed);
                var dst7 = Sse2.Add(src7, seed);
                var dst8 = Sse2.Add(src8, seed);
                var dst9 = Sse2.Add(src9, seed);
                var dst10 = Sse2.Add(src10, seed);
                var dst11 = Sse2.Add(src11, seed);

                Unsafe.Write((long*)customSecret + 0, dst0);
                Unsafe.Write((long*)customSecret + 2, dst1);
                Unsafe.Write((long*)customSecret + 4, dst2);
                Unsafe.Write((long*)customSecret + 6, dst3);
                Unsafe.Write((long*)customSecret + 8, dst4);
                Unsafe.Write((long*)customSecret + 10, dst5);
                Unsafe.Write((long*)customSecret + 12, dst6);
                Unsafe.Write((long*)customSecret + 14, dst7);
                Unsafe.Write((long*)customSecret + 16, dst8);
                Unsafe.Write((long*)customSecret + 18, dst9);
                Unsafe.Write((long*)customSecret + 20, dst10);
                Unsafe.Write((long*)customSecret + 22, dst11);
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe void XXH3_initCustomSecret_scalar(byte* customSecret, ulong seed)
        {
            fixed (byte* kSecretPtr = &XXH3_SECRET[0])
            {
                int nbRounds = XXH_SECRET_DEFAULT_SIZE / 16;

                for (int i = 0; i < nbRounds; i++)
                {
                    ulong lo = XXH_readLE64(kSecretPtr + 16 * i) + seed;
                    ulong hi = XXH_readLE64(kSecretPtr + 16 * i + 8) - seed;
                    XXH_writeLE64((byte*) customSecret + 16 * i, lo);
                    XXH_writeLE64((byte*) customSecret + 16 * i + 8, hi);
                }
            }
        }
    }
}