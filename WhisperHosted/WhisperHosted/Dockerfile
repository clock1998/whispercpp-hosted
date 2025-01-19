# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
USER root
RUN apt update
RUN apt install -y git build-essential ffmpeg wget cmake
RUN git clone https://github.com/ggerganov/whisper.cpp.git
WORKDIR ./whisper.cpp
#RUN make -j large-v3-turbo
RUN sh ./models/download-ggml-model.sh base
RUN cmake -B build
RUN cmake --build build --config Release
RUN ./build/bin/quantize models/ggml-base.bin models/ggml-base.q5_0.bin q5_0

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

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
USER app
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WhisperHosted.dll"]
