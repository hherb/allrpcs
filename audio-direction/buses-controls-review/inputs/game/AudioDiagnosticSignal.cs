using System.Buffers.Binary;
using Godot;

namespace Allrpcs.Game;

/// <summary>Creates temporary diagnostic PCM signals only; no production audio assets.</summary>
internal static class AudioDiagnosticSignal
{
    private const int SampleRate = 48000;

    /// <summary>Creates a one-second looping sine probe, optionally confined to the left channel.</summary>
    internal static AudioStreamWav Create(double frequency, double amplitude = 0.01, bool leftOnly = false)
    {
        byte[] data = new byte[SampleRate * 4];
        for (int index = 0; index < SampleRate; index++)
        {
            short sample = (short)(Math.Sin(2 * Math.PI * frequency * index / SampleRate) * amplitude * short.MaxValue);
            BinaryPrimitives.WriteInt16LittleEndian(data.AsSpan(index * 4, 2), sample);
            BinaryPrimitives.WriteInt16LittleEndian(data.AsSpan(index * 4 + 2, 2), leftOnly ? (short)0 : sample);
        }
        return new AudioStreamWav
        {
            Data = data,
            Format = AudioStreamWav.FormatEnum.Format16Bits,
            MixRate = SampleRate,
            Stereo = true,
            LoopMode = AudioStreamWav.LoopModeEnum.Forward,
            LoopBegin = 0,
            LoopEnd = SampleRate,
        };
    }
}
