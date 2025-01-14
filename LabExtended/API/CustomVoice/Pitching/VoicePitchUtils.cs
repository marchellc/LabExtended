namespace LabExtended.API.CustomVoice.Pitching;

public static class VoicePitchUtils
{
    public const int MAX_FRAME_LENGTH = 16000;

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