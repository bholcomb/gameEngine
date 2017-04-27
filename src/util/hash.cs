using System;

namespace Util
{
   //this class is based on Bob Jenkins lookup3 hashing function from: 
   //http://www.burtleburtle.net/bob/c/lookup3.c
   //this guy is a genius.
   public static class Hash
   {
      //this is Ella's birthday
      public static UInt32 theInitValue=102809;
      static UInt32 rot(UInt32 x, int k)
      {
         return ((x << k) | (x >> (32 - k)));
      }

      public static UInt32 hash(String str)
      {
         byte[] bytes = System.Text.Encoding.ASCII.GetBytes(str);
         return hash(bytes, theInitValue);
      }

      public static UInt32 hash(String str, UInt32 initval)
      {
         byte[] bytes = System.Text.Encoding.ASCII.GetBytes(str);
         return hash(bytes, initval);
      }

      public static UInt32 hash(byte[] bytes)
      {
         return hash(bytes, theInitValue);
      }

      static UInt32 hash(byte[] key, UInt32 initval)
      {
         int length = key.Length;
         UInt32 a, b, c;
         a = b = c = (UInt32)(0xdeadbeef + length + initval);
         int offset = 0;
         for (; length > 12; offset += 12, length -= 12)
         {
            a = a + (UInt32)key[offset + 0];
            a = a + (UInt32)(key[offset + 1] << 8);
            a = a + (UInt32)(key[offset + 2] << 16);
            a = a + (UInt32)(key[offset + 3] << 24);
            b = b + (UInt32)key[offset + 4];
            b = b + (UInt32)(key[offset + 5] << 8);
            b = b + (UInt32)(key[offset + 6] << 16);
            b = b + (UInt32)(key[offset + 7] << 24);
            c = c + (UInt32)key[offset + 8];
            c = c + (UInt32)(key[offset + 9] << 8);
            c = c + (UInt32)(key[offset + 10] << 16);
            c = c + (UInt32)(key[offset + 11] << 24);

            /*
             * mix -- mix 3 32-bit values reversibly. This is reversible, so any
             * information in (a,b,c) before mix() is still in (a,b,c) after
             * mix().
             * 
             * If four pairs of (a,b,c) inputs are run through mix(), or through
             * mix() in reverse, there are at least 32 bits of the output that
             * are sometimes the same for one pair and different for another
             * pair.
             * 
             * This was tested for: - pairs that differed by one bit, by two
             * bits, in any combination of top bits of (a,b,c), or in any
             * combination of bottom bits of (a,b,c). - "differ" is defined as
             * +, -, ^, or ~^. For + and -, I transformed the output delta to a
             * Gray code (a^(a>>1)) so a string of 1's (as is commonly produced
             * by subtraction) look like a single 1-bit difference. - the base
             * values were pseudorandom, all zero but one bit set, or all zero
             * plus a counter that starts at zero.
             * 
             * Some k values for my "a-=c; a^=rot(c,k); c+=b;" arrangement that
             * satisfy this are 4 6 8 16 19 4 9 15 3 18 27 15 14 9 3 7 17 3
             * Well, "9 15 3 18 27 15" didn't quite get 32 bits diffing for
             * "differ" defined as + with a one-bit base and a two-bit delta. I
             * used http://burtleburtle.net/bob/hash/avalanche.html to choose
             * the operations, constants, and arrangements of the variables.
             * 
             * This does not achieve avalanche. There are input bits of (a,b,c)
             * that fail to affect some output bits of (a,b,c), especially of a.
             * The most thoroughly mixed value is c, but it doesn't really even
             * achieve avalanche in c.
             * 
             * This allows some parallelism. Read-after-writes are good at
             * doubling the number of bits affected, so the goal of mixing pulls
             * in the opposite direction as the goal of parallelism. I did what
             * I could. Rotates seem to cost as much as shifts on every machine
             * I could lay my hands on, and rotates are much kinder to the top
             * and bottom bits, so I used rotates.
             * 
             * #define mix(a,b,c) \ { \ a -= c; a ^= rot(c, 4); c += b; \ b -=
             * a; b ^= rot(a, 6); a += c; \ c -= b; c ^= rot(b, 8); b += a; \ a
             * -= c; a ^= rot(c,16); c += b; \ b -= a; b ^= rot(a,19); a += c; \
             * c -= b; c ^= rot(b, 4); b += a; \ }
             * 
             * mix(a,b,c);
             */
            a -= c; a ^= rot(c, 4);  c += b;
            b -= a; b ^= rot(a, 6);  a += c;
            c -= b; c ^= rot(b, 8);  b += a;
            a -= c; a ^= rot(c, 16); c += b;
            b -= a; b ^= rot(a, 19); a += c;
            c -= b; c ^= rot(b, 4);  b += a;
         }

         // -------------------------------- last block: affect all 32 bits of
         // (c)
         switch (length)
         { // all the case statements fall through using the goto case trick
            case 12: c = c + (UInt32)(key[offset + 11] << 24); goto case 11;
            case 11: c = c + (UInt32)(key[offset + 10] << 16); goto case 10;
            case 10: c = c + (UInt32)(key[offset + 9] << 8); goto case 9;
            case 9: c = c + (UInt32)key[offset + 8]; goto case 8;
            case 8: b = b + (UInt32)(key[offset + 7] << 24); goto case 7;
            case 7: b = b + (UInt32)(key[offset + 6] << 16); goto case 6;
            case 6: b = b + (UInt32)(key[offset + 5] << 8); goto case 5;
            case 5: b = b + (UInt32)key[offset + 4]; goto case 4;
            case 4: a = a + (UInt32)(key[offset + 3] << 24); goto case 3;
            case 3: a = a + (UInt32)(key[offset + 2] << 16); goto case 2;
            case 2: a = a + (UInt32)(key[offset + 1] << 8); goto case 1;
            case 1: a = a + (UInt32)key[offset + 0]; break;
            case 0: return c;
         }
         /*
          * final -- final mixing of 3 32-bit values (a,b,c) into c
          * 
          * Pairs of (a,b,c) values differing in only a few bits will usually
          * produce values of c that look totally different. This was tested for
          * - pairs that differed by one bit, by two bits, in any combination of
          * top bits of (a,b,c), or in any combination of bottom bits of (a,b,c).
          * 
          * - "differ" is defined as +, -, ^, or ~^. For + and -, I transformed
          * the output delta to a Gray code (a^(a>>1)) so a string of 1's (as is
          * commonly produced by subtraction) look like a single 1-bit
          * difference.
          * 
          * - the base values were pseudorandom, all zero but one bit set, or all
          * zero plus a counter that starts at zero.
          * 
          * These constants passed: 14 11 25 16 4 14 24 12 14 25 16 4 14 24 and
          * these came close: 4 8 15 26 3 22 24 10 8 15 26 3 22 24 11 8 15 26 3
          * 22 24
          * 
          * #define final(a,b,c) \ { c ^= b; c -= rot(b,14); \ a ^= c; a -=
          * rot(c,11); \ b ^= a; b -= rot(a,25); \ c ^= b; c -= rot(b,16); \ a ^=
          * c; a -= rot(c,4); \ b ^= a; b -= rot(a,14); \ c ^= b; c -= rot(b,24);
          * \ }
          */
         c ^= b; c -= rot(b, 14); 
         a ^= c; a -= rot(c, 11); 
         b ^= a; b -= rot(a, 25);
         c ^= b; c -= rot(b, 16);
         a ^= c; a -= rot(c, 4); 
         b ^= a; b -= rot(a, 14);
         c ^= b; c -= rot(b, 24);

         return c;
      }
   }
}