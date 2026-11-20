// This is hell, I spent a week making this 600 line shit ass class
using System.IO;
public class FFmpegArg
{
    public enum supportedVideoEncoders
    {
        copy, // no encoding!
        gif, // gif only!

        //libaom_av1 is libaom-av1 and libvpx_vp9 is libvpx-vp9 REMEMBER DO NOT FORGET
        libx264, libx265, libaom_av1, libvpx, libvpx_vp9,

        // hardware varies!!
        h264_nvenc, hevc_nvenc, av1_nvenc, // nvidia
        h264_amf, hevc_amf, av1_amf,  // amd
        h264_qsv, hevc_qsv, av1_qsv, // intel
    }
    public enum encodingPresets
    {
        ultrafast, superfast, veryfast, faster, fast, medium, slow, veryslow, slower, placebo, //libx264 and libx265
        best, good, realtime, // VP8 and VP9
        p1, p2, p3, p4, p5, p6, p7, // nvidia
        speed, balanced, quality // amd
    }
    public enum supportedAudioEncoders
    {
        copy, // no encoding!
        aac, // aac
        libmp3lame, // mp3
        libvorbis, libopus, libspeex, // ogg
        pcm_s16le, pcm_s24le, pcm_s32le, pcm_f32le, pcm_f64le, pcm_alaw, // wav
        alac,
        flac  // flac & ogg
    }
    public enum supportedOutputFormats
    {
        MP4, MOV, GIF, MKV, WebM, OGG, AAC, WAV, FLAC, MP3
        // .aac is ADTS .mkv is MATROSKA
    }
    public enum videoRateControlTypes
    {
        CBR, VBR, CQP, CQ
    }
    public enum scalingFlags
    {
        fast_bilinear,bilinear,bicubic,neighbor,area,bicublin,gauss,sinc,lanczos,spline
    }
    public string? ffprobePath;
    public string? inputPath;
    public string? outputPath;
    public string? startTime;
    public string? endTime;
    public bool? isTestFile;
    public string? loggingMode;
    public supportedOutputFormats? outputFormat;
    public supportedVideoEncoders? videoEncoder;
    public videoRateControlTypes? videoRateControl;
    public string? videoBitrate;
    public string? CQConstant;
    public string? QPConstant;
    public string? MinimumVideoBitrate;
    public string? MaximumVideoBitrate;
    public string? BufferSize;
    public encodingPresets? videoEncodingSpeed;
    public string? cpuUsed;
    public string[]? videoResolution;
    public scalingFlags? videoscalingflag;
    public string? videoFramerate;

