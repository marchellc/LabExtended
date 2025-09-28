namespace LabExtended.API.Custom.Voice.Threading.Pitch;

/// <summary>
/// Utilities for modifying voice pitch.
/// </summary>
public static class VoicePitchUtils
{
    /// <summary>
    /// The maximum length of a frame.
    /// </summary>
    public const int MAX_FRAME_LENGTH = 16000;

    /// <summary>
    /// Performs an in-place Short-Time Fourier Transform (STFT) on the specified buffer using the given frame size and
    /// transform direction.
    /// </summary>
    /// <remarks>The input buffer must be organized as pairs of real and imaginary values: [Re0, Im0, Re1,
    /// Im1, ..., ReN, ImN]. The method modifies the input buffer directly and does not allocate additional arrays. The
    /// transform is most commonly used for spectral analysis or signal processing tasks. This method is not
    /// thread-safe.</remarks>
    /// <param name="fftBuffer">The array of interleaved real and imaginary parts representing the input signal. The transform is performed
    /// in-place, and the buffer will contain the transformed data upon completion. The length of the array must be at
    /// least 2 × fftFrameSize.</param>
    /// <param name="fftFrameSize">The number of complex samples in the frame to be transformed. Must be a power of two and greater than zero.</param>
    /// <param name="sign">The direction of the transform. Use 1 for the forward transform and -1 for the inverse transform.</param>
    public static void ShortTimeFourierTransform(float[] fftBuffer, long fftFrameSize, long sign)
    {
        float wr, wi, arg, temp;
        float tr, ti, ur, ui;
        long i, bitm, j, le, le2, k;

        for (i = 2; i < 2 * fftFrameSize - 2; i += 2)
        {
            for (bitm = 2, j = 0; bitm < 2 * fftFrameSize; bitm <<= 1)
            {
                if ((i & bitm) != 0)
                    j++;

                j <<= 1;
            }

            if (i < j)
            {
                temp = fftBuffer[i];

                fftBuffer[i] = fftBuffer[j];
                fftBuffer[j] = temp;

                temp = fftBuffer[i + 1];

                fftBuffer[i + 1] = fftBuffer[j + 1];
                fftBuffer[j + 1] = temp;
            }
        }

        long max = (long)(Math.Log(fftFrameSize) / Math.Log(2.0) + .5);

        for (k = 0, le = 2; k < max; k++)
        {
            le <<= 1;
            le2 = le >> 1;

            ur = 1.0F;
            ui = 0.0F;

            arg = (float)Math.PI / (le2 >> 1);
            wr = (float)Math.Cos(arg);
            wi = (float)(sign * Math.Sin(arg));

            for (j = 0; j < le2; j += 2)
            {

                for (i = j; i < 2 * fftFrameSize; i += le)
                {
                    tr = fftBuffer[i + le2] * ur - fftBuffer[i + le2 + 1] * ui;
                    ti = fftBuffer[i + le2] * ui + fftBuffer[i + le2 + 1] * ur;

                    fftBuffer[i + le2] = fftBuffer[i] - tr;
                    fftBuffer[i + le2 + 1] = fftBuffer[i + 1] - ti;

                    fftBuffer[i] += tr;
                    fftBuffer[i + 1] += ti;

                }

                tr = ur * wr - ui * wi;
                ui = ur * wi + ui * wr;
                ur = tr;
            }
        }
    }
}