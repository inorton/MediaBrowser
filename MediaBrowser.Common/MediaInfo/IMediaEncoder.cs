﻿using System;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Entities;

namespace MediaBrowser.Common.MediaInfo
{
    /// <summary>
    /// Interface IMediaEncoder
    /// </summary>
    public interface IMediaEncoder
    {
        /// <summary>
        /// Gets the encoder path.
        /// </summary>
        /// <value>The encoder path.</value>
        string EncoderPath { get; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        string Version { get; }

        /// <summary>
        /// Extracts the image.
        /// </summary>
        /// <param name="inputFiles">The input files.</param>
        /// <param name="type">The type.</param>
        /// <param name="threedFormat">The threed format.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="outputPath">The output path.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task ExtractImage(string[] inputFiles, InputType type, Video3DFormat? threedFormat, TimeSpan? offset, string outputPath, CancellationToken cancellationToken);

        /// <summary>
        /// Extracts the text subtitle.
        /// </summary>
        /// <param name="inputFiles">The input files.</param>
        /// <param name="type">The type.</param>
        /// <param name="subtitleStreamIndex">Index of the subtitle stream.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="outputPath">The output path.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task ExtractTextSubtitle(string[] inputFiles, InputType type, int subtitleStreamIndex, TimeSpan offset, string outputPath, CancellationToken cancellationToken);

        /// <summary>
        /// Converts the text subtitle to ass.
        /// </summary>
        /// <param name="inputPath">The input path.</param>
        /// <param name="outputPath">The output path.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task ConvertTextSubtitleToAss(string inputPath, string outputPath, TimeSpan offset, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the media info.
        /// </summary>
        /// <param name="inputFiles">The input files.</param>
        /// <param name="type">The type.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Task.</returns>
        Task<MediaInfoResult> GetMediaInfo(string[] inputFiles, InputType type, CancellationToken cancellationToken);

        /// <summary>
        /// Gets the probe size argument.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>System.String.</returns>
        string GetProbeSizeArgument(InputType type);

        /// <summary>
        /// Gets the input argument.
        /// </summary>
        /// <param name="inputFiles">The input files.</param>
        /// <param name="type">The type.</param>
        /// <returns>System.String.</returns>
        string GetInputArgument(string[] inputFiles, InputType type);
    }

    /// <summary>
    /// Enum InputType
    /// </summary>
    public enum InputType
    {
        /// <summary>
        /// The audio file
        /// </summary>
        AudioFile,
        /// <summary>
        /// The video file
        /// </summary>
        VideoFile,
        /// <summary>
        /// The bluray
        /// </summary>
        Bluray,
        /// <summary>
        /// The DVD
        /// </summary>
        Dvd,
        /// <summary>
        /// The URL
        /// </summary>
        Url
    }
}
