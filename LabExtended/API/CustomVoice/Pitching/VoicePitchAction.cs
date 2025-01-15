namespace LabExtended.API.CustomVoice.Pitching;

public class VoicePitchAction : IVoicePitchAction, IDisposable
{
    private float[] gInFIFO = new float[VoicePitchUtils.MAX_FRAME_LENGTH];
    private float[] gOutFIFO = new float[VoicePitchUtils.MAX_FRAME_LENGTH];
    private float[] gFFTworksp = new float[2 * VoicePitchUtils.MAX_FRAME_LENGTH];
    private float[] gLastPhase = new float[VoicePitchUtils.MAX_FRAME_LENGTH / 2 + 1];
    private float[] gSumPhase = new float[VoicePitchUtils.MAX_FRAME_LENGTH / 2 + 1];
    private float[] gOutputAccum = new float[2 * VoicePitchUtils.MAX_FRAME_LENGTH];
    private float[] gAnaFreq = new float[VoicePitchUtils.MAX_FRAME_LENGTH];
    private float[] gAnaMagn = new float[VoicePitchUtils.MAX_FRAME_LENGTH];
    private float[] gSynFreq = new float[VoicePitchUtils.MAX_FRAME_LENGTH];
    private float[] gSynMagn = new float[VoicePitchUtils.MAX_FRAME_LENGTH];

    private long gRover;
    private long gInit;

    public void Modify(ref VoicePitchPacket packet)
    {
        var data = new float[48000];
        
        packet.Decoder.Decode(packet.Data, packet.Length, data);

        PitchShift(packet.Pitch, 480U, 48000, data);

        packet.Length = packet.Encoder.Encode(data, packet.Data, 480);
    }

    public void Dispose()
    {
        gInFIFO = null;
        gOutFIFO = null;
            
        gFFTworksp = null;
        gOutputAccum = null;

        gLastPhase = null;
        gSumPhase = null;

        gAnaFreq = null;
        gAnaMagn = null;

        gSynFreq = null;
        gSynMagn = null;

        gRover = 0;
        gInit = 0;
    }
    
    public void PitchShift(float pitchShift, long numSampsToProcess, float sampleRate, float[] indata)
        => PitchShift(pitchShift, numSampsToProcess, 2048, 10, sampleRate, indata);

