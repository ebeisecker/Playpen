using System;
using MonoTouch.Foundation;
using MonoTouch.AudioToolbox;
using System.Runtime.InteropServices;

namespace MusicCube
{
	public static class MyOpenALSupport
	{
		const int AL_FORMAT_STEREO16 = 0x1103; 
		const int AL_FORMAT_MONO16 = 0x1101;
			
		public static void BufferDataStaticProc(int bid, int format, byte[] data, int size, int freq)
		{
			
		}
		
		public static byte[] MyGetOpenALAudioData(NSUrl inFileUrl, out int outDataSize, out int outDataFormat, out double outSampleRate)
		{
			long fileDataSize = 0;
			AudioStreamBasicDescription theFileFormat;
			AudioFile afid;
			byte[] theData = null;
			
			// Open a file with ExtAudioFileOpen()
			using(afid = AudioFile.Open(inFileUrl, AudioFilePermission.Read, (AudioFileType)0))
			{
				// Get the audio data format
				theFileFormat = afid.StreamBasicDescription;
				
				if(theFileFormat.ChannelsPerFrame > 2)
					Console.WriteLine("MyGetOpenALAudioData - Unsupported Format, channel count is greater than stero");
				
				if(theFileFormat.BitsPerChannel != 8 && theFileFormat.BitsPerChannel != 16)
					Console.WriteLine("MyGetOpenALAudioData - Unsupported Format, must be 8 or 16 bit PCM");
				
				fileDataSize = afid.DataPacketCount;
				
				// Read all the data into Memory
				var dataSize = fileDataSize;
				theData = new byte[dataSize];
				
				afid.ReadPacketData(false, 0, Convert.ToInt32(dataSize), theData, 0, theData.Length);
				outDataSize = Convert.ToInt32(dataSize);
				outDataFormat = theFileFormat.ChannelsPerFrame > 1 ? AL_FORMAT_STEREO16 : AL_FORMAT_MONO16;
				outSampleRate = theFileFormat.SampleRate;
				
				return theData;
			}
		}
	}
}
