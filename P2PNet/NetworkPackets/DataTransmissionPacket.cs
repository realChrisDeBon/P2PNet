using static P2PNet.Distribution.Distribution_Protocol;
using P2PNet.NetworkPackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PNet.NetworkPackets
    {
    /// <summary>
    /// Represents a data transmission packet used for transmitting data throughout the peer network.
    /// </summary>
    public sealed class DataTransmissionPacket : INetworkPacket
        {
        /// <summary>
        /// Gets or sets the data contained in the packet.
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// Gets or sets the format of the data.
        /// </summary>
        public DataFormat DataType { get; set; }

        public DataTransmissionPacket() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataTransmissionPacket"/> class with specified data and data format.
        /// </summary>
        /// <param name="data">The data payload within the packet.</param>
        /// <param name="dataType">Denotes the type of data contained within the data payload.</param>
        /// <remarks>Upon instantiating an instance of the <see cref="DataTransmissionPacket"/> class, the <see cref="DataType"/> parameter is used to wrap the raw data with corresponding tags so it can be parsed and more easily managed throughout its life cycle.</remarks>
        public DataTransmissionPacket(byte[] data, DataFormat dataType)
            {
                {
                var tags = DataFormatTagMap[dataType];

                var combinedData = new byte[data.Length + tags.OpeningTag.Length + tags.ClosingTag.Length];

                Encoding.UTF8.GetBytes(tags.OpeningTag).CopyTo(combinedData, 0);
                data.CopyTo(combinedData, tags.OpeningTag.Length);
                Encoding.UTF8.GetBytes(tags.ClosingTag).CopyTo(combinedData, tags.OpeningTag.Length + data.Length);

                this.Data = combinedData;
                this.DataType = dataType;
                }
            }

        /// <summary>
        /// Returns a string representation of the data transmission packet.
        /// </summary>
        /// <returns>A string representation of the data transmission packet.</returns>
        public override string ToString()
            {
            return $"{DataFormatTagMap[DataType].OpeningTag}{Data}{DataFormatTagMap[DataType].ClosingTag}";
            }
        }
    }