    private void PitchShift(float pitchShift, long numSampsToProcess, long fftFrameSize, long osamp, float sampleRate, float[] indata)
    {
        double magn, phase, tmp, window, real, imag;
        double freqPerBin, expct;
        long i, k, qpd, index, inFifoLatency, stepSize, fftFrameSize2;

        float[] outdata = indata;

        /* set up some handy variables */
        fftFrameSize2 = fftFrameSize / 2;
        stepSize = fftFrameSize / osamp;
        freqPerBin = sampleRate / (double)fftFrameSize;
        expct = 2.0 * Math.PI * (double)stepSize / (double)fftFrameSize;
        inFifoLatency = fftFrameSize - stepSize;

        if (gRover == 0)
            gRover = inFifoLatency;

        /* main processing loop */
        for (i = 0; i < numSampsToProcess; i++)
        {
            /* As long as we have not yet collected enough data just read in */
            gInFIFO[gRover] = indata[i];
            outdata[i] = gOutFIFO[gRover - inFifoLatency];
            gRover++;

            /* now we have enough data for processing */
            if (gRover >= fftFrameSize)
            {
                gRover = inFifoLatency;

                /* do windowing and re,im interleave */
                for (k = 0; k < fftFrameSize; k++)
                {
                    window = -.5 * Math.Cos(2.0 * Math.PI * (double)k / (double)fftFrameSize) + .5;

                    gFFTworksp[2 * k] = (float)(gInFIFO[k] * window);
                    gFFTworksp[2 * k + 1] = 0.0F;
                }


                /* ***************** ANALYSIS ******************* */
                /* do transform */
                VoicePitchUtils.ShortTimeFourierTransform(gFFTworksp, fftFrameSize, -1);

                /* this is the analysis step */
                for (k = 0; k <= fftFrameSize2; k++)
                {

                    /* de-interlace FFT buffer */
                    real = gFFTworksp[2 * k];
                    imag = gFFTworksp[2 * k + 1];

                    /* compute magnitude and phase */
                    magn = 2.0 * Math.Sqrt(real * real + imag * imag);
                    phase = Math.Atan2(imag, real);

                    /* compute phase difference */
                    tmp = phase - gLastPhase[k];
                    gLastPhase[k] = (float)phase;

                    /* subtract expected phase difference */
                    tmp -= (double)k * expct;

                    /* map delta phase into +/- Pi interval */
                    qpd = (long)(tmp / Math.PI);
                    if (qpd >= 0) qpd += qpd & 1;
                    else qpd -= qpd & 1;
                    tmp -= Math.PI * (double)qpd;

                    /* get deviation from bin frequency from the +/- Pi interval */
                    tmp = osamp * tmp / (2.0 * Math.PI);

                    /* compute the k-th partials' true frequency */
                    tmp = (double)k * freqPerBin + tmp * freqPerBin;

                    /* store magnitude and true frequency in analysis arrays */
                    gAnaMagn[k] = (float)magn;
                    gAnaFreq[k] = (float)tmp;

                }

                /* ***************** PROCESSING ******************* */
                /* this does the actual pitch shifting */
                for (int zero = 0; zero < fftFrameSize; zero++)
                {
                    gSynMagn[zero] = 0;
                    gSynFreq[zero] = 0;
                }

                for (k = 0; k <= fftFrameSize2; k++)
                {
                    index = (long)(k * pitchShift);

                    if (index <= fftFrameSize2)
                    {
                        gSynMagn[index] += gAnaMagn[k];
                        gSynFreq[index] = gAnaFreq[k] * pitchShift;
                    }
                }

                /* ***************** SYNTHESIS ******************* */
                /* this is the synthesis step */
                for (k = 0; k <= fftFrameSize2; k++)
                {

                    /* get magnitude and true frequency from synthesis arrays */
                    magn = gSynMagn[k];
                    tmp = gSynFreq[k];

                    /* subtract bin mid frequency */
                    tmp -= (double)k * freqPerBin;

                    /* get bin deviation from freq deviation */
                    tmp /= freqPerBin;

                    /* take osamp into account */
                    tmp = 2.0 * Math.PI * tmp / osamp;

                    /* add the overlap phase advance back in */
                    tmp += (double)k * expct;

                    /* accumulate delta phase to get bin phase */
                    gSumPhase[k] += (float)tmp;
                    phase = gSumPhase[k];

                    /* get real and imag part and re-interleave */
                    gFFTworksp[2 * k] = (float)(magn * Math.Cos(phase));
                    gFFTworksp[2 * k + 1] = (float)(magn * Math.Sin(phase));
                }

                /* zero negative frequencies */
                for (k = fftFrameSize + 2; k < 2 * fftFrameSize; k++)
                    gFFTworksp[k] = 0.0F;

                /* do inverse transform */
                VoicePitchUtils.ShortTimeFourierTransform(gFFTworksp, fftFrameSize, 1);

                /* do windowing and add to output accumulator */
                for (k = 0; k < fftFrameSize; k++)
                {
                    window = -.5 * Math.Cos(2.0 * Math.PI * (double)k / (double)fftFrameSize) + .5;
                    gOutputAccum[k] += (float)(2.0 * window * gFFTworksp[2 * k] / (fftFrameSize2 * osamp));
                }

                for (k = 0; k < stepSize; k++)
                    gOutFIFO[k] = gOutputAccum[k];

                /* shift accumulator */
                //memmove(gOutputAccum, gOutputAccum + stepSize, fftFrameSize * sizeof(float));
                for (k = 0; k < fftFrameSize; k++)
                    gOutputAccum[k] = gOutputAccum[k + stepSize];

                /* move input FIFO */
                for (k = 0; k < inFifoLatency; k++)
                    gInFIFO[k] = gInFIFO[k + stepSize];
            }
        }
    }
}