    public bool? accurateColorGif;
    public bool? disableAudio;
    public supportedAudioEncoders? audioEncoder;
    public string? audioBitrate;
    public string? audioChannelAmount;
    public bool? copyMetaData;
    public bool? defaultOnError;
    public static supportedVideoEncoders? StringToVideoEncoder(String target)
    {
        if (Enum.TryParse<supportedVideoEncoders>(target, out supportedVideoEncoders result))
        {
            return result;
        }
        return null;
    }
    public static supportedAudioEncoders? StringToAudioEncoder(String target)
    {
        if (Enum.TryParse<supportedAudioEncoders>(target, out supportedAudioEncoders result))
        {
            return result;
        }
        return null;
    }
    public static supportedOutputFormats? StringToOutputFormat(String target)
    {
        if (Enum.TryParse<supportedOutputFormats>(target, out supportedOutputFormats result))
        {
            return result;
        }
        return null;
    }
    public static encodingPresets? StringToEncoderPreset(String target)
    {
        if (Enum.TryParse<encodingPresets>(target, out encodingPresets result))
        {
            return result;
        }
        return null;
    }
    public static bool MultipleAudioSupport(supportedOutputFormats target)
    {
        switch (target.ToString())
        {
            case ("AAC" or "WAV" or "FLAC" or "MP3"):
                return false;
            default:
                return true;
        }
    }
    public static bool IsFormatVideo(supportedOutputFormats target)
    {
        switch (target)
        {
            case (supportedOutputFormats.OGG or supportedOutputFormats.AAC or supportedOutputFormats.WAV or supportedOutputFormats.FLAC or supportedOutputFormats.MP3):
                return false;
            default:
                return true;
        }
    }
    public static bool CheckCpuUsedCompatibility(supportedVideoEncoders target)
    {
        switch (target)
        {
            case (supportedVideoEncoders.libaom_av1 or supportedVideoEncoders.libvpx or supportedVideoEncoders.libvpx_vp9):
                return true;
            default:
                return false;
        }
    }
    public static supportedAudioEncoders[] GetAllCompatibleAudioEncoders(supportedOutputFormats target)
    {
        supportedAudioEncoders[] result = new supportedAudioEncoders[] {};
        supportedAudioEncoders[] all = Enum.GetValues<supportedAudioEncoders>();
        foreach (supportedAudioEncoders i in all)
        {
            if (i != supportedAudioEncoders.copy)
            {
                if (CheckAudioCompatibility(i, target) ?? false)
                {
                    result = [..result, i];
                }
            }
        }

        return result;
    }
    public static bool? CheckVideoCompatibility(supportedVideoEncoders videoEncoder, supportedOutputFormats outputFormat, string? ffprobePathArg=null, string? inputPathArg=null)
    {
        if (videoEncoder == supportedVideoEncoders.copy)
        {
            if (ffprobePathArg == null) {MessageBoxAPI.Error("FFprobe path not found,, this error occur while check video compatibility");return null;}
            if (inputPathArg == null) {MessageBoxAPI.Error("Input path not found,, this error occur while check video compatibility");return null;}
            string[] result = ProcessAPI.Start(ffprobePathArg, $" -v error -select_streams v:0 -show_entries stream=codec_name -of default=noprint_wrappers=1:nokey=1 \"{inputPathArg}\"");
            if (result[1] == "0")
            {
                switch (result[0])
                {
                    case ("h264"):
                        return CheckVideoCompatibility(supportedVideoEncoders.libx264, outputFormat);
                    case ("hevc"):
                        return CheckVideoCompatibility(supportedVideoEncoders.libx265, outputFormat);
                    case ("av1"):
                        return CheckVideoCompatibility(supportedVideoEncoders.libaom_av1, outputFormat);
                    case ("vp8"):
                        return CheckVideoCompatibility(supportedVideoEncoders.libvpx, outputFormat);
                    case ("vp9"):
                        return CheckVideoCompatibility(supportedVideoEncoders.libvpx_vp9, outputFormat);
                    case ("gif"):
                        return CheckVideoCompatibility(supportedVideoEncoders.gif, outputFormat);
                    default:
                        return false;
                }
            } else {return false;}
        }

        switch (outputFormat)
        {
            case (supportedOutputFormats.MP4):
                switch (videoEncoder)
                {
                    case not (supportedVideoEncoders.libvpx or supportedVideoEncoders.libvpx_vp9 or supportedVideoEncoders.gif):
                        return true;
                }
                break;
            case (supportedOutputFormats.MOV):
                switch (videoEncoder.ToString())
                {
                    case not ("libaom_av1" or "libvpx" or "libvpx_vp9" or "av1_nvenc" or "av1_amf" or "av1_qsv" or "gif"):
                        return true;
                }
                break;
            case (supportedOutputFormats.MKV):
                if (videoEncoder != supportedVideoEncoders.gif){return true;}
                break;
            case (supportedOutputFormats.WebM):
                switch (videoEncoder)
                {
                    case (supportedVideoEncoders.libaom_av1 or supportedVideoEncoders.libvpx or supportedVideoEncoders.libvpx_vp9
                    or supportedVideoEncoders.av1_nvenc or supportedVideoEncoders.av1_amf or supportedVideoEncoders.av1_qsv):
                    return true;
                }
                break;
            case (supportedOutputFormats.GIF):
                if (videoEncoder == supportedVideoEncoders.gif) {return true;}
                break;
        }
        return false;
    }

    //#####################################################################
    //#####################################################################

