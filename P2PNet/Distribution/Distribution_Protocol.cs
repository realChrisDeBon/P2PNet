using P2PNet.NetworkPackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;

namespace P2PNet.Distribution
    {
    public static class Distribution_Protocol
        {
        public struct MessageTags
            {
            public string OpeningTag;
            public string ClosingTag;
            }

        #region Packet protocol
        public readonly static Dictionary<PacketType, MessageTags> PacketTagMap = new Dictionary<PacketType, MessageTags>()
        {
            { PacketType.IdentityPacket, new MessageTags() { OpeningTag = "<Identity>", ClosingTag = "</Identity>" } },
            { PacketType.DisconnectPacket, new MessageTags() { OpeningTag = "<Disconnect>", ClosingTag = "</Disconnect>" } },
            { PacketType.PeerGroupPacket, new MessageTags() { OpeningTag = "<PeerGroup>", ClosingTag = "</PeerGroup>" } },
            { PacketType.DataTransmissionPacket, new MessageTags() { OpeningTag = "<DataTransmit>", ClosingTag = "</DataTransmit>" } },
            { PacketType.PureMessage, new MessageTags() {OpeningTag = "<Message>", ClosingTag = "</Message>"} }
        }; // Recognized packet types
        public struct PacketTypeRelay
            {
            public PacketType packetType;
            public string Data;
            }
        public enum PacketType
            {
            IdentityPacket,
            DisconnectPacket,
            PeerGroupPacket,
            DataTransmissionPacket,
            PureMessage,
            BADPACKET
            }
        public static void WrapPacket(PacketType packetType, ref string data)
            {
            string header = PacketTagMap[packetType].OpeningTag;
            string closer = PacketTagMap[packetType].ClosingTag;
            data = header + data + closer;
            }
        #endregion

        #region Data protocol
        public enum DataFormat
            {
            File,
            Task,
            }
        public readonly static Dictionary<DataFormat, MessageTags> DataFormatTagMap = new Dictionary<DataFormat, MessageTags>()
            {
                { DataFormat.File, new MessageTags() { OpeningTag = "<File>", ClosingTag = "</File>" } },
                { DataFormat.Task, new MessageTags() { OpeningTag = "<Task>", ClosingTag = "</Task>" } },
            };

        public static byte[] UnwrapData(DataTransmissionPacket packet)
            {

            // Get the tags
            var tags = DataFormatTagMap[packet.DataType];

            // Check if tags exist in the data
            var openingTagIndex = FindByteSequence(packet.Data, Encoding.UTF8.GetBytes(tags.OpeningTag));
            var closingTagIndex = FindByteSequence(packet.Data, Encoding.UTF8.GetBytes(tags.ClosingTag));

            if (openingTagIndex == -1 || closingTagIndex == -1 || openingTagIndex >= closingTagIndex)
                {
                // Invalid tag structure. Consider throwing an exception
                throw new InvalidDataException("Invalid tag structure in packet data.");
                }

            // Calculate the length of raw JSON data
            var jsonDataLength = closingTagIndex - (openingTagIndex + tags.OpeningTag.Length);

            // Extract the raw JSON byte array
            var rawJsonData = new byte[jsonDataLength];
            Array.Copy(packet.Data, openingTagIndex + tags.OpeningTag.Length, rawJsonData, 0, jsonDataLength);

            return rawJsonData;


            // Helper to find a sequence of bytes within another byte array
            int FindByteSequence(byte[] haystack, byte[] needle)
                {
                for (int i = 0; i <= haystack.Length - needle.Length; i++)
                    {
                    bool found = true;
                    for (int j = 0; j < needle.Length; j++)
                        {
                        if (haystack[i + j] != needle[j])
                            {
                            found = false;
                            break;
                            }
                        }
                    if (found)
                        return i;
                    }
                return -1; // Not found
                }
            }
        #endregion

        #region Serialization

        private static readonly Dictionary<Type, JsonTypeInfo> _serializerContexts = new Dictionary<Type, JsonTypeInfo>
        {
            { typeof(PureMessagePacket), new PureMessagePacketContext().GetTypeInfo(typeof(PureMessagePacket)) },
            { typeof(IdentifierPacket), new IdentifierPacketContext().GetTypeInfo(typeof(IdentifierPacket)) },
            { typeof(CollectionSharePacket), new CollectionSharePacketContext().GetTypeInfo(typeof(CollectionSharePacket)) },
            { typeof(DataTransmissionPacket), new DataTransmissionPacketContext().GetTypeInfo(typeof(DataTransmissionPacket)) },
        };

        internal static string Serialize<T>(T obj)
            {
            if (!_serializerContexts.TryGetValue(typeof(T), out var context))
                {
                throw new ArgumentException($"No serializer context found for type {typeof(T)}");
                }

            return System.Text.Json.JsonSerializer.Serialize(obj, context);
            }

        internal static T Deserialize<T>(string json)
            {
            if (!_serializerContexts.TryGetValue(typeof(T), out var context))
                {
                throw new ArgumentException($"No serializer context found for type {typeof(T)}");
                }

            return System.Text.Json.JsonSerializer.Deserialize<T>(json, (JsonTypeInfo<T>)context);
            }
        #endregion
        }


        #region PACKET_CONTEXT
        [JsonSerializable(typeof(PureMessagePacket))]
        public partial class PureMessagePacketContext : JsonSerializerContext { }

        [JsonSerializable(typeof(IdentifierPacket))]
        public partial class IdentifierPacketContext : JsonSerializerContext { }

        [JsonSerializable(typeof(CollectionSharePacket))]
        public partial class CollectionSharePacketContext : JsonSerializerContext { }

        [JsonSerializable(typeof(DataTransmissionPacket))]
        public partial class DataTransmissionPacketContext : JsonSerializerContext { }
        #endregion


        
    }