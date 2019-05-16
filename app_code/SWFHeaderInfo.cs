/*
--------------------------------------
Darron Schall (darron@darronschall.com)
July 7th, 2003

File: SWFHeaderInfo.cs

Note: Some ideas for this code were used from the flasm
source code. See http://flasm.sourceforge.net
for more information on flasm.

Requires: This file requires "SharpZipLib" for compilation, found at
http://www.icsharpcode.net/OpenSource/SharpZipLib/ for
decompressing compressed .swf files with the ZLib algorithm.

Description:
See the summary for the SWFHeaderInfo class

Revision History:
Rev Date    Who   Description
1.0 07/05/03  darron  Initial Draft
1.1 07/10/03  darron  Explicitly close the swf file on disk when decompressing
1.2 08/05/03  darron  Better handling for invalid .swf files, compressed some
            code for reading the first 3 bytes, checking for successful
            decompression before reading the swf, made sure the .swf
            file is closed when an error occurs.
1.3 10/10/03  darron  * Thanks to jose for pointing out that I was calculating the
            framerate wrong - 
            http://www.darronschall.com/weblog/archives/000030.cfm
            * Updated xMin, xMax, yMin, yMax, and FrameRate to be of type float
            so that the values are parsed correctly now.
            * Fixed some spelling errors in the comments.
--------------------------------------
License For Use
--------------------------------------
Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
list of conditions and the following disclaimer.

2. Redistributions in binary form must reproduce the above copyright notice,
this list of conditions and the following disclaimer in the documentation
and/or other materials provided with the distribution.

3. The name of the author may not be used to endorse or promote products derived
from this software without specific prior written permission.

THIS SOFTWARE IS PROVIDED BY THE AUTHOR "AS IS" AND ANY EXPRESS OR IMPLIED
WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO
EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT
OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING
IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
OF SUCH DAMAGE.
*/


using System;
using System.IO;

namespace NFN.Flash
{
  /// <summary>
  /// SWFHeaderInfo is a class that exposes as properties the contents of 
  /// a .swf file header.  The path to the .swf file is passed into
  /// the constructor.  The Status property is useful for
  /// determining if the input was valid or not.  It is either "OK" if
  /// the swf header was successfully read, or an error message otherwise.
  /// 
  /// SWFHeaderInfo works for both compressed and uncompressed .swf  files.
  /// </summary>
  public class SWFHeaderInfo
  {
    private byte bitPos; // bitPos is our "position" in out bitBuffer "array", valid values are 0-8 (8 means get a new buffer)
    private byte bitBuffer;  // this is a byte that we'll be treating like an array
    uint nBits; // the number of bit used to store the data for the RECT type storing
          // the movies xMin, xMax, yMin, and yMax values
    private ICSharpCode.SharpZipLib.Zip.Compression.Inflater zipInflator; // used to decompressed swf
    Stream swf; // either a FileStream or MemoryStream
    
    // private internal data values
    private byte mVersion;
    private uint mLength;
    private float mxMin;  // 10/10/03 - changed to floats
    private float mxMax;
    private float myMin;
    private float myMax;
    private float mFrameRate;
    private ushort mFrameCount;
    private bool mCompressed;
    private string mStatus;

    /// <summary>public read-only (because no "set" defined) properties</summary>
    public byte Version 
    {
      get 
      {
        return mVersion;
      }
    }
    
    /// <summary></summary>
    public uint Length 
    {
      get 
      {
        return mLength;
      }
    }

    /// <summary></summary>
    public float xMin 
    {
      get 
      {
        return mxMin;
      }
    }
    /// <summary></summary>
    public float xMax 
    {
      get 
      {
        return mxMax;
      }
    }

    /// <summary></summary>
    public float yMin 
    {
      get 
      {
        return myMin;
      }
    }

    /// <summary></summary>
    public float yMax
    {
      get 
      {
        return myMax;
      }
    }