    public static bool? CheckAudioCompatibility(supportedAudioEncoders audioEncoder, supportedOutputFormats outputFormat, string? ffprobePathArg=null, string? inputPathArg=null)
    {
        if (audioEncoder == supportedAudioEncoders.copy)
        {
            if (ffprobePathArg == null) {MessageBoxAPI.Error("FFprobe path not found,, this error occur while check audio compatibility");return null;}
            if (inputPathArg == null) {MessageBoxAPI.Error("Input path not found,, this error occur while check audio compatibility");return null;}
            string[] result = ProcessAPI.Start(ffprobePathArg, $" -v error -select_streams a:0 -show_entries stream=codec_name -of default=noprint_wrappers=1:nokey=1 \"{inputPathArg}\"");
            if (result[1] == "0")
            {
                switch (result[0])
                {
                    case ("aac"):
                        return CheckAudioCompatibility(supportedAudioEncoders.aac, outputFormat);
                    case ("flac"):
                        return CheckAudioCompatibility(supportedAudioEncoders.flac, outputFormat);
                    case ("mp3"):
                        return CheckAudioCompatibility(supportedAudioEncoders.libmp3lame, outputFormat);
                    case ("vorbis"):
                        return CheckAudioCompatibility(supportedAudioEncoders.libvorbis, outputFormat);
                    case ("opus"):
                        return CheckAudioCompatibility(supportedAudioEncoders.libopus, outputFormat);
                    case ("speex"):
                        return CheckAudioCompatibility(supportedAudioEncoders.libspeex, outputFormat);
                    case ("alac"):
                        return CheckAudioCompatibility(supportedAudioEncoders.alac, outputFormat);
                    case ("pcm_s16le"):
                        return CheckAudioCompatibility(supportedAudioEncoders.pcm_s16le, outputFormat);
                    case ("pcm_s24le"):
                        return CheckAudioCompatibility(supportedAudioEncoders.pcm_s24le, outputFormat);
                    case ("pcm_s32le"):
                        return CheckAudioCompatibility(supportedAudioEncoders.pcm_s32le, outputFormat);
                    case ("pcm_f32le"):
                        return CheckAudioCompatibility(supportedAudioEncoders.pcm_f32le, outputFormat);
                    case ("pcm_f64le"):
                        return CheckAudioCompatibility(supportedAudioEncoders.pcm_f64le, outputFormat);
                    case ("pcm_alaw"):
                        return CheckAudioCompatibility(supportedAudioEncoders.pcm_alaw, outputFormat);
                    default:
                        return false;
                }
            } else {return false;}
        }
        switch (outputFormat)
        {
            case (supportedOutputFormats.MP4):
                switch (audioEncoder.ToString())
                {
                    case not ("libspeex" or "pcm_alaw"):
                        return true;
                }
                break;
            case (supportedOutputFormats.MOV):
                switch (audioEncoder.ToString())
                {
                    case not ("flac" or "libopus"):
                        return true;
                }
                break;
            case (supportedOutputFormats.MKV):
                return true; // Matroska supports everything :DD
            case (supportedOutputFormats.WebM):
                switch (audioEncoder)
                {
                    case (supportedAudioEncoders.libvorbis or supportedAudioEncoders.libopus):
                        return true;
                }
                break;
            case (supportedOutputFormats.OGG):
                switch (audioEncoder.ToString())
                {
                    case ("flac" or "libvorbis" or "libopus" or "libspeex"):
                        return true;
                }
                break;
            case (supportedOutputFormats.AAC):
                if (audioEncoder==supportedAudioEncoders.aac) {return true;}
                break;
            case (supportedOutputFormats.WAV):
                switch (audioEncoder.ToString())
                {
                    case ("aac" or "flac" or "libvorbis" or "pcm_s16le" or "pcm_s24le"
                            or "pcm_s32le" or "pcm_f32le" or "pcm_f64le" or "pcm_alaw"):
                        return true;
                }
                break;
            case (supportedOutputFormats.FLAC):
                if (audioEncoder==supportedAudioEncoders.flac) {return true;}
                break;
            case (supportedOutputFormats.MP3):
                if (audioEncoder==supportedAudioEncoders.libmp3lame) {return true;}
                break;
        }
        return false;
    }

    //#####################################################################
    //#####################################################################

    public static encodingPresets[] GetSupportedEncoderPresets(supportedVideoEncoders inputvideoEncoder)
    {
        encodingPresets[] result = new encodingPresets[] {};
        encodingPresets[] presets = Enum.GetValues<encodingPresets>();
        foreach (encodingPresets p in presets)
        {
            if (EncodingPresetCheck(p, inputvideoEncoder) != null)
            {
                result = [..result, p];
            }
        }

        return result;
    }

