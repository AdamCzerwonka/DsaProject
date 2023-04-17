#region copy
// Dsa implementation in C#
// Copyright (C) 2023 Adam Czerwonka, Marcel Badek
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Numerics;
using System.Security.Cryptography;

namespace DsaProject.Core;

public static class BigIntegerExtensions
{
    public static BigInteger NextBigInteger(int bitLength)
    {
        if (bitLength < 1) return BigInteger.Zero;

        int bytes = bitLength / 8;
        int bits = bitLength % 8;

        // Generates enough random bytes to cover our bits.
        Random rnd = new Random();
        byte[] bs = new byte[bytes + 1];
        rnd.NextBytes(bs);

        // Mask out the unnecessary bits.
        byte mask = (byte)(0xFF >> (8 - bits));
        bs[bs.Length - 1] &= mask;

        return new BigInteger(bs);
    }

    public static BigInteger PropablyPrime(int bitLength)
    {
        if (bitLength < 1)
        {
            return BigInteger.Zero;
        }

        var bs = RandomNumberGenerator.GetBytes(bitLength / 8);

        bs[^1] |= 1;

        return new BigInteger(bs);
    }

    public static BigInteger RandomBigInteger(BigInteger start, BigInteger end)
    {
        if (start == end) return start;

        BigInteger res = end;

        // Swap start and end if given in reverse order.
        if (start > end)
        {
            end = start;
            start = res;
            res = end - start;
        }
        else
            // The distance between start and end to generate a random BigIntger between 0 and (end-start) (non-inclusive).
            res -= start;

        byte[] bs = res.ToByteArray();

        // Count the number of bits necessary for res.
        int bits = 8;
        byte mask = 0x7F;
        while ((bs[bs.Length - 1] & mask) == bs[bs.Length - 1])
        {
            bits--;
            mask >>= 1;
        }

        bits += 8 * bs.Length;

        // Generate a random BigInteger that is the first power of 2 larger than res, 
        // then scale the range down to the size of res,
        // finally add start back on to shift back to the desired range and return.
        return ((NextBigInteger(bits + 1) * res) / BigInteger.Pow(2, bits + 1)) + start;
    }

    public static BigInteger x, y;

    // Function for extended Euclidean Algorithm
    static BigInteger gcdExtended(BigInteger a, BigInteger b)
    {
        // Base Case
        if (a == 0)
        {
            x = 0;
            y = 1;
            return b;
        }

        // To store results of recursive call
        var gcd = gcdExtended(b % a, a);
        var x1 = x;
        var y1 = y;

        // Update x and y using results of recursive
        // call
        x = y1 - (b / a) * x1;
        y = x1;

        return gcd;
    }

    // Function to find modulo inverse of a
    public static BigInteger modInverse(BigInteger A, BigInteger M)
    {
        var g = gcdExtended(A, M);
        if (g != 1)
        {
            throw new Exception("xd");
        }

        // M is added to handle negative x
        var res = (x % M + M) % M;
        return res;
    }

    public static bool IsProbablyPrime(this BigInteger source, int certainty)
    {
        if (source == 2 || source == 3)
            return true;
        if (source < 2 || source % 2 == 0)
            return false;

        var d = source - 1;
        var s = 0;

        while (d % 2 == 0)
        {
            d /= 2;
            s += 1;
        }

        // There is no built-in method for generating random BigInteger values.
        // Instead, random BigIntegers are constructed from randomly generated
        // byte arrays of the same length as the source.
        var rng = RandomNumberGenerator.Create();
        var bytes = new byte[source.ToByteArray().LongLength];

        for (var i = 0; i < certainty; i++)
        {
            BigInteger a;
            do
            {
                rng.GetBytes(bytes);
                a = new BigInteger(bytes);
            } while (a < 2 || a >= source - 2);

            var x = BigInteger.ModPow(a, d, source);
            if (x == 1 || x == source - 1)
                continue;

            for (var r = 1; r < s; r++)
            {
                x = BigInteger.ModPow(x, 2, source);
                if (x == 1)
                    return false;
                if (x == source - 1)
                    break;
            }

            if (x != source - 1)
                return false;
        }

        return true;
    }
}