    /// <summary></summary>
    public float FrameRate 
    {
      get 
      {
        return mFrameRate;
      }
    }

    /// <summary></summary>
    public ushort FrameCount
    {
      get 
      {
        return mFrameCount;
      }
    }

    /// <summary></summary>
    public bool Compressed 
    {
      get 
      {
        return mCompressed;
      }
    }

    /// <summary></summary>
    public string Status
    {
      get 
      {
        return mStatus;
      }
    }


    /// <summary></summary>
    public SWFHeaderInfo(string path)
    {
      char first; // C if compressed, F otherwise
      
      // initialize
      mVersion = 0;
      mLength = 0;
      mxMin = 0;
      mxMax = 0;
      myMin = 0;
      mxMax = 0;
      mFrameRate = 0;
      mFrameCount = 0;
      mCompressed = false;
      mStatus = "No input specified";

      // attempt to open the swf file
      try 
      {
        swf = new FileStream(path, FileMode.Open);
      } 
      catch
      {
        mStatus = "Error opening swf file";
        // We don't need to close the file here because there
        // was a problem opening it.
        //swf.Close();
        return;
      }

      bitPos = 8; // set out bitPos to be "out of bounds" so the first call to getBits reads in
            // a new byte from the file (the first byte)
            
      // try to read the first byte of the file
      try 
      {
        first = (char)getBits(swf,8);
      }
      catch
      {
        mStatus = "Error reading first byte of file";
        // close the swf file on error - 08/04/03
        swf.Close();
        return;
      }
      
      if (first == 'C') 
      {
        // compressed swf file
        mCompressed = true;
        swf = decompressSwf(swf); 

        if (mStatus == "Error decompressing swf file") 
        {
          // trouble decompressing.. bail out! - 08/04/03
          swf.Close();
          return;
        }
        
        // reset our "array" index so the first call to getBits reads a new byte
        bitPos = 8;
        // attempt to get the first byte of the file
        try 
        {
          first = (char)getBits(swf,8);
        }
        catch
        {
          mStatus = "Error reading first byte of file";
          // close the swf file on error - 08/04/03
          swf.Close();
          return;
        }
      }
      
      // wrapped everything in a try/catch block "just in case" for
      // a little better error handling
      try 
      {
        // make sure the first 3 bytes are "FWS"
        if (first != 'F' || (char)getBits(swf,8) != 'W' || (char)getBits(swf,8) != 'S') 
        {
          // not a swf file!
          mStatus = "File specified does not seem to be a valid .swf file";
          // close the swf file on error - 08/04/03
          swf.Close();
          return;
        }

        // start collecting informatin
        mVersion = (byte)getBits(swf,8); 
        mLength = getDoubleWord(swf); 

        
        // 10/10/03 - changed to floats
        nBits = getBits(swf,5);
        mxMin = getBits(swf, nBits); // always 0
        mxMax = (float)getBits(swf, nBits)/20;  
        myMin = getBits(swf, nBits); // always 0
        myMax = (float)getBits(swf, nBits)/20;

        mFrameRate =(float)swf.ReadByte() / 256;
        mFrameRate += swf.ReadByte(); 

        mFrameCount = (ushort)(getWord(swf)); 
        
        mStatus = "OK";

      }
      catch
      {
        mStatus = "Error reading .swf header information";
        return;
      }
            
      swf.Close();

    }

    // corrections for flash storing the byte values in little-endian format
    // ported from the flasm source code
    private uint getWord(Stream f) 
    {
      return (uint) (f.ReadByte() | f.ReadByte() << 8);
    }

    // corrections for flash storing the byte values in little-endian format
    // ported from the flasm source code
    private uint getDoubleWord(Stream f) 
    {
      return getWord(f) | (getWord(f) << 16);
    }
    