    //#####################################################################
    //#####################################################################
    public static string? EncodingPresetCheck(encodingPresets inputEncodingPreset, supportedVideoEncoders inputvideoEncoder, bool crash=false)
    {
        string result = "";
        switch (inputvideoEncoder)
        {
            case (supportedVideoEncoders.libx264 or supportedVideoEncoders.libx265):
                switch (inputEncodingPreset.ToString())
                {
                    case ("ultrafast" or "superfast" or "veryfast" or "faster" or "fast" or "medium" or
                            "slow" or "slower" or "veryslow" or "placebo"):
                        result += " -preset " + inputEncodingPreset.ToString();
                        break;
                    default:
                        if (!crash) {return null;}
                        MessageBoxAPI.Error("Libx264 and libx265 does not support your preset,, this error occur while converting ffmpeg arg to string");
                        return null;
                }
                break;
            case (supportedVideoEncoders.libvpx or supportedVideoEncoders.libvpx_vp9):
                switch (inputEncodingPreset)
                {
                    case (encodingPresets.best or encodingPresets.good or encodingPresets.realtime):
                        result += " -deadline " + inputEncodingPreset.ToString();
                        break;
                    default:
                        if (!crash) {return null;}
                        MessageBoxAPI.Error("VP8 and VP9 does not support your preset,, this error occur while converting ffmpeg arg to string");
                        return null;
                }
                break;
            case (supportedVideoEncoders.h264_nvenc or supportedVideoEncoders.hevc_nvenc or supportedVideoEncoders.av1_nvenc):
                switch (inputEncodingPreset.ToString())
                {
                    case ("p1" or "p2" or "p3" or "p4" or "p5" or "p6" or "p7"): //bro.
                        result += " -preset " + inputEncodingPreset.ToString();
                        break;
                    default:
                        if (!crash) {return null;}
                        MessageBoxAPI.Error("Nvidia encoders does not support your preset,, this error occur while converting ffmpeg arg to string");
                        return null;
                }
                break;
            case (supportedVideoEncoders.h264_amf or supportedVideoEncoders.hevc_amf or supportedVideoEncoders.av1_amf):
                switch (inputEncodingPreset)
                {
                    case (encodingPresets.speed or encodingPresets.balanced or encodingPresets.quality):
                        result += " -preset " + inputEncodingPreset.ToString();
                        break;
                    default:
                        if (!crash) {return null;}
                        MessageBoxAPI.Error("Amd encoders does not support your preset,, this error occur while converting ffmpeg arg to string");
                        return null;
                }
                break;
            case (supportedVideoEncoders.h264_qsv or supportedVideoEncoders.hevc_qsv or supportedVideoEncoders.av1_qsv):
                switch (inputEncodingPreset.ToString())
                {
                    case ("veryfast" or "faster" or "fast" or "medium" or "slow" or "slower" or "veryslow"):
                        result += " -preset " + inputEncodingPreset.ToString();
                        break;
                    default:
                        if (!crash) {return null;}
                        MessageBoxAPI.Error("Intel encoders does not support your preset,, this error occur while converting ffmpeg arg to string");
                        return null;
                }
                break;
        }
        return result == "" ? null : result;
    }

    //#####################################################################
    //#####################################################################

