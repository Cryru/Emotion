using System.Runtime.CompilerServices;

namespace Emotion.ThirdParty.IcariaNoise;

// =~= EMOTION ENGINE =~=
// Modifications applied:
// 1. Public -> internal
// 2. Added private where non specified
// 3. Removed periodic functions
// 4. Spacings between functons
// =~=~=~=~=~=~=~=~=~=~=~

internal static partial class IcariaNoise
{
    /// <summary> -1 to 1 gradient noise function. Analagous to Perlin noise. </summary>
    internal static float GradientNoise(float x, float y, int seed = 0)
    {
        // NOTE: if you are looking to understand how this function works, first make sure
        // you understand the concepts behind Perlin Noise. these comments only detail
        // the specifics of this implementation.

        // break up sample coords into a float and int component, 
        // (ix, iy) represent the lower-left corner of the unit square the sample is in,
        // (fx, fy) represent the 0.0 to 1.0 position within that square
        // ix = floor(x) and fx = x - ix
        // iy = floor(y) and iy = y - iy
        int ix = x > 0 ? (int)x : (int)x - 1;
        int iy = y > 0 ? (int)y : (int)y - 1;
        float fx = x - ix;
        float fy = y - iy;

        // Hashes for non-periodic noise are the product of two linear fields p1 and p2, where
        // p1 = x * XPrime1 + y * YPrime1 (XPrime1 and YPrime1 are constant 32-bit primes)
        // p2 = x * XPrime2 + y * YPrime2 (XPrime2 and YPrime2 are constant 32-bit primes)
        // adding a constant to the value of these fields at the lower-left corner of the square can get
        // you the value at the remaining 3 corners, which reduces the multiplies per hash by a factor of 3.
        // this behaves poorly at x = 0 or y = 0 so we add a very large constant offset 
        // to the x and y coordinates before calculating the hash. 
        ix += Const.Offset;
        iy += Const.Offset;
        ix += Const.SeedPrime * seed; // add seed before hashing to propigate its effect
        int p1 = ix * Const.XPrime1 + iy * Const.YPrime1;
        int p2 = ix * Const.XPrime2 + iy * Const.YPrime2;
        int llHash = p1 * p2;
        int lrHash = (p1 + Const.XPrime1) * (p2 + Const.XPrime2);
        int ulHash = (p1 + Const.YPrime1) * (p2 + Const.YPrime2);
        int urHash = (p1 + Const.XPlusYPrime1) * (p2 + Const.XPlusYPrime2);
        return InterpolateGradients2D(llHash, lrHash, ulHash, urHash, fx, fy);
    }

    /// <summary>Two seperatly seeded fields of -1 to 1 gradient noise. Analagous to Perlin noise.</summary>
    internal static (float x, float y) GradientNoiseVec2(float x, float y, int seed = 0)
    {
        int ix = x > 0 ? (int)x : (int)x - 1;
        int iy = y > 0 ? (int)y : (int)y - 1;
        float fx = x - ix;
        float fy = y - iy;

        ix += Const.Offset;
        iy += Const.Offset;
        ix += Const.SeedPrime * seed; // add seed before hashing to propigate its effect
        int p1 = ix * Const.XPrime1 + iy * Const.YPrime1;
        int p2 = ix * Const.XPrime2 + iy * Const.YPrime2;
        int llHash = p1 * p2;
        int lrHash = (p1 + Const.XPrime1) * (p2 + Const.XPrime2);
        int ulHash = (p1 + Const.YPrime1) * (p2 + Const.YPrime2);
        int urHash = (p1 + Const.XPlusYPrime1) * (p2 + Const.XPlusYPrime2);

        x = InterpolateGradients2D(llHash, lrHash, ulHash, urHash, fx, fy);
        // multiplying by a 32-bit value is all you need to reseed already randomized bits. 
        y = InterpolateGradients2D(
            llHash * Const.XPrime1, lrHash * Const.XPrime1,
            ulHash * Const.XPrime1, urHash * Const.XPrime1, fx, fy);
        return (x, y);
    }

    /// <summary>Evaluates and interpolates the gradients at each corner.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe float InterpolateGradients2D(int llHash, int lrHash, int ulHash, int urHash, float fx, float fy)
    {
        // here we calculate a gradient at each corner, where the value is the dot-product 
        // of a vector derived from the hash and the vector from the coner to the
        // sample point. these vectors are blended using bilinear interpolation
        // and the result is the return value for the noise function.
        // to convert a hash value to a vector, we reinterpret the random bits
        // as a floating-point number, but use bitmasks to set the exponent
        // of the value to 0.5, which makes the range of output results is
        // -1 to -0.5 and 0.5 to 1. With this value in both channels our vector 
        // can face along any diagonal axis and has a magnitude close to 1,
        // which is a good enough distribution of vectors for gradient noise.
        // to avoid having to calculate the mask twice for the x and y coordinates,
        // the mask has a second copy of the exponent bits in unsignifigant bits
        // of the mantissa, so bitshifting the masked hash to align the second exponent
        // gives a second random float in the same range as the first.
        // this could be broken up into functions but doing so massively hurts
        // preformance without optimizations enabled.
        int xHash, yHash;
        xHash = (llHash & Const.GradAndMask) | Const.GradOrMask;
        yHash = xHash << Const.GradShift1;
        float llGrad = fx * *(float*)&xHash + fy * *(float*)&yHash; // dot-product
        xHash = (lrHash & Const.GradAndMask) | Const.GradOrMask;
        yHash = xHash << Const.GradShift1;
        float lrGrad = (fx - 1) * *(float*)&xHash + fy * *(float*)&yHash;
        xHash = (ulHash & Const.GradAndMask) | Const.GradOrMask;
        yHash = xHash << Const.GradShift1;
        float ulGrad = fx * *(float*)&xHash + (fy - 1) * *(float*)&yHash; // dot-product
        xHash = (urHash & Const.GradAndMask) | Const.GradOrMask;
        yHash = xHash << Const.GradShift1;
        float urGrad = (fx - 1) * *(float*)&xHash + (fy - 1) * *(float*)&yHash;
        // adjust blending values with the smoothstep function s(x) = x * x * (3 - 2 * x)
        // which gives a result close to x but with a slope of zero at x = 0 and x = 1.
        // this makes the blending transitions between cells less harsh.
        float sx = fx * fx * (3 - 2 * fx);
        float sy = fy * fy * (3 - 2 * fy);
        float lowerBlend = llGrad + (lrGrad - llGrad) * sx;
        float upperBlend = ulGrad + (urGrad - ulGrad) * sx;
        return lowerBlend + (upperBlend - lowerBlend) * sy;
    }


