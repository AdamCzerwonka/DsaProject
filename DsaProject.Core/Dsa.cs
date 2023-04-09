using System.Numerics;
using System.Security.Cryptography;
using Test;

namespace DsaProject.Core;

public static class Dsa
{
    public static (BigInteger, BigInteger) Sign(byte[] data, DsaKey key)
    {
        var hash = SHA256.HashData(data);
        var hashValue = new BigInteger(hash, isUnsigned: true);
        return Sign(hashValue, key);
    }

    public static (BigInteger, BigInteger) Sign(Stream data, DsaKey key)
    {
        var hash = SHA256.HashData(data);
        var hashValue = new BigInteger(hash, isUnsigned: true);
        return Sign(hashValue, key);
    }

    private static (BigInteger, BigInteger) Sign(BigInteger hash, DsaKey key)
    {
        BigInteger r, s;
        do
        {
            var k = BigIntegerExtensions.RandomBigInteger(BigInteger.One, key.Q - BigInteger.One);
            r = BigInteger.ModPow(key.G, k, key.P) % key.Q;
            var kInv = BigIntegerExtensions.modInverse(k, key.Q);
            s = (kInv * (hash + key.X * r)) % key.Q;
        } while (r == BigInteger.Zero || s == BigInteger.Zero);

        return (r, s);
    }

    public static bool Verify(byte[] data, DsaKey key, BigInteger r, BigInteger s)
    {
        var hash = SHA256.HashData(data);
        var hashValue = new BigInteger(hash, isUnsigned: true);
        return Verify(hashValue, key, r, s);
    }

    public static bool Verify(Stream data, DsaKey key, BigInteger r, BigInteger s)
    {
        var hash = SHA256.HashData(data);
        var hashValue = new BigInteger(hash, isUnsigned: true);
        return Verify(hashValue, key, r, s);
    }

    private static bool Verify(BigInteger hash, DsaKey key, BigInteger r, BigInteger s)
    {
        if (r < 0 || r > key.Q)
        {
            return false;
        }

        if (s < 0 || s > key.Q)
        {
            return false;
        }

        var sInv = BigIntegerExtensions.modInverse(s, key.Q);
        var u1 = (hash * sInv) % key.Q;
        var u2 = (r * sInv) % key.Q;
        var v = ((BigInteger.ModPow(key.G, u1, key.P) * BigInteger.ModPow(key.Y, u2, key.P)) % key.P) % key.Q;
        return v == r;
    }
}