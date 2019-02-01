using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ClassLibrary1
{
    public class B64Encoder
    {
        private List<char> _encodingList;
        private string _srcFileName;
        private string _dstFileName;
        private int _numOfBytesWrittenInLine;
        private byte[] _writeBuff;
        private int _wBuffIdx;

        private int _bytesRead;
        private List<byte> _fourDecodedBytes;
        private char _oneChar;

        FileStream _fsr;
        FileStream _fsw;
        BinaryReader _br;


        private const int MAX_NUM_OF_CHARS_IN_LINE = 76;
        private const string JPG_TYPE = "/9j/";
        private const string PNG_TYPE = "iVBORw0KGgoAAAANSUhEUgAA";
        private const string BMP_TYPE = "Qk";
        private const string DOCX_TYPE = "UEsDBBQABgAIAAAAIQ";
        private const string ZIP_TYPE = "UEsDBBQAAAAIA";

        public string SrcFileName { get => _srcFileName; set => _srcFileName = value; }
        public string DstFileName { get => _dstFileName; set => _dstFileName = value; }

        public B64Encoder(string srcFileName, string dstFileName = "")
        {
            _encodingList = new List<char>(new char[] { 'A','B','C','D','E','F','G','H','I','J','K','L','M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
                                                        'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z',
                                                        '0','1','2','3','4','5','6','7','8','9','+','/','='});

            SrcFileName = srcFileName;
            DstFileName = dstFileName;
            _numOfBytesWrittenInLine = 0;
            _writeBuff = new byte[5];
            _wBuffIdx = 0;

            _bytesRead = 0;
            _fourDecodedBytes = new List<byte>();
            _oneChar = ' ';

            _fsr = null;
            _fsw = null;
            _br = null;
        }


        private void addByteToWBuffer(byte oneByte)
        {
            if (_numOfBytesWrittenInLine == MAX_NUM_OF_CHARS_IN_LINE)
            {
                _writeBuff[_wBuffIdx++] = (byte)'\n';
                _numOfBytesWrittenInLine = 0;
            }
            _writeBuff[_wBuffIdx++] = oneByte;
            _numOfBytesWrittenInLine++;
        }


        public void encode()
        {
            _fsr = new FileStream(SrcFileName, FileMode.Open, FileAccess.Read);
            _br = new BinaryReader(_fsr);
            _fsw = new FileStream(DstFileName, FileMode.OpenOrCreate, FileAccess.Write);

            long numOfBytes = new FileInfo(SrcFileName).Length;
            long numOf3Bytes = numOfBytes / 3;
            byte[] readBuff = null;
            int threeBytes = 0;
            for (int i = 0; i < numOf3Bytes; ++i)
            {
                // make 3 bytes into one number
                readBuff = _br.ReadBytes(3);
                threeBytes = 0;
                threeBytes = readBuff[0];
                threeBytes <<= 8;
                threeBytes += readBuff[1];
                threeBytes <<= 8;
                threeBytes += readBuff[2];


                //divide the number into 4 6-bits series
                addByteToWBuffer((byte)_encodingList[(threeBytes >> 18)]);
                addByteToWBuffer((byte)_encodingList[((threeBytes >> 12) & 0x3F)]);
                addByteToWBuffer((byte)_encodingList[((threeBytes >> 6) & 0x3F)]);
                addByteToWBuffer((byte)_encodingList[(threeBytes & 0x3F)]);


                _fsw.Write(_writeBuff, 0, _wBuffIdx);
                _wBuffIdx = 0;
            }

            // remainder
            int numofRemainingBytes = (int)(numOfBytes % 3);
            if (numofRemainingBytes != 0)
            {
                readBuff = _br.ReadBytes(numofRemainingBytes);
                threeBytes = 0;
                threeBytes = readBuff[0];

                if (numofRemainingBytes == 1)
                {
                    addByteToWBuffer((byte)_encodingList[(threeBytes >> 2)]);
                    addByteToWBuffer((byte)_encodingList[((threeBytes << 4) & 0x3F)]);
                    addByteToWBuffer((byte)_encodingList.Last());
                }
                else
                {
                    threeBytes <<= 8;
                    threeBytes += readBuff[1];
                    addByteToWBuffer((byte)_encodingList[(threeBytes >> 10)]);
                    addByteToWBuffer((byte)_encodingList[((threeBytes >> 4) & 0x3F)]);
                    addByteToWBuffer((byte)_encodingList[((threeBytes << 2) & 0x3F)]);
                }
                addByteToWBuffer((byte)_encodingList.Last());

                _fsw.Write(_writeBuff, 0, _wBuffIdx);
                _wBuffIdx = 0;
            }


            _fsr.Close();
            _fsw.Flush();
            _fsw.Close();
        }


        public void guessTheExtension()
        {
            _fsr = new FileStream(SrcFileName, FileMode.Open, FileAccess.Read);
            _br = new BinaryReader(_fsr);

            char[] fileType = new char[24];
            fileType = _br.ReadChars(24);
            string fileTypeStr = new string(fileType);

            DstFileName = SrcFileName.Remove(SrcFileName.Length - 3, 3);

            if (fileTypeStr.Length >= JPG_TYPE.Length && fileTypeStr.Substring(0, JPG_TYPE.Length) == JPG_TYPE)
                DstFileName += "jpg";
            else if (fileTypeStr.Length >= PNG_TYPE.Length && fileTypeStr.Substring(0, PNG_TYPE.Length) == PNG_TYPE)
                DstFileName += "png";
            else if (fileTypeStr.Length >= BMP_TYPE.Length && fileTypeStr.Substring(0, BMP_TYPE.Length) == BMP_TYPE)
                DstFileName += "bmp";
            else if (fileTypeStr.Length >= DOCX_TYPE.Length && fileTypeStr.Substring(0, DOCX_TYPE.Length) == DOCX_TYPE)
                DstFileName += "docx";
            else if (fileTypeStr.Length >= ZIP_TYPE.Length && fileTypeStr.Substring(0, ZIP_TYPE.Length) == ZIP_TYPE)
                DstFileName += "zip";
            else
                DstFileName += "txt";


            _fsr.Seek(0, SeekOrigin.Begin);
        }


        private void readFourMeaningfulBytes()
        {
            _fourDecodedBytes.Clear();
            while (_fourDecodedBytes.Count != 4)
            {
                _oneChar = _br.ReadChar();
                _bytesRead++;

                if (_oneChar != '\n')
                {
                    _fourDecodedBytes.Add((byte)(_encodingList.IndexOf(_oneChar)));
                }
            }
        }


        public void decode()
        {
            _fsw = new FileStream(DstFileName, FileMode.OpenOrCreate, FileAccess.Write);

            long numOfBytes = new FileInfo(SrcFileName).Length;

            _bytesRead = 0;
            byte decodedByte = 0;
            while (_bytesRead != numOfBytes)
            {
                readFourMeaningfulBytes();
                _wBuffIdx = 0;

                decodedByte = (byte)(_fourDecodedBytes[0] << 2);
                decodedByte += (byte)(_fourDecodedBytes[1] >> 4);
                _writeBuff[_wBuffIdx++] = decodedByte;

                if (_encodingList[_fourDecodedBytes[3]] == (byte)_encodingList.Last())
                {
                    if (_encodingList[_fourDecodedBytes[2]] != (byte)_encodingList.Last())
                    {
                        decodedByte = (byte)(_fourDecodedBytes[1] << 4);
                        decodedByte += (byte)(_fourDecodedBytes[2] >> 2);
                        _writeBuff[_wBuffIdx++] = decodedByte;
                    }
                }
                else
                {
                    decodedByte = (byte)(_fourDecodedBytes[1] << 4);
                    decodedByte += (byte)(_fourDecodedBytes[2] >> 2);
                    _writeBuff[_wBuffIdx++] = decodedByte;

                    decodedByte = (byte)(_fourDecodedBytes[2] << 6);
                    decodedByte += (byte)(_fourDecodedBytes[3]);
                    _writeBuff[_wBuffIdx++] = decodedByte;
                }

                _fsw.Write(_writeBuff, 0, _wBuffIdx);
            }

            _fsr.Close();
            _fsw.Flush();
            _fsw.Close();
        }
    }
}