    /// <summary>Calculates a 2D gradient based on a hash value and coordinates relative to the gradient's origin.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe float EvalGradient(int hash, float fx, float fy)
    {
        // to convert a hash value to a vector, we reinterpret the random bits
        // as a floating-point number, but use bitmasks to set the exponent
        // of the value to 0.5, which makes the range of output results is
        // -1 to -0.5 and 0.5 to 1. With this value in both channels our vector 
        // can face along any diagonal axis and has a magnitude close to 1,
        // which is a good enough distribution of vectors for gradient noise.
        // to avoid having to calculate the mask twice for the x and y coordinates,
        // the mask has a second copy of the exponent bits in unsignifigant bits
        // of the mantissa, so bitshifting the masked hash to align the second exponent
        // gives a second random float in the same range as the first.
        int xHash = (hash & Const.GradAndMask) | Const.GradOrMask;
        int yHash = xHash << Const.GradShift1;
        return fx * *(float*)&xHash + fy * *(float*)&yHash; // dot-product
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Hash(int x, int y)
    {
        // bitshift on y to make sure Hash(x + 1, y) and Hash(x, y + 1)
        // are radically different, shifts below 6 produce visable artifacts.
        int hash = x ^ (y << 6);
        // bits passed into this hash function are in the upper part of the lower bits of an int, 
        // we bit shift them slightly lower here to maximize the impact of the following multiply. 
        // the lowest bit will effect all bits when multiplied, but higher bits don't effect anything
        // below them, so you want your signifigant bits as low as possible. the bitshift isn't larger
        // because then it would in some cases bitshift some of your bits off the bottom of the int,
        // which is a disaster for hash quality.
        hash += hash >> 5;
        // multiply propigates lower bits to every single bit
        hash *= Const.XPrime1;
        // xor and add operators are nonlinear relative to eachother, so interleaving like this
        // produces the nonlinearities the hash function needs to avoid visual artifacts. 
        // we are bitshifting down to make these nonlinearities occur in low bits so after the final multiply
        // they effect the largest fraction of the output hash.
        hash ^= hash >> 4;
        hash += hash >> 2;
        hash ^= hash >> 16;
        // multiply propigates lower bits to every single bit (again)
        hash *= Const.XPrime2;
        return hash;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float Lerp(float a, float b, float t) => a + (b - a) * t;

    internal static class Const
    {
        internal const int FractalOctaves = 8;

        internal const int
            Offset = 0228125273,
            SeedPrime = 525124619,
            SeedMask = 0x0FFFFFFF,
            XPrime1 = 0863909317,
            YPrime1 = 1987438051,
            ZPrime1 = 1774326877,
            XPlusYPrime1 = unchecked(XPrime1 + YPrime1),
            XPlusZPrime1 = unchecked(XPrime1 + ZPrime1),
            YPlusZPrime1 = unchecked(YPrime1 + ZPrime1),
            XPlusYPlusZPrime1 = unchecked(XPrime1 + YPrime1 + ZPrime1),
            XMinusYPrime1 = unchecked(XPrime1 - YPrime1),
            YMinusXPrime1 = unchecked(XPrime1 - YPrime1),
            XPrime2 = 1299341299,
            YPrime2 = 0580423463,
            ZPrime2 = 0869819479,
            XPlusYPrime2 = unchecked(XPrime2 + YPrime2),
            XPlusZPrime2 = unchecked(XPrime2 + ZPrime2),
            YPlusZPrime2 = unchecked(YPrime2 + ZPrime2),
            XPlusYPlusZPrime2 = unchecked(XPrime2 + YPrime2 + ZPrime2),
            XMinusYPrime2 = unchecked(XPrime2 - YPrime2),
            YMinusXPrime2 = unchecked(XPrime2 - YPrime2),
            GradAndMask = -0x7F9FE7F9, //-0x7F87F801
            GradOrMask = 0x3F0FC3F0, //0x3F03F000
            GradShift1 = 10,
            GradShift2 = 20,
            PeriodShift = 18,
            WorleyAndMask = 0x007803FF,
            WorleyOrMask = 0x3F81FC00,
            PortionAndMask = 0x007FFFFF,
            PortionOrMask = 0x3F800000;
    }
}
