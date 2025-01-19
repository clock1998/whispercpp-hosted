using Microsoft.JSInterop;
using System.Diagnostics;

namespace WhisperHosted.Components.Pages
{
    public partial class Home
    {
        public async Task<string> TranscribeAudioAsync(string inputPath)
        {
            string tempWavFilePath = Path.Combine(Path.GetDirectoryName(inputPath)!, Path.ChangeExtension(Path.GetRandomFileName(), ".wav")) ;
            
            string outputPath = Path.Combine(Path.GetDirectoryName(inputPath)!, Path.GetFileNameWithoutExtension(inputPath)); // Default output path with .txt extension
            // Register cleanup on exit
            AppDomain.CurrentDomain.ProcessExit += (sender, e) => Cleanup(tempWavFilePath);

            try
            {
                // Check if the input file exists
                if (!File.Exists(inputPath))
                {
                    throw new FileNotFoundException("Input file not found.", inputPath);
                }

                // Convert the input audio file to WAV format ffmpeg -i "./unsafe_uploads/jpn1gcl1.m4a" -ar 16000 -ac 1 -c:a pcm_s16le "asd.wav"
                if (!await RunProcessAsync("ffmpeg", $"-i \"{inputPath}\" -ar 16000 -ac 1 -c:a pcm_s16le \"{tempWavFilePath}\""))
                {
                    throw new Exception("Failed to convert audio file to WAV format.");
                }

                // Perform transcription using Whisper CLI ggml-large-v3-turbo.bin
                if (!await RunProcessAsync("./whisper.cpp/build/bin/whisper-cli", $"-mc 0 -otxt -of \"{outputPath}\" -m ./whisper.cpp/models/ggml-base.q5_0.bin -f \"{tempWavFilePath}\""))
                {
                    throw new Exception("Transcription process failed.");
                }

                return Path.ChangeExtension(outputPath, ".txt");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                throw ex;
            }
            finally
            {
                // Ensure cleanup of temporary files
                Cleanup(tempWavFilePath);
            }
        }

        private void Cleanup(string tempFile)
        {
            if (File.Exists(tempFile))
            {
                File.Delete(tempFile);
            }
        }

        private async Task<bool> RunProcessAsync(string command, string arguments)
        {
            try
            {
                Process process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = command,
                        Arguments = arguments,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                while (!process.HasExited)
                {
                    if (!process.StandardOutput.EndOfStream)
                    {
                        string? line = await process.StandardOutput.ReadLineAsync();
                        if (line != null)
                        {
                            whisperProgress += line + "\n";
                            Logger.LogInformation("Updating UI with progress: {line}", line);
                            StateHasChanged();
                            await module.InvokeVoidAsync("scrollToBottom", progressDiv);
                        }
                    }
                    // await Task.Delay(100); // Prevents tight-looping
                }

                string standardError = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (process.ExitCode != 0)
                {
                    throw new Exception($"Process failed with exit code {process.ExitCode}.\nStandard Error: {standardError}");
                }


                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while running the process: {ex.Message}", ex);
            }
        }
    }
}