    public override String? ToString()
    {
        string result = " -y"; //saying yes bc the script will check if the output already exists then warns the user
        if (startTime!=null) {result+=" -ss "+startTime;}
        if (endTime!=null) {result+=" -to "+endTime;}
        if (isTestFile ?? false) {result += " -f lavfi";}
        if (loggingMode!=null) {result += " -v " + loggingMode;}
        if (inputPath!=null) {result+= $" -i \"{inputPath}\"";} else {MessageBoxAPI.Error("InputPath not found while converting ffmpeg arg class to string");return null;}
        if (outputPath==null) {MessageBoxAPI.Error("outputPath not found while converting ffmpeg arg class to string");return null;}
        if (outputFormat==null) {MessageBoxAPI.Error("output format arg not found while converting ffmpeg arg class to string");return null;}
        if (ffprobePath==null) {MessageBoxAPI.Error("FFprobePath not found while converting ffmpeg arg class to string");return null;}
        //if (videoEncoder==null) {MessageBoxAPI.Error("VideoEncoder not found while converting ffmpeg arg class to string");return null;}

        if (videoFramerate==null||videoResolution==null|| !(accurateColorGif ?? false))
        {result += " -map \"0:v:0?\" -map \"0:a:0?\"";}
        else
        {result += " -map \"0:a:0?\"";}

        if (videoEncoder !=null)
        {
            bool? cc = CheckVideoCompatibility((supportedVideoEncoders)videoEncoder, (supportedOutputFormats)outputFormat, ffprobePath, inputPath);
            if (cc==null) {return null;}
            if (!((bool)cc))
            {
                MessageBoxAPI.Error("Your file format does not support your video encoder,, this error occure while converting ffmpeg arg to string");
                return null;
            }

            switch (videoEncoder)
            {
                case (supportedVideoEncoders.libvpx_vp9):
                    result += " -c:v libvpx-vp9";
                    break;
                case (supportedVideoEncoders.libaom_av1):
                    result += " -c:v libaom-av1";
                    break;
                default:
                    result += " -c:v " + videoEncoder.ToString();
                    break;
            }

            if (videoRateControl!=null)
            {
                switch (videoRateControl)
                {
                    case videoRateControlTypes.CBR:
                        // constant bitrate
                        if (videoBitrate==null) {MessageBoxAPI.Error("VideoBitrate not found while converting ffmpeg arg class to string");return null;}

                        switch (videoEncoder)
                        {
                            case supportedVideoEncoders.h264_nvenc or supportedVideoEncoders.hevc_nvenc or supportedVideoEncoders.av1_nvenc:
                                //nvidia
                                result += " -rc cbr -b:v " + videoBitrate;
                                break;
                            case supportedVideoEncoders.h264_amf or supportedVideoEncoders.hevc_amf or supportedVideoEncoders.av1_amf:
                                //amd
                                result += " -rc cbr -b:v " + videoBitrate; 
                                break;
                            default:
                                result += $" -b:v {videoBitrate} -minrate {videoBitrate} -maxrate {videoBitrate}";
                                if (BufferSize!=null){result += " -bufsize " + BufferSize;}
                                break;
                        }

                        break;
                    case videoRateControlTypes.VBR:

                        if (videoBitrate==null) {MessageBoxAPI.Error("VideoBitrate not found while converting ffmpeg arg class to string");return null;}

                        switch (videoEncoder)
                        {
                            case supportedVideoEncoders.h264_nvenc or supportedVideoEncoders.hevc_nvenc or supportedVideoEncoders.av1_nvenc:
                                //nvidia
                                result += " -rc vbr";
                                break;
                            case supportedVideoEncoders.h264_amf or supportedVideoEncoders.hevc_amf or supportedVideoEncoders.av1_amf:
                                //amd
                                result += " -rc vbr_peak";
                                break;
                        }
                        result += " -b:v " + videoBitrate;
                        if (MinimumVideoBitrate!=null) {result+=" -minrate "+MinimumVideoBitrate;} // optional
                        if (MaximumVideoBitrate!=null) {result+=" -maxrate "+MaximumVideoBitrate;}
                        break;
                    case videoRateControlTypes.CQP:

                        if (QPConstant==null) {MessageBoxAPI.Error("QPConstant not found while converting ffmpeg arg class to string");return null;}

                        switch (videoEncoder)
                        {
                            case supportedVideoEncoders.h264_nvenc or supportedVideoEncoders.hevc_nvenc or supportedVideoEncoders.av1_nvenc:
                                //nvidia
                                result += " -rc constqp -qp " + QPConstant;
                                break;
                            case supportedVideoEncoders.h264_amf or supportedVideoEncoders.hevc_amf or supportedVideoEncoders.av1_amf:
                                //amd
                                //not even gonna bother adding option for i and p frame
                                result += $" -rc cqp -qp_i {QPConstant} -qp_p {QPConstant}";
                                break;
                            case supportedVideoEncoders.h264_qsv or supportedVideoEncoders.hevc_qsv or supportedVideoEncoders.av1_qsv:
                                //intel
                                result += " -qscale:v 1 -global_quality " + QPConstant;
                                break;
                            case supportedVideoEncoders.libaom_av1 or supportedVideoEncoders.libvpx or supportedVideoEncoders.libvpx_vp9:
                                MessageBoxAPI.Error("your library does not support CQP rate control this happen while converting ffmpeg arg class to string");
                                return null;

                            default:
                                result += " -qp " + QPConstant;
                                break;
                        }
                        break;
                    case videoRateControlTypes.CQ:
                        if (CQConstant==null) {MessageBoxAPI.Error("CQConstant not found while converting ffmpeg arg class to string");return null;}

                        switch (videoEncoder)
                        {
                            case supportedVideoEncoders.h264_nvenc or supportedVideoEncoders.hevc_nvenc or supportedVideoEncoders.av1_nvenc:
                                //nvidia
                                result += $" -rc vbr -cq {CQConstant} -b:v 0";
                                break;
                            case supportedVideoEncoders.h264_amf or supportedVideoEncoders.hevc_amf or supportedVideoEncoders.av1_amf:
                                //amd
                                result += " -rc qvbr -qvbr_quality_level " + CQConstant;
                                break;
                            case supportedVideoEncoders.h264_qsv or supportedVideoEncoders.hevc_qsv or supportedVideoEncoders.av1_qsv:
                                //intel
                                result += " -global_quality " + CQConstant;
                                break;
                            case supportedVideoEncoders.libaom_av1 or supportedVideoEncoders.libvpx or supportedVideoEncoders.libvpx_vp9:
                                // idk
                                result += $" -crf {CQConstant} -b:v 0";
                                break;
                            default:
                                result += " -crf " + CQConstant;
                                break;
                        }
                        break;

                }
            }

            if (videoEncodingSpeed!=null)
            {
                result += EncodingPresetCheck((encodingPresets)videoEncodingSpeed, (supportedVideoEncoders)videoEncoder, true);
            }

            if (cpuUsed != null)
            {
                switch (videoEncoder)
                {
                    case (supportedVideoEncoders.libaom_av1 or supportedVideoEncoders.libvpx or supportedVideoEncoders.libvpx_vp9):
                        result += " -cpu-used " + cpuUsed;
                        break;
                    default:
                        MessageBoxAPI.Error("Your encoder does not support -cpu-used,, this error occur while converting ffmpeg arg to string");
                        return null;
                }
            }
        } // (videoEncoder != null) if statement ends here

        if (videoFramerate!=null)
        {
            result += " -filter_complex \"[0:v]fps="+videoFramerate;
            if (videoResolution!=null)
            {
                result += $",scale={videoResolution[0]}:{videoResolution[1]}";
                if (videoscalingflag!=null) {result += ":flags="+videoscalingflag.ToString();}
            }
            if (accurateColorGif ?? false)
            {
                if (outputFormat == supportedOutputFormats.GIF)
                {
                    result += ",split[a][b];[a]palettegen[p];[b][p]paletteuse=dither=sierra2_4a";
                }
            }
            result += "[outv]\" -map \"[outv]\"";

        } else if (videoResolution!=null) //video res is null
        {
            result += $" -filter_complex \"[0:v]scale={videoResolution[0]}:{videoResolution[1]}";
            if (videoscalingflag!=null) {result += ":flags="+videoscalingflag.ToString();}
            if (accurateColorGif ?? false)
            {
                if (outputFormat == supportedOutputFormats.GIF)
                {
                    result += ",split[a][b];[a]palettegen[p];[b][p]paletteuse=dither=sierra2_4a";
                }
            }
            result += "[outv]\" -map \"[outv]\"";
        } else if (accurateColorGif ?? false) // fps and res is null
        {
            if (outputFormat == supportedOutputFormats.GIF)
            {
                result += " -filter_complex \"[0:v]split[a][b];[a]palettegen[p];[b][p]paletteuse=dither=sierra2_4a[outv]\" -map \"[outv]\"";
            }
        }

        // AUDIO

        if (disableAudio ?? false || audioEncoder == null){result += " -an";} else if (audioEncoder != null)
        { //audio is on
            bool? cca = CheckAudioCompatibility((supportedAudioEncoders)audioEncoder, (supportedOutputFormats)outputFormat, ffprobePath, inputPath);
            if (cca==null) {return null;}
            if ((bool)cca)
            {
                result += " -c:a " + audioEncoder.ToString();
            } else
            {
                MessageBoxAPI.Error("Your audio format does not support your encoder,, this error occur while converting ffmpeg arg to string");
                return null;
            }


            if (audioBitrate!=null) {result += " -b:a " + audioBitrate;}
            if (audioChannelAmount!=null) {result += " -ac " + audioChannelAmount;}
        }

        if (copyMetaData ?? true)
        {result += " -map_metadata 0";}
        else
        {result += " -map_metadata -1";}

        switch (outputFormat)
        {
            case (supportedOutputFormats.MKV):
                result += " -f Matroska";
                break;
            case (supportedOutputFormats.AAC):
                result += " -f adts";
                break;
            default:
                result += " -f " + outputFormat.ToString()?.ToLower();
                break;
        }

        result += $" \"{outputPath}\"";

        string textPathDir = Path.GetDirectoryName(ffprobePath)!;
        string textPath = Path.Combine(textPathDir, "LastFFmpegArg.txt");
        File.WriteAllText(textPath, result);

        return result;
    }
}