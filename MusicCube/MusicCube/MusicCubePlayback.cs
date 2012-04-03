using System;
using MonoTouch.AudioToolbox;
using MonoTouch.EventKit;
using MonoTouch.Foundation;

namespace MusicCube
{
	public class MusicCubePlayback
	{
		#region Properties
		public bool IsPlaying { get { return isPLaying; } }
		public bool WasInterrupted { get { return wasInterrupted; } }
		public float[] SourcePos { get { return sourcePos; } }
		public float[] ListenerPos { get { return listenerPos; } }
		public float ListenerRotation { get { return listenerRotation; } } 
		#endregion
		
		#region Members
		uint source;
	    uint buffer;
		object data;
		float[] sourcePos = new float[3];
		float[] listenerPos = new float[3];
		float listenerRotation;
		float sourceVolume;
		bool isPLaying;
		bool wasInterrupted;
		#endregion
		
		public MusicCubePlayback ()
		{
			// Initial position of the sound source and
			// initial position and rotation of the listener
			// will be set by the view
			
			// setup our audio session
			AudioSession.Initialize();
			
			wasInterrupted = false;
			
			// Initialize our OpenAL environment
			InitOpenAL();
		}
		
		void InterruptionListener(object inClientData, AudioSessionInterruptionState inInterruptionState)
		{
			if(inInterruptionState == AudioSessionInterruptionState.Begin)
			{
				// Do nothing
				if(this.isPLaying)
				{
					this.wasInterrupted = true;
					this.isPLaying = false;
				}
			}
			else if(inInterruptionState == AudioSessionInterruptionState.End)
			{
				AudioSession.SetActive(true);
				InitOpenAL();
				if(this.wasInterrupted)
				{
					this.StartSound();
					this.wasInterrupted = false;
				}
			}
		}		                     
		
		#region Object Init / Maintenance
		
		#endregion 
		
		#region Open AL
		
		void InitBuffer()
		{
			int format;
			int size;
			double freq;
			
			var bundle = NSBundle.MainBundle;
			
			// Get some audio data from a wave file
			var fileUrl = new NSUrl(bundle.PathForResource("Sounds/sound", "wave"));
			if(fileUrl == null)
			{
				data = MyOpenALSupport.MyGetOpenALAudioData(fileUrl, out size, out format, out freq);
				
				// Use the static buffer Data API
				
			}
			else
			{
				Console.WriteLine("Could not find file!");
				data = null;
			}
		}
		
		void InitOpenAL()
		{
		}
		
		void TearDownOpenAL()
		{
		}
		
		#endregion
		
		void StartSound()
		{
		}
		
		void StopSound()
		{
		}
	}
}