    // idea for this function borrowed from the flasm source code, but
    // the function code is my creation
    private uint getBits (Stream f, uint n)
    {
      uint returnbits = 0;
      while (true)
      {
        // do we have more bits to read?
        if (n >= 1) 
        {
          // yes, more bits to read....  do we have unread bits in our buffer?
          if (bitPos == 8) {
            // no more bits to read in our buffer, lets get a new
            // buffer from f and reset the bitPos
            bitPos = 0;
            bitBuffer = (byte)f.ReadByte(); // read 8 bits at a time
          }

          // if we're here, we have more bits to read and 
          // we have unread bits in the buffer

          returnbits <<= 1; // shift the returnbit value left 1 place

          byte bitMask = (byte)(0x80 >> bitPos);
          
          // determine if the next bit we add to return bits should
          // be a 1 or a 0, based on the value of applying
          // the bitMask to the bitBuffer.  A quick example:
          // bitBuffer = 01011010
          // bitmask =   00001000
          // ~~~~~~~~~~~~~~~~~~~~  anding the bits together yeilds:
          //             00001000
          // and because the result is equal to the bitmask, we add
          // a 1 to the returnbits value.
          
          returnbits |= (bitBuffer & bitMask) == bitMask ? (uint)1 : (uint)0; 

          n -= 1; // one less bit to read
          bitPos += 1; // advance our "array" index
        }
        else 
        {
          //no more bits to read, return what we read from f
          return returnbits;
        }
      }
    }

    private Stream decompressSwf(Stream swf) 
    {
      // seek to after the 4th byte and read in 4 bytes to determine the 
      // size that the buffer needs to be to uncompressed the swf file
      swf.Seek(4, SeekOrigin.Begin);
      uint bufferSize = getDoubleWord(swf);

      // the uncompressedSwf byte array will be used as a MemoryStream
      byte[] uncompressedSwf = new byte[bufferSize];

      // only after the 8th byte is the ZLib compression applied, so just
      // copy the first 8 bytes from the compressed swf to the uncompressed swf
      swf.Seek(0, SeekOrigin.Begin);
      swf.Read(uncompressedSwf, 0, 8);

      // set the first byte to be 'F' instead of 'C'
      uncompressedSwf[0] = (byte)0x46;

      zipInflator = new ICSharpCode.SharpZipLib.Zip.Compression.Inflater();
      // because the zipInflator takes a byte array, not a FileStream,
      // we need to declare a temporary byte array to use as the 
      // input for inflation
      byte[] tmpSwf = new byte[bufferSize - 8];
        
      // read the rest of the swf into our temporary byte array
      swf.Read(tmpSwf,0,(int)(bufferSize-8));
      
      // close the swf on disk because we have enough information in memory now
      swf.Close();

      zipInflator.SetInput(tmpSwf);

      if (zipInflator.Inflate(uncompressedSwf,8,(int)(bufferSize-8)) == 0) 
      {
        mStatus = "Error decompressing swf file";
      }
        
      // change our swf from a FileStream to a MemoryStream now, using the uncompressed swf data
      return new MemoryStream(uncompressedSwf);
    }
    
      
    /*
    [STAThread]
    public static void Main() 
    {     
            SWFHeaderInfo s = new SWFHeaderInfo("C:\\Development\\SWFHeaderInfo\\test.swf");
      Console.WriteLine(s.Status);
      if (s.Status == "OK") 
      {
        Console.WriteLine("Version: " + s.Version);
        Console.WriteLine("Length: " + s.Length);
        Console.WriteLine("xMin: " + s.xMin);
        Console.WriteLine("xMax: " + s.xMax);
        Console.WriteLine("yMin: " + s.xMin);
        Console.WriteLine("yMax: " + s.yMax);
        Console.WriteLine("FrameRate: " + s.FrameRate);
        Console.WriteLine("FrameCount: " + s.FrameCount);
        Console.WriteLine("Compressed: " + s.Compressed);
      }
      
      // keep the input on the screen
      Console.Read();
      
      
    }
    */
    
  }
}
