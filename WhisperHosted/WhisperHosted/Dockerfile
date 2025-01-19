# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["WhisperHosted/WhisperHosted/WhisperHosted.csproj", "WhisperHosted/WhisperHosted/"]
COPY ["WhisperHosted/WhisperHosted.Client/WhisperHosted.Client.csproj", "WhisperHosted/WhisperHosted.Client/"]
RUN dotnet restore "./WhisperHosted/WhisperHosted/WhisperHosted.csproj"
COPY . .
WORKDIR "/src/WhisperHosted/WhisperHosted"
RUN dotnet build "./WhisperHosted.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./WhisperHosted.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Add Whisper dependencies
#FROM ubuntu:22.04 AS whisper
#RUN apt update
#RUN apt install -y git
#RUN apt install -y build-essential wget cmake
#RUN git clone https://github.com/ggerganov/whisper.cpp.git
##RUN make -j base
#WORKDIR ./whisper.cpp
#RUN sh ./models/download-ggml-model.sh base.en
#RUN cmake -B build
#RUN cmake --build build --config Release
# Pre-download Whisper model (adjust `base` to other models if needed)


# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
USER root
WORKDIR /app
RUN apt update
RUN apt install -y git
RUN apt install -y build-essential ffmpeg  wget cmake
RUN git clone https://github.com/ggerganov/whisper.cpp.git
#RUN make -j base
WORKDIR ./whisper.cpp
RUN sh ./models/download-ggml-model.sh base.en
RUN cmake -B build
RUN cmake --build build --config Release

WORKDIR /app
COPY --from=publish /app/publish .
# Copy Whisper binaries and pre-downloaded models
#COPY --from=whisper ./whisper.cpp ./whisper.cpp
# Set environment variables or volumes for Whisper
ENV WHISPER_MODELS_PATH=/whisper/models
ENV WHISPER_AUDIO_PATH=/whisper/audios
ENTRYPOINT ["dotnet", "WhisperHosted.dll"]
