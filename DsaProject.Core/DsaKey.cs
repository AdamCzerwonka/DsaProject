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

public class DsaKey
{
    public DsaKey()
    {
        GenerateKey();
    }

    public BigInteger Q { get; set; }
    public BigInteger P { get; set; }
    public BigInteger G { get; set; }
    public BigInteger X { get; set; }
    public BigInteger Y { get; set; }

    public void GenerateKey()
    {
        // Generate L (key lenght) value from 512 to 1024 and it must be divisable by 64
        var keyLenght = RandomNumberGenerator.GetInt32(512, 1024);
        while (keyLenght % 64 != 0)
        {
            keyLenght++;
        }

        Console.WriteLine(keyLenght);

        // Generate q
        do
        {
            Q = BigIntegerExtensions.PropablyPrime(160);
        } while (!Q.IsProbablyPrime(100));

        BigInteger pom1, pom2;
        do
        {
            pom1 = BigIntegerExtensions.PropablyPrime(keyLenght);
            pom2 = pom1 - BigInteger.One;
            pom1 -= pom2 % Q;
        } while (!pom1.IsProbablyPrime(100));

        P = pom1;
        do
        {
            var h = BigIntegerExtensions.RandomBigInteger(2, P - 2);
            
            G = BigInteger.ModPow(h, (P - 1) / Q, P);
        } while (G == BigInteger.One);

        X = BigIntegerExtensions.RandomBigInteger(1, Q - 1);
        Y = BigInteger.ModPow(G, X, P);
    }
}